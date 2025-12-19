using System.Linq.Expressions;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Notifications.Services;
using NERBABO.ApiService.Core.People.Cache;
using NERBABO.ApiService.Core.People.Dtos;
using NERBABO.ApiService.Core.People.Models;
using NERBABO.ApiService.Core.Reports.Models;
using NERBABO.ApiService.Core.Reports.Services;
using NERBABO.ApiService.Core.Students.Cache;
using NERBABO.ApiService.Core.Teachers.Cache;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Helper;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;
using ZLinq;

namespace NERBABO.ApiService.Core.People.Services;

public class PeopleService(
        ILogger<PeopleService> logger,
        AppDbContext context,
        UserManager<User> userManager,
        ICachePeopleRepository cache,
        ICacheStudentsRepository cacheStudents,
        ICacheTeacherRepository cacheTeacher,
        IPdfService pdfService,
        INotificationService notificationService

    ) : IPeopleService
{
    private readonly ILogger<PeopleService> _logger = logger;
    private readonly AppDbContext _context = context;
    private readonly UserManager<User> _userManager = userManager;
    private readonly ICachePeopleRepository _cache = cache;
    private readonly ICacheStudentsRepository _cacheStudents = cacheStudents;
    private readonly ICacheTeacherRepository _cacheTeacher = cacheTeacher;
    private readonly IPdfService _pdfService = pdfService;
    private readonly INotificationService _notificationService = notificationService;

    public async Task<Result<RetrievePersonDto>> CreateAsync(CreatePersonDto entityDto)
    {
        // Unique constraints validation
        if (await _context.People.AnyAsync(p => p.NIF == entityDto.NIF))
        {
            _logger.LogWarning("Duplicated NIF try detected.");
            return Result<RetrievePersonDto>
                .Fail("Erro de Validação.", "O NIF da pessoa deve ser único. Já existe no sistema.");
        }

        if (!string.IsNullOrEmpty(entityDto.NISS) &&
            await _context.People.AnyAsync(p => p.NISS == entityDto.NISS))
        {
            _logger.LogWarning("Duplicated NISS try detected.");
            return Result<RetrievePersonDto>
                .Fail("Erro de Validação.", "O NISS da pessoa deve ser único. Já existe no sistema.");
        }

        if (!string.IsNullOrEmpty(entityDto.IdentificationNumber)
            && await _context.People.AnyAsync(p =>
            (p.IdentificationNumber ?? "").ToLower()
            .Equals(entityDto.IdentificationNumber.ToLower()))
            )
        {
            _logger.LogWarning("Duplicated IdentificationNumber try detected.");
            return Result<RetrievePersonDto>
                .Fail("Erro de Validação.", "O Número de Identificação da pessoa deve ser único. Já existe no sistema.");
        }

        if (!string.IsNullOrEmpty(entityDto.Email)
            && await _context.People.AnyAsync(p =>
            (p.Email ?? "").ToLower()
            .Equals(entityDto.Email.ToLower()))
            )
        {
            _logger.LogWarning("Duplicated Email try detected.");
            return Result<RetrievePersonDto>
                .Fail("Erro de Validação.", "O Email da pessoa deve ser único. Já existe no sistema.");
        }

        // Enum validation
        if (string.IsNullOrEmpty(entityDto.Gender)
            || !EnumHelp.IsValidEnum<GenderEnum>(entityDto.Gender))
        {
            return Result<RetrievePersonDto>
                .Fail("Não encontrado", "Género não encontrado.",
                StatusCodes.Status404NotFound);
        }

        if (string.IsNullOrEmpty(entityDto.Habilitation)
            || !EnumHelp.IsValidEnum<HabilitationEnum>(entityDto.Habilitation))
        {
            return Result<RetrievePersonDto>
                .Fail("Não encontrado", "Tipo de Habilitações não encontrado.",
                StatusCodes.Status404NotFound);
        }

        if (string.IsNullOrEmpty(entityDto.IdentificationType)
            || !EnumHelp.IsValidEnum<IdentificationTypeEnum>(entityDto.IdentificationType))
        {
            return Result<RetrievePersonDto>
                .Fail("Não encontrado", "Tipo de Identificação não encontrado.",
                StatusCodes.Status404NotFound);
        }

        // create person on database
        var createdPerson = _context.People.Add(Person.ConvertCreateDtoToEntity(entityDto));
        await _context.SaveChangesAsync();

        var personToRetrieve = Person.ConvertEntityToRetrieveDto(createdPerson.Entity);

        // Update cache
        await RemoveRelatedCache();
        await _cache.SetSinglePersonCacheAsync(personToRetrieve);


        return Result<RetrievePersonDto>
            .Ok(personToRetrieve,
            "Pessoa Criada.", $"Foi criada uma pessoa com o nome {personToRetrieve.FullName}.",
            StatusCodes.Status201Created);
    }

    public async Task<Result<RetrievePersonDto>> CreateWithFilesAsync(
        CreatePersonDto entityDto,
        IFormFile? habilitationPdf,
        IFormFile? ibanPdf,
        IFormFile? identificationDocumentPdf,
        string userId)
    {
        // First create the person using existing validation logic
        var createResult = await CreateAsync(entityDto);
        if (!createResult.Success)
        {
            return createResult;
        }

        var createdPerson = createResult.Data!;
        var personId = createdPerson.Id;

        try
        {
            // Upload habilitation PDF if provided
            if (habilitationPdf != null && habilitationPdf.Length > 0)
            {
                var habilitationResult = await UploadPdfFileAsync(
                    personId,
                    habilitationPdf,
                    PdfTypes.HabilitationComprovative,
                    userId);

                if (habilitationResult.Success)
                {
                    var person = await _context.People.FindAsync(personId);
                    if (person != null)
                    {
                        person.HabilitationComprovativePdfId = habilitationResult.Data!.Id;
                    }
                }
            }

            // Upload IBAN PDF if provided
            if (ibanPdf != null && ibanPdf.Length > 0)
            {
                var ibanResult = await UploadPdfFileAsync(
                    personId,
                    ibanPdf,
                    PdfTypes.IbanComprovative,
                    userId);

                if (ibanResult.Success)
                {
                    var person = await _context.People.FindAsync(personId);
                    if (person != null)
                    {
                        person.IbanComprovativePdfId = ibanResult.Data!.Id;
                    }
                }
            }

            // Upload identification document PDF if provided
            if (identificationDocumentPdf != null && identificationDocumentPdf.Length > 0)
            {
                var identificationResult = await UploadPdfFileAsync(
                    personId,
                    identificationDocumentPdf,
                    PdfTypes.IdentificationDocument,
                    userId);

                if (identificationResult.Success)
                {
                    var person = await _context.People.FindAsync(personId);
                    if (person != null)
                    {
                        person.IdentificationDocumentPdfId = identificationResult.Data!.Id;
                    }
                }
            }

            // Save all changes
            await _context.SaveChangesAsync();

            // Refresh the person data to include updated PDF IDs
            var updatedPerson = await _context.People.FindAsync(personId);
            if (updatedPerson != null)
            {
                var retrievePerson = Person.ConvertEntityToRetrieveDto(updatedPerson);
                await _cache.SetSinglePersonCacheAsync(retrievePerson);

                // Generate notifications after PDF uploads
                await _notificationService.GenerateNotificationsAsync();

                return Result<RetrievePersonDto>
                    .Ok(retrievePerson,
                    "Pessoa Criada.", $"Foi criada uma pessoa com o nome {retrievePerson.FullName} e os ficheiros foram carregados.",
                    StatusCodes.Status201Created);
            }

            return createResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading files during person creation for person {PersonId}", personId);
            return Result<RetrievePersonDto>
                .Fail("Erro.", "Pessoa criada mas ocorreu um erro ao carregar os ficheiros.",
                    StatusCodes.Status500InternalServerError);
        }
    }

    private async Task<Result<SavedPdf>> UploadPdfFileAsync(long personId, IFormFile file, string pdfType, string userId)
    {
        // Validate file
        if (file is null || file.Length == 0)
        {
            return Result<SavedPdf>
                .Fail("Arquivo inválido.", "Nenhum arquivo foi fornecido ou o arquivo está vazio.",
                    StatusCodes.Status400BadRequest);
        }

        if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
        {
            return Result<SavedPdf>
                .Fail("Tipo de arquivo inválido.", "Apenas arquivos PDF são permitidos.",
                    StatusCodes.Status400BadRequest);
        }

        // Read file content
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var pdfContent = memoryStream.ToArray();

        // Save PDF using PdfService
        return await _pdfService.SavePdfAsync(pdfType, personId, pdfContent, userId);
    }

    public async Task<Result> DeleteAsync(long id)
    {
        // Check if person exists
        var existingPerson = await _context.People
            .FindAsync(id);
        if (existingPerson is null)
            return Result
                .Fail("Não encontrado.", $"Pessoa não encontrada.",
                StatusCodes.Status404NotFound);

        // Check if person is associated with a user
        if (await _userManager.Users.AnyAsync(u => u.PersonId == id))
        {
            _logger.LogWarning("Failed to delete the person with ID {personId} as colaborator is a User", id);
            return Result
                .Fail("Erro de Validação.", "Não pode eliminar uma pessoa que é um utilizador.");
        }

        // get and validate teacher deletion
        var existingTeacher = await _context.Teachers
            .FirstOrDefaultAsync(t => t.PersonId == id);
        if (existingTeacher is not null && !existingTeacher.CanDelete)
        {
            _logger.LogWarning("Failed to delete the person with ID {personId} as teacher has instances on ModuleTeachings.", id);
            return Result
                .Fail("Erro de Validação.", "Não pode eliminar uma pessoa que é um Formador que já lecionou módulos em ações.");
        }

        // get and validate student deletion
        var existingStudent = await _context.Students.FirstOrDefaultAsync(t => t.PersonId == id);
        if (existingStudent is not null && existingStudent.CanDelete)
        {
            _logger.LogWarning("Failed to delete the person with ID {personId} as student has instances on ActionEnrollments.", id);
            return Result
                .Fail("Erro de Validação.", "Não pode eliminar uma pessoa que é um Formando que está inscrito em ações.");
        }

        // pdfs
        var habilitationComprovative = await _pdfService.GetSavedPdfAsync(PdfTypes.HabilitationComprovative, existingPerson.Id);
        var ibanComprovative = await _pdfService.GetSavedPdfAsync(PdfTypes.IbanComprovative, existingPerson.Id);
        var identificationDocument = await _pdfService.GetSavedPdfAsync(PdfTypes.IdentificationDocument, existingPerson.Id);


        var transaction = _context.Database.BeginTransaction();
        try
        {
            if (existingTeacher is not null)
                _context.Teachers.Remove(existingTeacher);

            if (existingStudent is not null)
                _context.Students.Remove(existingStudent);

            // remove pdf files from database and from wwwroot
            if (habilitationComprovative.Data is not null)
                await _pdfService.DeleteSavedPdfAsync(habilitationComprovative.Data.Id);

            if (ibanComprovative.Data is not null)
                await _pdfService.DeleteSavedPdfAsync(ibanComprovative.Data.Id);

            if (identificationDocument.Data is not null)
                await _pdfService.DeleteSavedPdfAsync(identificationDocument.Data.Id);
            
            // remove from database
            _context.People.Remove(existingPerson);
            await _context.SaveChangesAsync();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
        finally
        {
            await transaction.CommitAsync();
        }

        // Update cache
        await RemoveRelatedCache(id);

        return Result
            .Ok("Pessoa Eliminada.", "Pessoa eliminada com sucesso.");
    }

    public async Task<Result<IEnumerable<RetrievePersonDto>>> GetAllAsync()
    {
        // Check if entry exists in cache
        var cachedPeople = await _cache.GetCacheAllPeopleAsync();
        if (cachedPeople is not null && cachedPeople.ToList().Count > 0)
            return Result<IEnumerable<RetrievePersonDto>>
                .Ok(cachedPeople);

        // Not in cache so fetch from database
        var existingPeople = _context.People
            .AsNoTracking()
            .Include(p => p.User)
            .Include(p => p.Teacher)
            .Include(p => p.Student)
            .OrderByDescending(p => p.UpdatedAt)
            .Select(p => Person.ConvertEntityToRetrieveDto(p))
            .ToList();

        // Check if data
        if (existingPeople is null || existingPeople.Count == 0)
            return Result<IEnumerable<RetrievePersonDto>>
                .Fail("Não encontrado.", "Não foram encontradas pessoas no sistema",
                StatusCodes.Status404NotFound);

        // update cache
        await _cache.SetAllPeopleCacheAsync(existingPeople);

        return Result<IEnumerable<RetrievePersonDto>>
            .Ok(existingPeople);
    }

    public async Task<Result<IEnumerable<RetrievePersonDto>>> GetAllWithoutProfileAsync(string profile)
    {
        // Validate and parse the profile input
        if (string.IsNullOrWhiteSpace(profile) || !Enum.TryParse<ProfilesEnum>(profile.Humanize(LetterCasing.Title), true, out var profileEnum))
        {
            return Result<IEnumerable<RetrievePersonDto>>
                .Fail("Perfil inválido.", $"O perfil '{profile}' não é válido.", StatusCodes.Status400BadRequest);
        }

        // Check if data entries exist in cache
        var cachedPeople = await _cache.GetCachePeopleWithoutProfileAsync(profile);
        if (cachedPeople is not null && cachedPeople.ToList().Count > 0)
        {
            return Result<IEnumerable<RetrievePersonDto>>
                .Ok(cachedPeople);
        }

        // Fetch from database
        var existingPeople = await _context.People
            .AsNoTracking()
            .Include(p => p.User)
            .Include(p => p.Teacher)
            .Include(p => p.Student)
            .ToListAsync();

        // Filter people without the specified profile
        Expression<Func<Person, bool>> filter = profileEnum switch
        {
            ProfilesEnum.Colaborator => p => !p.IsColaborator,
            ProfilesEnum.Student => p => !p.IsStudent,
            ProfilesEnum.Teacher => p => !p.IsTeacher,
            _ => throw new ArgumentOutOfRangeException(nameof(profileEnum), "Perfil não suportado.")
        };

        var peopleWithoutProfile = existingPeople
            .AsValueEnumerable()
            .Where(filter.Compile())
            .OrderByDescending(p => p.UpdatedAt)
            .Select(Person.ConvertEntityToRetrieveDto)
            .ToList();

        // Check if data exists
        if (peopleWithoutProfile.Count == 0)
        {
            return Result<IEnumerable<RetrievePersonDto>>
                .Fail("Não encontrado.", $"Não foram encontradas pessoas sem o perfil {profile}.", StatusCodes.Status404NotFound);
        }

        // Update cache
        await _cache.SetPeopleWithoutProfileCacheAsync(peopleWithoutProfile, profile);

        return Result<IEnumerable<RetrievePersonDto>>.Ok(peopleWithoutProfile);
    }

    public async Task<Result<RetrievePersonDto>> GetByIdAsync(long id)
    {
        // Check if entry exists in cache
        var cachedPerson = await _cache.GetSinglePersonCacheAsync(id);
        if (cachedPerson is not null)
            return Result<RetrievePersonDto>.Ok(cachedPerson);

        // Not in cache so fetch from database
        var existingPerson = await _context.People
                .AsNoTracking()
                .Include(p => p.User)
                .Include(p => p.Teacher)
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.Id == id);

        if (existingPerson is null)
            return Result<RetrievePersonDto>
                .Fail("Não encontrado.", "Pessoa não encontrada.",
                StatusCodes.Status404NotFound);

        var retrievePerson = Person.ConvertEntityToRetrieveDto(existingPerson);

        // update cache
        await _cache.SetSinglePersonCacheAsync(retrievePerson);

        return Result<RetrievePersonDto>
            .Ok(Person.ConvertEntityToRetrieveDto(existingPerson));
    }

    public async Task<Result<RelationshipPersonDto>> GetPersonRelationshipsAsync(long personId)
    {
        var existingPerson = await _context.People
            .AsNoTracking()
            .Include(p => p.Teacher)
            .Include(p => p.User)
            .Include(p => p.Student)
            .FirstOrDefaultAsync(p => p.Id == personId);
        if (existingPerson is null)
            return Result<RelationshipPersonDto>
                .Fail("Não encontrado.", "Pessoa não encontrada.");

        var relationship = new RelationshipPersonDto(existingPerson);

        return Result<RelationshipPersonDto>
            .Ok(relationship);
    }

    public async Task<Result<RetrievePersonDto>> UpdateAsync(UpdatePersonDto entityDto)
    {
        var existingPerson = await _context.People
            .Include(p => p.Teacher)
            .Include(p => p.Student)
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == entityDto.Id);
        if (existingPerson is null)
            return Result<RetrievePersonDto>
                .Fail("Não encontrado.", "Pessoa não encontrada.",
                StatusCodes.Status404NotFound);

        // Check if NIF is unique
        if (await _context.People.AnyAsync(p => p.NIF == entityDto.NIF
            && p.Id != existingPerson.Id))
        {
            _logger.LogWarning("Duplicated NIF try detected.");
            return Result<RetrievePersonDto>
                .Fail("Erro de Validação.", "O NIF da pessoa deve ser único. Já existe no sistema.");
        }

        // Check if NISS is unique
        if (!string.IsNullOrEmpty(entityDto.NISS)
            && await _context.People.AnyAsync(p => p.NISS == entityDto.NISS
            && p.Id != existingPerson.Id))
        {
            _logger.LogWarning("Duplicated NISS try detected.");
            return Result<RetrievePersonDto>
                .Fail("Erro de Validação.", "O NISS da pessoa deve ser único. Já existe no sistema.");
        }

        // Check if IdentificationNumber is unique
        if (!string.IsNullOrEmpty(entityDto.IdentificationNumber)
            && await _context.People.AnyAsync(p =>
            (p.IdentificationNumber ?? "").ToLower()
            .Equals(entityDto.IdentificationNumber)
            && p.Id != existingPerson.Id))
        {
            _logger.LogWarning("Duplicated IdentificationNumber try detected.");
            return Result<RetrievePersonDto>
                .Fail("Erro de Validação.", "O Numero de Identificação da pessoa deve ser único. Já existe no sistema.");
        }

        // Check if Email is unique
        if (!string.IsNullOrEmpty(entityDto.Email)
            && await _context.People.AnyAsync(p =>
            (p.Email ?? "").ToLower()
            .Equals(entityDto.Email.ToLower())
            && p.Id != existingPerson.Id))
        {
            _logger.LogWarning("Duplicated Email try detected.");
            return Result<RetrievePersonDto>
                .Fail("Erro de Validação.", "O Email da pessoa deve ser único. Já existe no sistema.");
        }

        // check if Gender is a valid GenderEnum
        if (!string.IsNullOrEmpty(entityDto.Gender)
            && !EnumHelp.IsValidEnum<GenderEnum>(entityDto.Gender))
        {
            return Result<RetrievePersonDto>
                .Fail("Não encontrado", "Género não encontrado",
                StatusCodes.Status404NotFound);
        }

        // Check if Habilitation is a valid HabilitationEnum
        if (!string.IsNullOrEmpty(entityDto.Habilitation)
            && !EnumHelp.IsValidEnum<HabilitationEnum>(entityDto.Habilitation))
        {
            return Result<RetrievePersonDto>
                .Fail("Não encontrado", "Tipo de Habilitações não encontrado",
                StatusCodes.Status404NotFound);
        }

        // Check if IdentificationType is a valid IdentificationTypeEnum
        if (!string.IsNullOrEmpty(entityDto.IdentificationType)
            && !EnumHelp.IsValidEnum<IdentificationTypeEnum>(entityDto.IdentificationType))
        {
            return Result<RetrievePersonDto>
                .Fail("Não encontrado", "Tipo de Identificação não encontrado",
                StatusCodes.Status404NotFound);
        }

        // Selective field updates - only update fields that have changed
        bool hasChanges = false;

        // Update FirstName if changed
        if (!string.Equals(existingPerson.FirstName, entityDto.FirstName))
        {
            existingPerson.FirstName = entityDto.FirstName;
            hasChanges = true;
        }

        // Update LastName if changed
        if (!string.Equals(existingPerson.LastName, entityDto.LastName))
        {
            existingPerson.LastName = entityDto.LastName;
            hasChanges = true;
        }

        // Update NIF if changed
        if (!string.Equals(existingPerson.NIF, entityDto.NIF))
        {
            existingPerson.NIF = entityDto.NIF;
            hasChanges = true;
        }

        // Update IdentificationNumber if changed
        if (!string.Equals(existingPerson.IdentificationNumber, entityDto.IdentificationNumber))
        {
            existingPerson.IdentificationNumber = entityDto.IdentificationNumber;
            hasChanges = true;
        }

        // Update IdentificationValidationDate if changed
        var newIdentificationValidationDate = Helper.StringDateOnlyConverter.ConvertToDateOnly(entityDto.IdentificationValidationDate);
        if (existingPerson.IdentificationValidationDate != newIdentificationValidationDate)
        {
            existingPerson.IdentificationValidationDate = newIdentificationValidationDate;
            hasChanges = true;
        }

        // Update NISS if changed
        if (!string.Equals(existingPerson.NISS, entityDto.NISS))
        {
            existingPerson.NISS = entityDto.NISS;
            hasChanges = true;
        }

        // Update IBAN if changed
        if (!string.Equals(existingPerson.IBAN, entityDto.IBAN))
        {
            existingPerson.IBAN = entityDto.IBAN;
            hasChanges = true;
        }

        // Update BirthDate if changed
        var newBirthDate = Helper.StringDateOnlyConverter.ConvertToDateOnly(entityDto.BirthDate);
        if (existingPerson.BirthDate != newBirthDate)
        {
            existingPerson.BirthDate = newBirthDate;
            hasChanges = true;
        }

        // Update Address if changed
        if (!string.Equals(existingPerson.Address, entityDto.Address))
        {
            existingPerson.Address = entityDto.Address;
            hasChanges = true;
        }

        // Update ZipCode if changed
        if (!string.Equals(existingPerson.ZipCode, entityDto.ZipCode))
        {
            existingPerson.ZipCode = entityDto.ZipCode;
            hasChanges = true;
        }

        // Update PhoneNumber if changed
        if (!string.Equals(existingPerson.PhoneNumber, entityDto.PhoneNumber))
        {
            existingPerson.PhoneNumber = entityDto.PhoneNumber;
            hasChanges = true;
        }

        // Update Email if changed
        if (!string.Equals(existingPerson.Email, entityDto.Email))
        {
            existingPerson.Email = entityDto.Email;
            hasChanges = true;
        }

        // Update Naturality if changed
        if (!string.Equals(existingPerson.Naturality, entityDto.Naturality))
        {
            existingPerson.Naturality = entityDto.Naturality;
            hasChanges = true;
        }

        // Update Nationality if changed
        if (!string.Equals(existingPerson.Nationality, entityDto.Nationality))
        {
            existingPerson.Nationality = entityDto.Nationality;
            hasChanges = true;
        }

        // Update Gender if changed
        var newGender = entityDto.Gender.DehumanizeTo<GenderEnum>();
        if (existingPerson.Gender != newGender)
        {
            existingPerson.Gender = newGender;
            hasChanges = true;
        }

        // Update Habilitation if changed
        var newHabilitation = entityDto.Habilitation.DehumanizeTo<HabilitationEnum>();
        if (existingPerson.Habilitation != newHabilitation)
        {
            existingPerson.Habilitation = newHabilitation;
            hasChanges = true;
        }

        // Update IdentificationType if changed
        var newIdentificationType = entityDto.IdentificationType.DehumanizeTo<IdentificationTypeEnum>();
        if (existingPerson.IdentificationType != newIdentificationType)
        {
            existingPerson.IdentificationType = newIdentificationType;
            hasChanges = true;
        }

        // Return fail result if no changes were detected
        if (!hasChanges)
        {
            _logger.LogInformation("No changes detected for Person with ID {id}. No update performed.", entityDto.Id);
            return Result<RetrievePersonDto>
                .Fail("Nenhuma alteração detetada.", "Não foi alterado nenhum dado. Modifique os dados e tente novamente.",
                StatusCodes.Status400BadRequest);
        }

        // Update UpdatedAt and save changes
        existingPerson.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var retrievePerson = Person.ConvertEntityToRetrieveDto(existingPerson);

        // Update cache
        await RemoveRelatedCache(retrievePerson.Id);
        await _cache.SetSinglePersonCacheAsync(retrievePerson);

        return Result<RetrievePersonDto>
            .Ok(retrievePerson,
                "Pessoa Atualizada.",
                $"Foi atualizada a pessoa com o nome {existingPerson.FirstName} {existingPerson.LastName}.");
    }

    private async Task RemoveRelatedCache(long? id = null)
    {
        await _cache.RemovePeopleCacheAsync(id);
        await _cacheStudents.RemoveStudentsCacheAsync();
        await _cacheTeacher.RemoveTeacherCacheAsync();
    }

    public async Task<Result<RetrievePersonDto>> UploadHabilitationPdfAsync(long personId, IFormFile file, string userId)
    {
        try
        {
            // Validate person exists
            var person = await _context.People.FindAsync(personId);
            if (person is null)
            {
                return Result<RetrievePersonDto>
                    .Fail("Não encontrado.", "Pessoa não encontrada.",
                        StatusCodes.Status404NotFound);
            }

            // Validate file
            if (file is null || file.Length == 0)
            {
                return Result<RetrievePersonDto>
                    .Fail("Arquivo inválido.", "Nenhum arquivo foi fornecido ou o arquivo está vazio.",
                        StatusCodes.Status400BadRequest);
            }

            if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
            {
                return Result<RetrievePersonDto>
                    .Fail("Tipo de arquivo inválido.", "Apenas arquivos PDF são permitidos.",
                        StatusCodes.Status400BadRequest);
            }

            // Read file content
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var pdfContent = memoryStream.ToArray();

            // Save PDF using PdfService
            var savePdfResult = await _pdfService.SavePdfAsync(PdfTypes.HabilitationComprovative, personId, pdfContent, userId);
            if (!savePdfResult.Success)
            {
                return Result<RetrievePersonDto>
                    .Fail(savePdfResult.Title!, savePdfResult.Message!, savePdfResult.StatusCode!.Value);
            }

            // Update person's HabilitationComprovativePdfId
            person.HabilitationComprovativePdfId = savePdfResult.Data!.Id;
            person.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var retrievePerson = Person.ConvertEntityToRetrieveDto(person);

            // Update cache
            await RemoveRelatedCache(person.Id);
            await _cache.SetSinglePersonCacheAsync(retrievePerson);

            // Delete notifications related to this person's missing habilitation document
            await _notificationService.GenerateNotificationsAsync();

            return Result<RetrievePersonDto>
                .Ok(retrievePerson, "PDF carregado.", "PDF de certificado de habilitações carregado com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading habilitation PDF for person {PersonId}", personId);
            return Result<RetrievePersonDto>
                .Fail("Erro interno.", "Ocorreu um erro ao carregar o PDF.",
                    StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<FileDownloadResult>> GetHabilitationPdfAsync(long personId)
    {
        try
        {
            var person = await _context.People.FindAsync(personId);
            if (person is null)
            {
                return Result<FileDownloadResult>
                    .Fail("Não encontrado.", "Pessoa não encontrada.",
                        StatusCodes.Status404NotFound);
            }

            if (!person.HabilitationComprovativePdfId.HasValue)
            {
                return Result<FileDownloadResult>
                    .Fail("Não encontrado.", "Nenhum PDF de certificado de habilitações encontrado para esta pessoa.",
                        StatusCodes.Status404NotFound);
            }

            var pdfContentResult = await _pdfService.GetSavedPdfContentAsync(person.HabilitationComprovativePdfId.Value);
            if (!pdfContentResult.Success)
            {
                return Result<FileDownloadResult>
                    .Fail(pdfContentResult.Title!, pdfContentResult.Message!, pdfContentResult.StatusCode!.Value);
            }

            var fileName = $"Habilitation_Comprovative_{person.FirstName}_{person.LastName}.pdf";
            var fileResult = new FileDownloadResult
            {
                Content = pdfContentResult.Data!,
                FileName = fileName,
                ContentType = "application/pdf"
            };

            return Result<FileDownloadResult>
                .Ok(fileResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading habilitation PDF for person {PersonId}", personId);
            return Result<FileDownloadResult>
                .Fail("Erro interno.", "Ocorreu um erro ao baixar o PDF.",
                    StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result> DeleteHabilitationPdfAsync(long personId)
    {
        try
        {
            var person = await _context.People.FindAsync(personId);
            if (person == null)
            {
                return Result
                    .Fail("Não encontrado.", "Pessoa não encontrada.",
                        StatusCodes.Status404NotFound);
            }

            if (!person.HabilitationComprovativePdfId.HasValue)
            {
                return Result
                    .Fail("Não encontrado.", "Nenhum PDF de certificado de habilitações encontrado para esta pessoa.",
                        StatusCodes.Status404NotFound);
            }

            // Delete PDF using PdfService
            var deletePdfResult = await _pdfService.DeleteSavedPdfAsync(person.HabilitationComprovativePdfId.Value);
            if (!deletePdfResult.Success)
            {
                return Result
                    .Fail(deletePdfResult.Title!, deletePdfResult.Message!, deletePdfResult.StatusCode!.Value);
            }

            // Update person's HabilitationComprovativePdfId
            person.HabilitationComprovativePdfId = null;
            person.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Update cache
            await RemoveRelatedCache(person.Id);

            // Regenerate notifications for this person since the document was deleted
            await _notificationService.GenerateNotificationsAsync();

            return Result
                .Ok("PDF eliminado.", "PDF de certificado de habilitações eliminado com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting habilitation PDF for person {PersonId}", personId);
            return Result
                .Fail("Erro interno.", "Ocorreu um erro ao eliminar o PDF.",
                    StatusCodes.Status500InternalServerError);
        }
    }

    // IBAN PDF Methods
    public async Task<Result<RetrievePersonDto>> UploadIbanPdfAsync(long personId, IFormFile file, string generatedByUserId)
    {
        try
        {
            // Validate the person exists
            var person = await _context.People.FindAsync(personId);
            if (person == null)
            {
                return Result<RetrievePersonDto>
                    .Fail("Não encontrado.", "Pessoa não encontrada.",
                        StatusCodes.Status404NotFound);
            }

            if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
            {
                return Result<RetrievePersonDto>
                    .Fail("Tipo de arquivo inválido.", "Apenas arquivos PDF são permitidos.",
                        StatusCodes.Status400BadRequest);
            }

            // Read file content
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var pdfContent = memoryStream.ToArray();

            // Save PDF using PdfService
            var savePdfResult = await _pdfService.SavePdfAsync(PdfTypes.IbanComprovative, personId, pdfContent, generatedByUserId);
            if (!savePdfResult.Success)
            {
                return Result<RetrievePersonDto>
                    .Fail(savePdfResult.Title!, savePdfResult.Message!, savePdfResult.StatusCode!.Value);
            }

            // Update person's IbanComprovativePdfId
            person.IbanComprovativePdfId = savePdfResult.Data!.Id;
            person.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var retrievePerson = Person.ConvertEntityToRetrieveDto(person);

            // Update cache
            await RemoveRelatedCache(person.Id);
            await _cache.SetSinglePersonCacheAsync(retrievePerson);

            // Delete notifications related to this person's missing IBAN document
            await _notificationService.GenerateNotificationsAsync();

            return Result<RetrievePersonDto>
                .Ok(retrievePerson, "PDF carregado.", "PDF de comprovativo de IBAN carregado com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading IBAN PDF for person {PersonId}", personId);
            return Result<RetrievePersonDto>
                .Fail("Erro interno.", "Ocorreu um erro ao carregar o PDF.",
                    StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<FileDownloadResult>> GetIbanPdfAsync(long personId)
    {
        try
        {
            var person = await _context.People.FindAsync(personId);
            if (person == null)
            {
                return Result<FileDownloadResult>
                    .Fail("Não encontrado.", "Pessoa não encontrada.",
                        StatusCodes.Status404NotFound);
            }

            if (!person.IbanComprovativePdfId.HasValue)
            {
                return Result<FileDownloadResult>
                    .Fail("Não encontrado.", "Nenhum PDF de comprovativo de IBAN encontrado para esta pessoa.",
                        StatusCodes.Status404NotFound);
            }

            var pdfContentResult = await _pdfService.GetSavedPdfContentAsync(person.IbanComprovativePdfId.Value);
            if (!pdfContentResult.Success)
            {
                return Result<FileDownloadResult>
                    .Fail(pdfContentResult.Title!, pdfContentResult.Message!, pdfContentResult.StatusCode!.Value);
            }

            var fileName = $"Comprovativo_IBAN_{person.FirstName}_{person.LastName}.pdf";
            return Result<FileDownloadResult>
                .Ok(new FileDownloadResult
                {
                    Content = pdfContentResult.Data!,
                    FileName = fileName
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting IBAN PDF for person {PersonId}", personId);
            return Result<FileDownloadResult>
                .Fail("Erro interno.", "Ocorreu um erro ao obter o PDF.",
                    StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result> DeleteIbanPdfAsync(long personId)
    {
        try
        {
            var person = await _context.People.FindAsync(personId);
            if (person == null)
            {
                return Result
                    .Fail("Não encontrado.", "Pessoa não encontrada.",
                        StatusCodes.Status404NotFound);
            }

            if (!person.IbanComprovativePdfId.HasValue)
            {
                return Result
                    .Fail("Não encontrado.", "Nenhum PDF de comprovativo de IBAN encontrado para esta pessoa.",
                        StatusCodes.Status404NotFound);
            }

            // Delete PDF using PdfService
            var deletePdfResult = await _pdfService.DeleteSavedPdfAsync(person.IbanComprovativePdfId.Value);
            if (!deletePdfResult.Success)
            {
                return Result
                    .Fail(deletePdfResult.Title!, deletePdfResult.Message!, deletePdfResult.StatusCode!.Value);
            }

            // Update person's IbanComprovativePdfId
            person.IbanComprovativePdfId = null;
            person.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Update cache
            await RemoveRelatedCache(person.Id);

            // Regenerate notifications for this person since the document was deleted
            await _notificationService.GenerateNotificationsAsync();

            return Result
                .Ok("PDF eliminado.", "PDF de comprovativo de IBAN eliminado com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting IBAN PDF for person {PersonId}", personId);
            return Result
                .Fail("Erro interno.", "Ocorreu um erro ao eliminar o PDF.",
                    StatusCodes.Status500InternalServerError);
        }
    }

    // Identification Document PDF Methods
    public async Task<Result<RetrievePersonDto>> UploadIdentificationDocumentPdfAsync(long personId, IFormFile file, string generatedByUserId)
    {
        try
        {
            // Validate the person exists
            var person = await _context.People.FindAsync(personId);
            if (person == null)
            {
                return Result<RetrievePersonDto>
                    .Fail("Não encontrado.", "Pessoa não encontrada.", StatusCodes.Status404NotFound);
            }

            if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
            {
                return Result<RetrievePersonDto>
                    .Fail("Tipo de arquivo inválido.", "Apenas arquivos PDF são permitidos.", StatusCodes.Status400BadRequest);
            }

            // Read file content
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var pdfContent = memoryStream.ToArray();

            // Save PDF using PdfService
            var savePdfResult = await _pdfService.SavePdfAsync(PdfTypes.IdentificationDocument, personId, pdfContent, generatedByUserId);
            if (!savePdfResult.Success)
            {
                return Result<RetrievePersonDto>
                    .Fail(savePdfResult.Title!, savePdfResult.Message!, savePdfResult.StatusCode!.Value);
            }

            // Update person's IdentificationDocumentPdfId
            person.IdentificationDocumentPdfId = savePdfResult.Data!.Id;
            person.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var retrievePerson = Person.ConvertEntityToRetrieveDto(person);

            // Update cache
            await RemoveRelatedCache(person.Id);
            await _cache.SetSinglePersonCacheAsync(retrievePerson);

            // Delete notifications related to this person's missing documents
            await _notificationService.GenerateNotificationsAsync();

            return Result<RetrievePersonDto>
                .Ok(retrievePerson, "PDF carregado.", "PDF de documento de identificação carregado com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading identification document PDF for person {PersonId}", personId);
            return Result<RetrievePersonDto>
                .Fail("Erro interno.", "Ocorreu um erro ao carregar o PDF.",
                    StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<FileDownloadResult>> GetIdentificationDocumentPdfAsync(long personId)
    {
        try
        {
            var person = await _context.People.FindAsync(personId);
            if (person == null)
            {
                return Result<FileDownloadResult>
                    .Fail("Não encontrado.", "Pessoa não encontrada.",
                        StatusCodes.Status404NotFound);
            }

            if (!person.IdentificationDocumentPdfId.HasValue)
            {
                return Result<FileDownloadResult>
                    .Fail("Não encontrado.", "Nenhum PDF de documento de identificação encontrado para esta pessoa.",
                        StatusCodes.Status404NotFound);
            }

            var pdfContentResult = await _pdfService.GetSavedPdfContentAsync(person.IdentificationDocumentPdfId.Value);
            if (!pdfContentResult.Success)
            {
                return Result<FileDownloadResult>
                    .Fail(pdfContentResult.Title!, pdfContentResult.Message!, pdfContentResult.StatusCode!.Value);
            }

            var fileName = $"Documento_Identificacao_{person.FirstName}_{person.LastName}.pdf";
            return Result<FileDownloadResult>
                .Ok(new FileDownloadResult
                {
                    Content = pdfContentResult.Data!,
                    FileName = fileName
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting identification document PDF for person {PersonId}", personId);
            return Result<FileDownloadResult>
                .Fail("Erro interno.", "Ocorreu um erro ao obter o PDF.",
                    StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result> DeleteIdentificationDocumentPdfAsync(long personId)
    {
        try
        {
            var person = await _context.People.FindAsync(personId);
            if (person == null)
            {
                return Result
                    .Fail("Não encontrado.", "Pessoa não encontrada.",
                        StatusCodes.Status404NotFound);
            }

            if (!person.IdentificationDocumentPdfId.HasValue)
            {
                return Result
                    .Fail("Não encontrado.", "Nenhum PDF de documento de identificação encontrado para esta pessoa.",
                        StatusCodes.Status404NotFound);
            }

            // Delete PDF using PdfService
            var deletePdfResult = await _pdfService.DeleteSavedPdfAsync(person.IdentificationDocumentPdfId.Value);
            if (!deletePdfResult.Success)
            {
                return Result
                    .Fail(deletePdfResult.Title!, deletePdfResult.Message!, deletePdfResult.StatusCode!.Value);
            }

            // Update person's IdentificationDocumentPdfId
            person.IdentificationDocumentPdfId = null;
            person.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Update cache
            await RemoveRelatedCache(person.Id);

            // Regenerate notifications for this person since the document was deleted
            await _notificationService.GenerateNotificationsAsync();

            return Result
                .Ok("PDF eliminado.", "PDF de documento de identificação eliminado com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting identification document PDF for person {PersonId}", personId);
            return Result
                .Fail("Erro interno.", "Ocorreu um erro ao eliminar o PDF.",
                    StatusCodes.Status500InternalServerError);
        }
    }
}