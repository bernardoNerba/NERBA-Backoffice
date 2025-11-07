using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Modules.Dtos;
using NERBABO.ApiService.Core.Modules.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Modules.Services;

public class ModuleCategoryService(
    AppDbContext context,
    ILogger<ModuleCategoryService> logger
) : IModuleCategoryService
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<ModuleCategoryService> _logger = logger;

    public async Task<Result<RetrieveCategoryDto>> CreateAsync(CreateCategoryDto entityDto)
    {
        var category = ModuleCategory.ConvertCreateDtoToEntity(entityDto);
        if (await _context.ModuleCategories.AnyAsync(mc => mc.Equals(category)))
        {
            _logger.LogWarning("Invalid category type when creating ModuleCategory");
            return Result<RetrieveCategoryDto>
                .Fail("Erro de Validação", "Já existe uma Categoria de Módulo com estas caracteristicas.");
        }

        var result = _context.ModuleCategories.Add(category);
        await _context.SaveChangesAsync();
        return Result<RetrieveCategoryDto>
            .Ok(ModuleCategory.ConvertEntityToRetrieveDto(result.Entity),
            "Categoria de Módulo Criada.", $"Categoria de Módulo {entityDto.Name} criada com sucesso.",
            StatusCodes.Status201Created);
    }

    public async Task<Result> DeleteAsync(long id)
    {
        var existingCategory = await _context.ModuleCategories
            .Include(mc => mc.Modules)
            .FirstOrDefaultAsync(mc => mc.Id == id);

        if (existingCategory is null)
        {
            _logger.LogWarning("ModuleCategory with id {Id} not found when deleting ModuleCategory", id);
            return Result
                .Fail("Não encontrado.", $"Categoria de Módulo com id {id} não encontrada.");
        }

        // A module category can only be deleted if the are no modules associated with it
        if (existingCategory.Modules.Count > 0)
        {
            _logger.LogWarning("Attempted to delete ModuleCategory with id {Id} that has associated Modules", id);
            return Result
                .Fail("Erro de Validação",
                "Não é possível eliminar uma Categoria de Módulo que tenha Módulos associados.");
        }

        _context.ModuleCategories.Remove(existingCategory);
        await _context.SaveChangesAsync();

        return Result
            .Ok($"Categoria de Módulo Eliminada.",
            "Categoria de Módulo eliminada com sucesso.");
    }

    public async Task<Result<IEnumerable<RetrieveCategoryDto>>> GetAllAsync()
    {
        var existingCategories = await _context.ModuleCategories
            .AsNoTracking()
            .OrderByDescending(mc => mc.UpdatedAt)
            .Select(mc => ModuleCategory.ConvertEntityToRetrieveDto(mc))
            .ToListAsync();
        
        if (existingCategories is null || existingCategories.Count == 0)
        {
            _logger.LogWarning("No ModuleCategories found when retrieving all ModuleCategories");
            return Result<IEnumerable<RetrieveCategoryDto>>
                .Fail("Não encontrado.", "Não existem Categorias de Módulo registadas.");
        }
        
        return Result<IEnumerable<RetrieveCategoryDto>>
            .Ok(existingCategories);
    }

    public async Task<Result<RetrieveCategoryDto>> GetByIdAsync(long id)
    {
        var existingCategory = await _context.ModuleCategories
            .AsNoTracking()
            .Where(mc => mc.Id == id)
            .FirstOrDefaultAsync();

        if (existingCategory is null)
        {
            _logger.LogWarning("ModuleCategory with id {Id} not found when retrieving ModuleCategory by id", id);
            return Result<RetrieveCategoryDto>
                .Fail("Não encontrado.", $"Categoria de Módulo com id {id} não encontrada.");
        }

        return Result<RetrieveCategoryDto>
            .Ok(ModuleCategory.ConvertEntityToRetrieveDto(existingCategory));
    }

    public async Task<Result<RetrieveCategoryDto>> UpdateAsync(UpdateCategoryDto entityDto)
    {
        var existingCategory = await _context.ModuleCategories
            .FirstOrDefaultAsync(mc => mc.Id == entityDto.Id);

        if (existingCategory is null)
        {
            _logger.LogWarning("ModuleCategory with id {Id} not found when updating ModuleCategory", entityDto.Id);
            return Result<RetrieveCategoryDto>
                .Fail("Não encontrado.", $"Categoria de Módulo com id {entityDto.Id} não encontrada.");
        }

        var categoryToCompare = new ModuleCategory(entityDto.Name, entityDto.ShortenName);

        // unique constraint check
        if (await _context.ModuleCategories.AnyAsync(mc => mc.Equals(categoryToCompare)
                && mc.Id != entityDto.Id
                ))
        {
            _logger.LogWarning("Invalid category type when updating ModuleCategory with id {Id}", entityDto.Id);
            return Result<RetrieveCategoryDto>
                .Fail("Erro de Validação", "Já existe uma Categoria de Módulo com estas caracteristicas.");
        }

        // Selective field updates - only update fields that have changed
        bool hasChanges = false;

        // Check name changes
        if (!existingCategory.Name.Equals(entityDto.Name))
        {
            existingCategory.Name = entityDto.Name;
            hasChanges = true;
        }

        // Check ShortenName changes
        if (!existingCategory.ShortenName.Equals(entityDto.ShortenName))
        {
            existingCategory.ShortenName = entityDto.ShortenName;
            hasChanges = true;
        }

        // No changes where made
        if (!hasChanges)
        {
            _logger.LogInformation("No changes detected when updating ModuleCategory with id {Id}", entityDto.Id);
            return Result<RetrieveCategoryDto>
                .Fail("Nenhuma alteração efetuada.",
                $"Nenhuma alteração foi efetuada à categoria de módulo com o nome {existingCategory.Name}.",
                StatusCodes.Status400BadRequest);
        }

        // Update existing category UpdatedAt anb save changes
        existingCategory.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        RetrieveCategoryDto retrieveCategory = ModuleCategory.ConvertEntityToRetrieveDto(existingCategory);
        
        return Result<RetrieveCategoryDto>
                .Ok(retrieveCategory,
                "Módulo Atualizada.",
                $"Foi atualizado a categoria de módulo com o nome {existingCategory.Name}.");
    }
}