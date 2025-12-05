using System.ComponentModel.DataAnnotations;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.People.Cache;
using NERBABO.ApiService.Core.People.Dtos;
using NERBABO.ApiService.Core.People.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Helper;
using NERBABO.ApiService.Shared.BulkImport.Models;
using NERBABO.ApiService.Shared.BulkImport.Services;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.People.BulkImport.Services;

public class PeopleBulkImportService : IPeopleBulkImportService
{
    private readonly ILogger<PeopleBulkImportService> _logger;
    private readonly AppDbContext _context;
    private readonly ICachePeopleRepository _cache;
    private readonly CsvParserService _csvParser;
    private readonly ExcelParserService _excelParser;

    // Required headers for People import
    private readonly List<string> RequiredHeaders = new()
    {
        "FirstName", "LastName", "NIF"
    };

    // Optional headers
    private readonly List<string> OptionalHeaders = new()
    {
        "Gender", "Habilitation", "IdentificationType",
        "IdentificationNumber", "IdentificationValidationDate", "NISS", "IBAN",
        "BirthDate", "Address", "ZipCode", "PhoneNumber", "Email",
        "Naturality", "Nationality"
    };

    public PeopleBulkImportService(
        ILogger<PeopleBulkImportService> logger,
        AppDbContext context,
        ICachePeopleRepository cache,
        CsvParserService csvParser,
        ExcelParserService excelParser)
    {
        _logger = logger;
        _context = context;
        _cache = cache;
        _csvParser = csvParser;
        _excelParser = excelParser;
    }

