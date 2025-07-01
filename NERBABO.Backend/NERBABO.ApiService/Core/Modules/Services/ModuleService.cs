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

        public async Task<Result<RetrieveModuleDto>> CreateAsync(CreateModuleDto entityDto)
        {
            // Unique constrains check
            if (await _context.Modules.AnyAsync(m => m.Name == entityDto.Name))
            {
                _logger.LogWarning("Duplicated Module Name detected");
                return Result<RetrieveModuleDto>
                    .Fail("Nome duplicado.", "O nome do módulo deve ser único. Já existe no sistema.");
            }

            var createdModule = _context.Modules.Add(Module.ConvertCreateDtoToEntity(entityDto));
            await _context.SaveChangesAsync();

            var moduleToRetrieve = Module.ConvertEntityToRetrieveDto(createdModule.Entity);

            var cache_key = $"module:{moduleToRetrieve.Id}";
            await _cacheService.SetAsync(cache_key, moduleToRetrieve, TimeSpan.FromMinutes(30));
            await ClearCache();

            return Result<RetrieveModuleDto>
                .Ok(moduleToRetrieve, "Módulo criado com sucesso.",
                $"Foi criado um módulo com o nome {moduleToRetrieve.Name}.",
                StatusCodes.Status201Created);

        }

        public async Task<Result<IEnumerable<RetrieveModuleDto>>> GetActiveModulesAsync()
        {
            // Check if entry exists in cache
            var cacheKey = "modules:list:active";
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

        public async Task<Result<IEnumerable<RetrieveModuleDto>>> GetAllAsync()
        {
            // Check if entry exists in cache
            var cacheKey = "modules:list";
            var cachedModules = await _cacheService.GetAsync<List<RetrieveModuleDto>>(cacheKey);
            if (cachedModules is not null && cachedModules.Count > 0)
                return Result<IEnumerable<RetrieveModuleDto>>
                    .Ok(cachedModules);

            // If not in cache, retrieve from database
            var existingModules = await _context.Modules
                .OrderByDescending(m => m.IsActive) // true (1) first
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

        public async Task<Result<RetrieveModuleDto>> GetByIdAsync(long id)
        {
            // Check if entry exists in cache
            var cacheKey = $"module:{id}";
            var cachedModule = await _cacheService.GetAsync<RetrieveModuleDto>(cacheKey);
            if (cachedModule is not null)
                return Result<RetrieveModuleDto>
                    .Ok(cachedModule);

            // If not in cache, retrieve from database
            var existingModule = await _context.Modules
                .Where(m => m.Id == id)
                .Select(m => Module.ConvertEntityToRetrieveDto(m))
                .FirstOrDefaultAsync();

            if (existingModule is null)
                return Result<RetrieveModuleDto>
                    .Fail("Não encontrado.", "Módulo não encontrado.",
                    StatusCodes.Status404NotFound);

            // Cache the result for future requests
            await _cacheService.SetAsync(cacheKey, existingModule, TimeSpan.FromMinutes(30));
            return Result<RetrieveModuleDto>
                .Ok(existingModule);

        }

        public async Task<Result<RetrieveModuleDto>> UpdateAsync(UpdateModuleDto entityDto)
        {
            var existingModule = await _context.Modules.FindAsync(entityDto.Id);
            if (existingModule is null)
                return Result<RetrieveModuleDto>
                    .Fail("Não encontrado", "Módulo não encontrado.",
                    StatusCodes.Status404NotFound);

            // Unique constrains check
            if (existingModule.Name != entityDto.Name
                && await _context.Modules.AnyAsync(m => m.Name == entityDto.Name))
            {
                _logger.LogWarning("Duplicated Module Name detected");
                return Result<RetrieveModuleDto>
                    .Fail("Nome duplicado.", "O nome do módulo deve ser único. Já existe no sistema.");
            }


            _context.Entry(existingModule).CurrentValues.SetValues(Module.ConvertUpdateDtoToEntity(entityDto));
            await _context.SaveChangesAsync();

            // Update cache
            var cacheKey = $"module:{existingModule.Id}";
            await _cacheService.SetAsync(cacheKey, Module.ConvertEntityToRetrieveDto(existingModule), TimeSpan.FromMinutes(30));
            await ClearCache();

            return Result<RetrieveModuleDto>
                .Ok(Module.ConvertEntityToRetrieveDto(existingModule),
                "Módulo Atualizada.",
                $"Foi atualizado o módulo com o nome {existingModule.Name}.");
        }

        public async Task<Result> DeleteAsync(long id)
        {
            // TODO: Handle delete validation when associations with other classes is done
            var existingModule = await _context.Modules.FindAsync(id);
            if (existingModule is null)
                return Result
                    .Fail("Não encontrado", "Módulo não encontrado.",
                    StatusCodes.Status404NotFound);


            _context.Modules.Remove(existingModule);
            await _context.SaveChangesAsync();

            // Remove from cache
            await _cacheService.RemoveAsync($"module:{id}");
            await ClearCache();

            return Result
                .Ok("Módulo Eliminado", "Módulo eliminado com sucesso.");
        }

        #region Private Helper Methods
        private async Task ClearCache()
        {
            // Clear cache entries related to modules
            await _cacheService.RemoveAsync("modules:list");
            await _cacheService.RemoveAsync("modules:list:active");
        }
        #endregion
    }
}
