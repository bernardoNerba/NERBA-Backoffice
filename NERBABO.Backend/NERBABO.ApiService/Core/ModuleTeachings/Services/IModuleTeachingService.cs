using System;
using NERBABO.ApiService.Core.ModuleTeachings.Dtos;
using NERBABO.ApiService.Core.Sessions.Dtos;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.ModuleTeachings.Services;

public interface IModuleTeachingService
: IGenericService<RetrieveModuleTeachingDto, CreateModuleTeachingDto, UpdateModuleTeachingDto, long>
{
    Task<Result<RetrieveModuleTeachingDto>> GetByActionAndModuleAsync(long actionId, long moduleId);
    Task<Result<IEnumerable<MinimalModuleTeachingDto>>> GetByActionIdMinimalAsync(long actionId);
}
