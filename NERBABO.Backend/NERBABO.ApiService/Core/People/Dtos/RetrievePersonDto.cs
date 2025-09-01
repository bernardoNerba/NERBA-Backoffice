using System;

namespace NERBABO.ApiService.Core.People.Dtos;

public class RetrievePersonDto
{
    public long Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string NIF { get; set; } = string.Empty;
    public string? IdentificationNumber { get; set; }
    public DateOnly? IdentificationValidationDate { get; set; }
    public string? NISS { get; set; }
    public string? IBAN { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? Address { get; set; }
    public string? ZipCode { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Naturality { get; set; }
    public string? Nationality { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string Habilitation { get; set; } = string.Empty;
    public string IdentificationType { get; set; } = string.Empty;

    public int Age =>
        BirthDate.HasValue ? DateTime.UtcNow.Year - BirthDate.Value.Year : 0;

    public bool IsTeacher { get; set; }
    public bool IsStudent { get; set; }
    public bool IsColaborator { get; set; }
    
    public long? HabilitationComprovativePdfId { get; set; }
    public long? NifComprovativePdfId { get; set; }
    public long? IdentificationDocumentPdfId { get; set; }

}