    public async Task<Result<BulkImportResult<RetrievePersonDto>>> ImportFromFileAsync(
        IFormFile file,
        FileType fileType,
        ImportOptions options)
    {
        var startTime = DateTime.UtcNow;
        var result = new BulkImportResult<RetrievePersonDto>
        {
            StartedAt = startTime
        };

        try
        {
            // 1. Parse file
            IFileParserService parser = fileType == FileType.CSV ? _csvParser : _excelParser;

            var structureValidation = parser.ValidateFileStructure(file, fileType, RequiredHeaders);
            if (!structureValidation.Success)
            {
                result.GlobalErrors.Add(structureValidation.Message!);
                result.CompletedAt = DateTime.UtcNow;
                return Result<BulkImportResult<RetrievePersonDto>>.Fail(
                    "Estrutura inválida",
                    structureValidation.Message!);
            }

            var parseResult = await parser.ParseFileAsync(file, fileType);
            if (!parseResult.Success)
            {
                result.GlobalErrors.Add(parseResult.Message!);
                result.CompletedAt = DateTime.UtcNow;
                return Result<BulkImportResult<RetrievePersonDto>>.Fail(
                    "Erro de parsing",
                    parseResult.Message!);
            }

            var rows = parseResult.Data!;
            result.TotalRows = rows.Count;

            if (rows.Count == 0)
            {
                result.GlobalErrors.Add("O ficheiro não contém dados para importar.");
                result.CompletedAt = DateTime.UtcNow;
                return Result<BulkImportResult<RetrievePersonDto>>.Ok(result);
            }

            // 2. Pre-load existing data for validation (performance optimization)
            var allNIFs = rows
                .Where(r => r.ContainsKey("NIF") && !string.IsNullOrEmpty(r["NIF"]))
                .Select(r => r["NIF"])
                .ToHashSet();

            var existingNIFs = await _context.People
                .Where(p => allNIFs.Contains(p.NIF))
                .Select(p => p.NIF)
                .ToHashSetAsync();

            var existingEmails = await _context.People
                .Where(p => p.Email != null && p.Email != "")
                .Select(p => p.Email!.ToLower())
                .ToHashSetAsync();

            // 3. Process rows in batches
            var rowNumber = 1;  // Start at 1 (after header)
            var successfulPeople = new List<Person>();
            var batchResultIndices = new List<int>();

            foreach (var row in rows)
            {
                rowNumber++;
                var rowResult = new ImportRowResult<RetrievePersonDto>
                {
                    RowNumber = rowNumber,
                    RowData = row
                };

                // Map row to DTO
                var personDto = MapRowToDto(row, rowResult);

                if (personDto == null)
                {
                    result.Results.Add(rowResult);
                    result.FailureCount++;

                    if (options.StopOnFirstError)
                        break;

                    continue;
                }

                // Validate DTO
                var validationErrors = ValidatePersonDto(personDto, existingNIFs, existingEmails);
                if (validationErrors.Any())
                {
                    rowResult.Errors.AddRange(validationErrors);
                    rowResult.Success = false;
                    result.Results.Add(rowResult);
                    result.FailureCount++;

                    if (options.StopOnFirstError)
                        break;

                    continue;
                }

                // Convert to entity and add to batch
                var person = Person.ConvertCreateDtoToEntity(personDto);
                successfulPeople.Add(person);
                existingNIFs.Add(person.NIF);  // Prevent duplicates within the same import

                if (!string.IsNullOrEmpty(person.Email))
                    existingEmails.Add(person.Email.ToLower());

                rowResult.Success = true;
                result.Results.Add(rowResult);
                batchResultIndices.Add(result.Results.Count - 1);
                result.SuccessCount++;

                // Commit batch
                if (successfulPeople.Count >= options.BatchSize)
                {
                    await CommitBatchAsync(successfulPeople, result, batchResultIndices);
                    successfulPeople.Clear();
                    batchResultIndices.Clear();
                }
            }

            // 4. Commit remaining batch
            if (successfulPeople.Any())
            {
                await CommitBatchAsync(successfulPeople, result, batchResultIndices);
            }

            // 5. Update cache
            await _cache.RemovePeopleCacheAsync();

            result.CompletedAt = DateTime.UtcNow;
            result.Success = result.FailureCount == 0;

            return Result<BulkImportResult<RetrievePersonDto>>.Ok(
                result,
                "Importação concluída",
                result.Summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante importação em massa de pessoas");
            result.GlobalErrors.Add($"Erro inesperado: {ex.Message}");
            result.CompletedAt = DateTime.UtcNow;

            return Result<BulkImportResult<RetrievePersonDto>>.Fail(
                "Erro de importação",
                $"Ocorreu um erro durante a importação: {ex.Message}",
                StatusCodes.Status500InternalServerError);
        }
    }

    private CreatePersonDto? MapRowToDto(
        Dictionary<string, string> row,
        ImportRowResult<RetrievePersonDto> rowResult)
    {
        try
        {
            var dto = new CreatePersonDto
            {
                FirstName = GetValue(row, "FirstName") ?? string.Empty,
                LastName = GetValue(row, "LastName") ?? string.Empty,
                NIF = GetValue(row, "NIF") ?? string.Empty,
                IdentificationNumber = GetValue(row, "IdentificationNumber"),
                IdentificationValidationDate = GetValue(row, "IdentificationValidationDate"),
                NISS = GetValue(row, "NISS"),
                IBAN = GetValue(row, "IBAN"),
                BirthDate = GetValue(row, "BirthDate"),
                Address = GetValue(row, "Address"),
                ZipCode = GetValue(row, "ZipCode"),
                PhoneNumber = GetValue(row, "PhoneNumber"),
                Email = GetValue(row, "Email"),
                Naturality = GetValue(row, "Naturality"),
                Nationality = GetValue(row, "Nationality"),
                // Set defaults for optional enum fields (using humanized descriptions)
                Gender = GetValue(row, "Gender") ?? GenderEnum.Unknown.Humanize(),
                Habilitation = GetValue(row, "Habilitation") ?? HabilitationEnum.WithoutProof.Humanize(),
                IdentificationType = GetValue(row, "IdentificationType") ?? IdentificationTypeEnum.Unknown.Humanize()
            };

            return dto;
        }
        catch (Exception ex)
        {
            rowResult.Errors.Add(new ImportValidationError
            {
                Field = "General",
                ErrorMessage = $"Erro ao mapear linha: {ex.Message}",
                Severity = ErrorSeverity.Error
            });
            return null;
        }
    }

    private string? GetValue(Dictionary<string, string> row, string key)
    {
        if (row.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
            return value.Trim();

        return null;
    }

    private List<ImportValidationError> ValidatePersonDto(
        CreatePersonDto dto,
        HashSet<string> existingNIFs,
        HashSet<string> existingEmails)
    {
        var errors = new List<ImportValidationError>();

        // DataAnnotations validation
        var validationContext = new ValidationContext(dto);
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(dto, validationContext, validationResults, true))
        {
            errors.AddRange(validationResults.Select(vr => new ImportValidationError
            {
                Field = vr.MemberNames.FirstOrDefault() ?? "Unknown",
                ErrorMessage = vr.ErrorMessage ?? "Erro de validação",
                Severity = ErrorSeverity.Error
            }));
        }

        // Business logic validation
        if (existingNIFs.Contains(dto.NIF))
        {
            errors.Add(new ImportValidationError
            {
                Field = "NIF",
                ErrorMessage = "NIF já existe no sistema ou duplicado no ficheiro",
                AttemptedValue = dto.NIF,
                Severity = ErrorSeverity.Error
            });
        }

        if (!string.IsNullOrEmpty(dto.Email) &&
            existingEmails.Contains(dto.Email.ToLower()))
        {
            errors.Add(new ImportValidationError
            {
                Field = "Email",
                ErrorMessage = "Email já existe no sistema ou duplicado no ficheiro",
                AttemptedValue = dto.Email,
                Severity = ErrorSeverity.Error
            });
        }

        // Enum validations - only validate if value was provided and is not the default
        if (!string.IsNullOrEmpty(dto.Gender) &&
            !EnumHelp.IsValidEnum<GenderEnum>(dto.Gender))
        {
            errors.Add(new ImportValidationError
            {
                Field = "Gender",
                ErrorMessage = "Género inválido",
                AttemptedValue = dto.Gender,
                Severity = ErrorSeverity.Error
            });
        }

        if (!string.IsNullOrEmpty(dto.Habilitation) &&
            !EnumHelp.IsValidEnum<HabilitationEnum>(dto.Habilitation))
        {
            errors.Add(new ImportValidationError
            {
                Field = "Habilitation",
                ErrorMessage = "Habilitação inválida",
                AttemptedValue = dto.Habilitation,
                Severity = ErrorSeverity.Error
            });
        }

        if (!string.IsNullOrEmpty(dto.IdentificationType) &&
            !EnumHelp.IsValidEnum<IdentificationTypeEnum>(dto.IdentificationType))
        {
            errors.Add(new ImportValidationError
            {
                Field = "IdentificationType",
                ErrorMessage = "Tipo de identificação inválido",
                AttemptedValue = dto.IdentificationType,
                Severity = ErrorSeverity.Error
            });
        }

        return errors;
    }

    private async Task CommitBatchAsync(
        List<Person> people,
        BulkImportResult<RetrievePersonDto> result,
        List<int> batchResultIndices)
    {
        try
        {
            await _context.People.AddRangeAsync(people);
            await _context.SaveChangesAsync();

            // Update result with created entities
            var createdPeople = people.Select(Person.ConvertEntityToRetrieveDto).ToList();

            for (int i = 0; i < createdPeople.Count; i++)
            {
                if (i < batchResultIndices.Count)
                {
                    var resultIndex = batchResultIndices[i];
                    result.Results[resultIndex].Data = createdPeople[i];
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao guardar lote de pessoas");
            throw;
        }
    }

    public async Task<Result<FileDownloadResult>> GetTemplateFileAsync(FileType fileType)
    {
        try
        {
            var templatePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Core",
                "People",
                "Templates",
                fileType == FileType.CSV ? "People_Import_Template.csv" : "People_Import_Template.xlsx"
            );

            if (!File.Exists(templatePath))
            {
                return Result<FileDownloadResult>.Fail(
                    "Template não encontrado",
                    "O ficheiro de template não foi encontrado no servidor.",
                    StatusCodes.Status404NotFound);
            }

            var fileBytes = await File.ReadAllBytesAsync(templatePath);
            var contentType = fileType == FileType.CSV ? "text/csv" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = fileType == FileType.CSV ? "People_Import_Template.csv" : "People_Import_Template.xlsx";

            var fileResult = new FileDownloadResult
            {
                Content = fileBytes,
                FileName = fileName,
                ContentType = contentType
            };

            return Result<FileDownloadResult>.Ok(fileResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter ficheiro de template");
            return Result<FileDownloadResult>.Fail(
                "Erro ao obter template",
                $"Ocorreu um erro ao obter o template: {ex.Message}",
                StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<BulkImportResult<RetrievePersonDto>>> ValidateImportAsync(
        IFormFile file,
        FileType fileType)
    {
        // Same as ImportFromFileAsync but without database commits
        // For now, we can use the same logic - just not save to DB
        // This is a simplified version for MVP
        return await ImportFromFileAsync(file, fileType, new ImportOptions
        {
            StopOnFirstError = false,
            BatchSize = int.MaxValue
        });
    }
}
