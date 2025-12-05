using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Shared.BulkImport.Services;

public class CsvParserService : IFileParserService
{
    private readonly ILogger<CsvParserService> _logger;

    public CsvParserService(ILogger<CsvParserService> logger)
    {
        _logger = logger;
    }

    public async Task<Result<List<Dictionary<string, string>>>> ParseFileAsync(
        IFormFile file,
        FileType fileType)
    {
        if (fileType != FileType.CSV)
            return Result<List<Dictionary<string, string>>>
                .Fail("Tipo inválido", "Este serviço só processa ficheiros CSV.");

        try
        {
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",  // European CSV format
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                MissingFieldFound = null,  // Don't fail on missing fields
                BadDataFound = null        // Custom handling for bad data
            };

            using var csv = new CsvReader(reader, config);

            // Read records as dictionaries
            var records = new List<Dictionary<string, string>>();
            await csv.ReadAsync();
            csv.ReadHeader();

            var headers = csv.HeaderRecord;
            if (headers == null || headers.Length == 0)
                return Result<List<Dictionary<string, string>>>
                    .Fail("Cabeçalho inválido", "O ficheiro CSV não contém cabeçalhos.");

            while (await csv.ReadAsync())
            {
                var row = new Dictionary<string, string>();
                foreach (var header in headers)
                {
                    row[header] = csv.GetField(header) ?? string.Empty;
                }
                records.Add(row);
            }

            return Result<List<Dictionary<string, string>>>.Ok(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar ficheiro CSV");
            return Result<List<Dictionary<string, string>>>
                .Fail("Erro de Parsing", $"Erro ao ler ficheiro CSV: {ex.Message}");
        }
    }

    public Result ValidateFileStructure(
        IFormFile file,
        FileType fileType,
        List<string> requiredHeaders)
    {
        if (file == null || file.Length == 0)
            return Result.Fail("Ficheiro inválido", "Nenhum ficheiro foi fornecido.");

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            return Result.Fail("Extensão inválida", "O ficheiro deve ter extensão .csv");

        // Validate headers (read first line only)
        try
        {
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true
            };

            using var csv = new CsvReader(reader, config);
            csv.Read();
            csv.ReadHeader();

            var fileHeaders = csv.HeaderRecord?.ToList() ?? new List<string>();
            var missingHeaders = requiredHeaders
                .Where(rh => !fileHeaders.Contains(rh, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (missingHeaders.Any())
                return Result.Fail(
                    "Cabeçalhos em falta",
                    $"Os seguintes cabeçalhos obrigatórios estão em falta: {string.Join(", ", missingHeaders)}");

            return Result.Ok("Válido", "Estrutura do ficheiro está válida.");
        }
        catch (Exception ex)
        {
            return Result.Fail("Erro de validação", $"Erro ao validar estrutura: {ex.Message}");
        }
    }
}
