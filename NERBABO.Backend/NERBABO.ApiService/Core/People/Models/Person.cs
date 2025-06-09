using Humanizer;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.People.Dtos;
using NERBABO.ApiService.Core.Students.Models;
using NERBABO.ApiService.Core.Teachers.Models;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.People.Models;

public class Person : Entity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string NIF { get; set; } = string.Empty;
    public string? IdentificationNumber { get; set; } = string.Empty;
    public DateOnly? IdentificationValidationDate { get; set; }
    public IdentificationTypeEnum IdentificationType { get; set; } = IdentificationTypeEnum.Unknown;
    public string? NISS { get; set; } = string.Empty;
    public string? IBAN { get; set; } = string.Empty;
    public DateOnly? BirthDate { get; set; }
    public string? Address { get; set; } = string.Empty;
    public string? ZipCode { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public string? Naturality { get; set; } = string.Empty;
    public string? Nationality { get; set; } = string.Empty;
    public GenderEnum Gender { get; set; } = GenderEnum.Unknown;
    public HabilitationEnum Habilitation { get; set; } = HabilitationEnum.WithoutProof;


    public User? User { get; set; }
    public Teacher? Teacher { get; set; }
    public Student? Student { get; set; }

    public Person() { }

    public static Person ConvertCreateDtoToEntity(CreatePersonDto personDto)
    {
        return new Person
        {
            FirstName = personDto.FirstName,
            LastName = personDto.LastName,
            NIF = personDto.NIF,
            IdentificationNumber = personDto.IdentificationNumber,
            IdentificationValidationDate = Helper.StringDateOnlyConverter.ConvertToDateOnly(personDto.IdentificationValidationDate),
            NISS = personDto.NISS,
            IBAN = personDto.IBAN,
            BirthDate = Helper.StringDateOnlyConverter.ConvertToDateOnly(personDto.BirthDate),
            Address = personDto.Address,
            ZipCode = personDto.ZipCode,
            PhoneNumber = personDto.PhoneNumber,
            Email = personDto.Email,
            Naturality = personDto.Naturality,
            Nationality = personDto.Nationality,
            Gender = personDto.Gender?.DehumanizeTo<GenderEnum>() ?? GenderEnum.Unknown,
            Habilitation = personDto.Habilitation?.DehumanizeTo<HabilitationEnum>() ?? HabilitationEnum.WithoutProof,
            IdentificationType = personDto.IdentificationType?.DehumanizeTo<IdentificationTypeEnum>() ?? IdentificationTypeEnum.Unknown,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow

        };
    }

    public static RetrievePersonDto ConvertEntityToRetrieveDto(Person person)
    {
        return new RetrievePersonDto
        {
            Id = person.Id,
            FirstName = person.FirstName,
            LastName = person.LastName,
            FullName = $"{person.FirstName} {person.LastName}",
            NIF = person.NIF,
            IdentificationNumber = person.IdentificationNumber,
            IdentificationValidationDate = person.IdentificationValidationDate,
            NISS = person.NISS,
            IBAN = person.IBAN,
            BirthDate = person.BirthDate,
            Address = person.Address,
            ZipCode = person.ZipCode,
            PhoneNumber = person.PhoneNumber,
            Email = person.Email,
            Naturality = person.Naturality,
            Nationality = person.Nationality,
            Gender = person.Gender.Humanize().Transform(To.TitleCase),
            Habilitation = person.Habilitation.Humanize().Transform(To.TitleCase),
            IdentificationType = person.IdentificationType.Humanize().Transform(To.TitleCase),
        };
    }

    public static Person ConvertUpdateDtoToEntity(UpdatePersonDto personDto)
    {
        return new Person
        {
            Id = personDto.Id,
            FirstName = personDto.FirstName,
            LastName = personDto.LastName,
            NIF = personDto.NIF,
            IdentificationNumber = personDto.IdentificationNumber,
            IdentificationValidationDate = Helper.StringDateOnlyConverter.ConvertToDateOnly(personDto.IdentificationValidationDate),
            NISS = personDto.NISS,
            IBAN = personDto.IBAN,
            BirthDate = Helper.StringDateOnlyConverter.ConvertToDateOnly(personDto.BirthDate),
            Address = personDto.Address,
            ZipCode = personDto.ZipCode,
            PhoneNumber = personDto.PhoneNumber,
            Email = personDto.Email,
            Naturality = personDto.Naturality,
            Nationality = personDto.Nationality,
            Gender = personDto.Gender?.DehumanizeTo<GenderEnum>() ?? GenderEnum.Unknown,
            Habilitation = personDto.Habilitation?.DehumanizeTo<HabilitationEnum>() ?? HabilitationEnum.WithoutProof,
            IdentificationType = personDto.IdentificationType?.DehumanizeTo<IdentificationTypeEnum>() ?? IdentificationTypeEnum.Unknown,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
