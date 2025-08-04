using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Actions.Cache;
using NERBABO.ApiService.Core.Courses.Cache;
using NERBABO.ApiService.Core.Modules.Cache;
using NERBABO.ApiService.Core.Modules.Dtos;
using NERBABO.ApiService.Core.Modules.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Models;
using System;

namespace NERBABO.ApiService.Core.Modules.Services
{
    public class ModuleService(
        AppDbContext context,
        ICacheModuleRepository cache,
        ICacheCourseRepository cacheCourse,
        ICacheActionRepository cacheAction,
        ILogger<ModuleService> logger
        ) : IModuleService
    {
        private readonly AppDbContext _context = context;
        private readonly ICacheModuleRepository _cache = cache;
        private readonly ICacheCourseRepository _cacheCourse = cacheCourse;
        private readonly ICacheActionRepository _cacheAction = cacheAction;
        private readonly ILogger<ModuleService> _logger = logger;

        public async Task<Result<RetrieveModuleDto>> CreateAsync(CreateModuleDto entityDto)
        {
            // Unique constrains check
            if (await _context.Modules.AnyAsync(m =>
                m.Name.ToLower().Equals(entityDto.Name.ToLower())))
            {
                _logger.LogWarning("Duplicated Module Name detected");
                return Result<RetrieveModuleDto>
                    .Fail("Erro de Validação.", "O nome do módulo deve ser único. Já existe no sistema.");
            }

            var createdModule = _context.Modules.Add(Module.ConvertCreateDtoToEntity(entityDto));
            await _context.SaveChangesAsync();

            var moduleToRetrieve = Module.ConvertEntityToRetrieveDto(createdModule.Entity);

            // Update cache
            await _cache.RemoveModuleCacheAsync();
            await _cache.SetSingleModuleCacheAsync(moduleToRetrieve);

            return Result<RetrieveModuleDto>
                .Ok(moduleToRetrieve, "Módulo criado com sucesso.",
                $"Foi criado um módulo com o nome {moduleToRetrieve.Name}.",
                StatusCodes.Status201Created);
        }

        public async Task<Result<IEnumerable<RetrieveModuleDto>>> GetActiveModulesAsync()
        {
            // Check if entry exists in cache
            var cachedModules = await _cache.GetCacheActiveModulesAsync();
            if (cachedModules is not null && cachedModules.ToList().Count > 0)
                return Result<IEnumerable<RetrieveModuleDto>>
                    .Ok(cachedModules);

            // If not in cache, retrieve from database
            var existingModules = await _context.Modules
                .Where(m => m.IsActive)
                .OrderByDescending(m => m.UpdatedAt)
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
            await _cache.SetActiveModulesCacheAsync(existingModules);

            return Result<IEnumerable<RetrieveModuleDto>>
                .Ok(existingModules);
        }

        public async Task<Result<IEnumerable<RetrieveModuleDto>>> GetAllAsync()
        {
            // Check if entry exists in cache
            var cachedModules = await _cache.GetCacheAllModulesAsync();
            if (cachedModules is not null && cachedModules.ToList().Count > 0)
                return Result<IEnumerable<RetrieveModuleDto>>
                    .Ok(cachedModules);

            // If not in cache, retrieve from database
            var existingModules = await _context.Modules
                .OrderByDescending(m => m.UpdatedAt)
                .Select(m => Module.ConvertEntityToRetrieveDto(m))
                .ToListAsync();

            // Check if there is data on db
            if (existingModules is null || existingModules.Count == 0)
            {
                _logger.LogInformation("No modules found in the database.");
                return Result<IEnumerable<RetrieveModuleDto>>
                    .Fail("Não encontrado.", "Não existem módulos ativos no sistema.",
                    StatusCodes.Status404NotFound);
            }

            // Update cache
            await _cache.SetAllModulesCacheAsync(existingModules);

            return Result<IEnumerable<RetrieveModuleDto>>
                .Ok(existingModules);
        }

        public async Task<Result<RetrieveModuleDto>> GetByIdAsync(long id)
        {
            // Check if entry exists in cache
            var cachedModule = await _cache.GetSingleModuleCacheAsync(id);
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
            await _cache.SetSingleModuleCacheAsync(existingModule);
            return Result<RetrieveModuleDto>
                .Ok(existingModule);
        }

        public async Task<Result<RetrieveModuleDto>> UpdateAsync(UpdateModuleDto entityDto)
        {
            var existingModule = await _context.Modules.FindAsync(entityDto.Id);
            if (existingModule is null)
                return Result<RetrieveModuleDto>
                    .Fail("Não encontrado.", "Módulo não encontrado.",
                    StatusCodes.Status404NotFound);

            // Unique constrains check
            if (await _context.Modules.AnyAsync(m =>
                m.Name.ToLower().Equals(entityDto.Name.ToLower())
                && m.Id != entityDto.Id)
                )
            {
                _logger.LogWarning("Duplicated Module Name detected");
                return Result<RetrieveModuleDto>
                    .Fail("Erro de Validação.", "O nome do módulo deve ser único. Já existe no sistema.");
            }

            // Selective field updates - only update fields that have changed
            bool hasChanges = false;

            // Update Name if changed
            if (!string.Equals(existingModule.Name, entityDto.Name))
            {
                existingModule.Name = entityDto.Name;
                hasChanges = true;
            }

            // Update Hours if changed
            if (Math.Abs(existingModule.Hours - entityDto.Hours) > 0.01f)
            {
                existingModule.Hours = entityDto.Hours;
                hasChanges = true;
            }

            // Update IsActive if changed
            if (existingModule.IsActive != entityDto.IsActive)
            {
                existingModule.IsActive = entityDto.IsActive;
                hasChanges = true;
            }

            // Return fail result if no changes were detected
            if (!hasChanges)
            {
                _logger.LogInformation("No changes detected for Module with ID {id}. No update performed.", entityDto.Id);
                return Result<RetrieveModuleDto>
                    .Fail("Nenhuma alteração detetada.", "Não foi alterado nenhum dado. Modifique os dados e tente novamente.",
                    StatusCodes.Status400BadRequest);
            }

            // Update UpdatedAt and save changes
            existingModule.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Update cache
            await _cache.RemoveModuleCacheAsync(existingModule.Id);
            await _cacheCourse.RemoveCourseCacheAsync();
            await _cacheAction.RemoveActionCacheAsync();
            await _cache.SetSingleModuleCacheAsync(Module.ConvertEntityToRetrieveDto(existingModule));

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

            // check if there are any active courses associated with this module
            // if true dont allow delete.
            var existingCoursesWithModule = await _context.Courses
                .Include(c => c.Modules)
                .Where(c => c.Modules.Any(m => m.Id == id))
                .ToListAsync();

            if (existingCoursesWithModule.Any(c => c.IsCourseActive))
            {
                _logger.LogWarning("Tryed to delete a module that has active on going courses, when is not possible.");
                return Result
                    .Fail("Erro de Validação", "Não pode efetuar esta ação sendo que existem cursos ativos associados com este módulo");
            }

            _context.Modules.Remove(existingModule);
            await _context.SaveChangesAsync();

            // Update cache
            await _cache.RemoveModuleCacheAsync(id);
            await _cacheCourse.RemoveCourseCacheAsync();
            await _cacheAction.RemoveActionCacheAsync();

            return Result
                .Ok("Módulo Eliminado", "Módulo eliminado com sucesso.");
        }

        public async Task<Result> ToggleModuleIsActiveAsync(long id)
        {
            var existingModule = await _context.Modules.FindAsync(id);
            if (existingModule is null)
                return Result
                    .Fail("Não encontrado.", "Módulo não encontrado.",
                    StatusCodes.Status404NotFound);

            existingModule.IsActive = !existingModule.IsActive;
            await _context.SaveChangesAsync();

            // Update cache
            await _cache.RemoveModuleCacheAsync(id);
            await _cacheCourse.RemoveCourseCacheAsync();
            await _cacheAction.RemoveActionCacheAsync();
            await _cache.SetSingleModuleCacheAsync(Module.ConvertEntityToRetrieveDto(existingModule));

            var status = existingModule.IsActive ? "Ativado" : "Desativado";

            return Result
                .Ok("Módulo atualizado.", $"Módulo {status} com sucesso.");
        }
    }
}
