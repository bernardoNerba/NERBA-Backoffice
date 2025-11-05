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

    public Task<Result> DeleteAsync(long id)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IEnumerable<RetrieveCategoryDto>>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Result<RetrieveCategoryDto>> GetByIdAsync(long id)
    {
        throw new NotImplementedException();
    }

    public Task<Result<RetrieveCategoryDto>> UpdateAsync(UpdateCategoryDto entityDto)
    {
        throw new NotImplementedException();
    }
}