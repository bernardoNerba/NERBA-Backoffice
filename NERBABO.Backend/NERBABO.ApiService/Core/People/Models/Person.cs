using NERBABO.ApiService.Core.Account.Models;
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
    public string? PostalCode { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public string? Naturality { get; set; } = string.Empty;
    public string? Nationality { get; set; } = string.Empty;
    public GenderEnum Gender { get; set; } = GenderEnum.Unknown;
    public HabilitationEnum Habilitation { get; set; } = HabilitationEnum.WithoutProof;


    public User? User { get; set; }

    public Person() { }

}
