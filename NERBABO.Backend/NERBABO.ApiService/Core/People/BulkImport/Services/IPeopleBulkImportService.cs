using NERBABO.ApiService.Core.People.Dtos;
using NERBABO.ApiService.Core.People.Models;
using NERBABO.ApiService.Shared.BulkImport.Services;

namespace NERBABO.ApiService.Core.People.BulkImport.Services;

public interface IPeopleBulkImportService :
    IBulkImportService<CreatePersonDto, Person, RetrievePersonDto>
{
}
