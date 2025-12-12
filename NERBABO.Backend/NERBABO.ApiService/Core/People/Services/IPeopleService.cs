using NERBABO.ApiService.Core.People.Dtos;
using NERBABO.ApiService.Core.People.Models;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.People.Services;

public interface IPeopleService
    : IGenericService<RetrievePersonDto, CreatePersonDto, UpdatePersonDto, long>
{
    Task<Result<IEnumerable<RetrievePersonDto>>> GetAllWithoutProfileAsync(string profile);
    Task<Result<RelationshipPersonDto>> GetPersonRelationshipsAsync(long personId);

    // Create with files
    Task<Result<RetrievePersonDto>> CreateWithFilesAsync(
        CreatePersonDto personDto,
        IFormFile? habilitationPdf,
        IFormFile? ibanPdf,
        IFormFile? identificationDocumentPdf,
        string userId);

    // PDF-related methods
    Task<Result<RetrievePersonDto>> UploadHabilitationPdfAsync(long personId, IFormFile file, string userId);
    Task<Result<FileDownloadResult>> GetHabilitationPdfAsync(long personId);
    Task<Result> DeleteHabilitationPdfAsync(long personId);

    // IBAN PDF methods
    Task<Result<RetrievePersonDto>> UploadIbanPdfAsync(long personId, IFormFile file, string userId);
    Task<Result<FileDownloadResult>> GetIbanPdfAsync(long personId);
    Task<Result> DeleteIbanPdfAsync(long personId);

    // Identification Document PDF methods
    Task<Result<RetrievePersonDto>> UploadIdentificationDocumentPdfAsync(long personId, IFormFile file, string userId);
    Task<Result<FileDownloadResult>> GetIdentificationDocumentPdfAsync(long personId);
    Task<Result> DeleteIdentificationDocumentPdfAsync(long personId);
}
