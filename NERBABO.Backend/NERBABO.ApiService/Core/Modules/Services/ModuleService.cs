using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Modules.Dtos;
using NERBABO.ApiService.Core.Modules.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Modules.Services
{
    public class ModuleService(
        AppDbContext context,
        ICacheService cacheService,
        ILogger<ModuleService> logger
        ) : IModuleService
    {
        private readonly AppDbContext _context = context;
        private readonly ICacheService _cacheService = cacheService;
        private readonly ILogger<ModuleService> _logger = logger;

        public async Task<Result<RetrieveModuleDto>> CreateModuleAsync(CreateModuleDto moduleDto)
        {
            // Unique constrains check
            if (await _context.Modules.AnyAsync(m => m.Name == moduleDto.Name))
            {
                _logger.LogWarning("Duplicated Module Name detected");
                return Result<RetrieveModuleDto>
                    .Fail("Nome duplicado.", "O nome do módulo deve ser único. Já existe no sistema.");
            }

            var createdModule = _context.Modules.Add(Module.ConvertCreateDtoToEntity(moduleDto));
            await _context.SaveChangesAsync();

            var moduleToRetrieve = Module.ConvertEntityToRetrieveDto(createdModule.Entity);

            var cache_key = $"module:{moduleToRetrieve.Id}";
            await _cacheService.SetAsync(cache_key, moduleToRetrieve, TimeSpan.FromMinutes(30));
            await _cacheService.RemoveAsync("modules:list");

            return Result<RetrieveModuleDto>
                .Ok(moduleToRetrieve, "Módulo criado com sucesso.",
                $"Foi criado um módulo com o nome {moduleToRetrieve.Name}.",
                StatusCodes.Status201Created);

        }

        public Task<Result<IEnumerable<RetrieveModuleDto>>> GetActiveModulesAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Result<IEnumerable<RetrieveModuleDto>>> GetAllModulesAsync()
        {
            // Check if entry exists in cache
            var cacheKey = "modules:list";
            var cachedModules = await _cacheService.GetAsync<List<RetrieveModuleDto>>(cacheKey);
            if (cachedModules is not null && cachedModules.Count > 0)
                return Result<IEnumerable<RetrieveModuleDto>>
                    .Ok(cachedModules);

            // If not in cache, retrieve from database
            var existingModules = await _context.Modules
                .Where(m => m.IsActive)
                .OrderByDescending(m => m.CreatedAt)
                .ThenBy(m => m.Name)
                .Select(m => Module.ConvertEntityToRetrieveDto(m))
                .ToListAsync();

            // Check if there is data on db
            if (existingModules is null || existingModules.Count == 0)
            {
                _logger.LogInformation("No modules found in the database.");
                return Result<IEnumerable<RetrieveModuleDto>>
                    .Fail("Nenhum módulo encontrado.", "Não existem módulos ativos no sistema.",
                    StatusCodes.Status404NotFound);
            }

            // Cache the result for future requests
            await _cacheService.SetAsync(cacheKey, existingModules, TimeSpan.FromMinutes(30));

            return Result<IEnumerable<RetrieveModuleDto>>
                .Ok(existingModules);
        }

        public Task<Result<RetrieveModuleDto>> GetModuleByIdAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<RetrieveModuleDto>> UpdateModuleAsync(UpdateModuleDto moduleDto)
        {
            throw new NotImplementedException();
        }

        Task<Result> IModuleService.DeleteModuleAsync(long id)
        {
            throw new NotImplementedException();
        }
    }
}
