using System;
using NERBABO.ApiService.Core.ModuleAvaliations.Dtos;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.ModuleAvaliations.Services;

public interface IModuleAvaliationsService
: IGenericService<RetrieveModuleAvaliationDto, CreateModuleAvaliationDto, UpdateModuleAvaliationDto, long>
{

}
