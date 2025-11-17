using NERBABO.ApiService.Core.Modules.Dtos;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Modules.Services;


public interface IModuleCategoryService
    : IGenericService<RetrieveCategoryDto, CreateCategoryDto, UpdateCategoryDto, long>
{

}