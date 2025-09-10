using System;
using NERBABO.ApiService.Core.ModuleAvaliations.Dtos;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.ModuleAvaliations.Services;

public class ModuleAvaliationsService(
    ILogger<ModuleAvaliationsService> logger,
    AppDbContext context
) : IModuleAvaliationsService
{
    private readonly ILogger<ModuleAvaliationsService> _logger = logger;
    private readonly AppDbContext _context = context;

    public Task<Result<RetrieveModuleAvaliationDto>> CreateAsync(CreateModuleAvaliationDto entityDto)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeleteAsync(long id)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IEnumerable<RetrieveModuleAvaliationDto>>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Result<RetrieveModuleAvaliationDto>> GetByIdAsync(long id)
    {
        throw new NotImplementedException();
    }

    public Task<Result<RetrieveModuleAvaliationDto>> UpdateAsync(UpdateModuleAvaliationDto entityDto)
    {
        throw new NotImplementedException();
    }
}
