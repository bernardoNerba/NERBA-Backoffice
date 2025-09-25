using Humanizer;
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Global.Dtos;
using NERBABO.ApiService.Core.Global.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Helper;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;
using ZLinq;

namespace NERBABO.ApiService.Core.Global.Services;

public class TaxService(
    AppDbContext context,
    ILogger<TaxService> logger
    ) : ITaxService
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<TaxService> _logger = logger;

    public async Task<Result<RetrieveTaxDto>> CreateAsync(CreateTaxDto entityDto)
    {
        if (!Tax.IsValidTaxType(entityDto.Type))
        {
            _logger.LogWarning("Invalid tax type when creating instance {type}", entityDto.Type);
            return Result<RetrieveTaxDto>
                .Fail("Erro de Validação.", "Tipo de Taxa inválida.");
        }

        if (await _context.Taxes.AnyAsync(t => t.Name.ToLower().Equals(entityDto.Name.ToLower())))
        {
            _logger.LogWarning("Duplicated Name when creating tax. {name}", entityDto.Name);
            return Result<RetrieveTaxDto>
                .Fail("Erro de Validação", "Já existe uma taxa com este regime.");
        }

        var taxToCreate = Tax.ConvertCreateDtoToEntity(entityDto);
        var result = _context.Taxes.Add(taxToCreate);
        await _context.SaveChangesAsync();

        return Result<RetrieveTaxDto>
            .Ok(Tax.ConvertEntityToRetrieveDto(result.Entity),
            "Taxa Criada.", $"Taxa {entityDto.Name} criada com sucesso.",
            StatusCodes.Status201Created);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var taxIva = await _context.Taxes.FindAsync(id);
        if (taxIva is null)
            return Result
                .Fail("Não encontrado.", "Taxa não encontrada.",
                StatusCodes.Status404NotFound);

        if ((await _context.GeneralInfo.Where(g => g.Id == 1).FirstAsync()).IvaId == id ||
            (await _context.Teachers.Where(t => t.IvaRegimeId == id).AnyAsync()) ||
            (await _context.Teachers.Where(t => t.IrsRegimeId == id).AnyAsync()))
        {
            return Result
                .Fail("Erro de Validação", "Não é possível eliminar esta taxa sendo que existem entidades associadas");
        }

        _context.Taxes.Remove(taxIva);
        await _context.SaveChangesAsync();

        return Result
            .Ok("Taxa Iva Eliminada.", "Taxa de Iva eliminada com sucesso.");
    }

    public async Task<Result<IEnumerable<RetrieveTaxDto>>> GetAllAsync()
    {

        var existingTaxes = await _context.Taxes
            .OrderBy(t => t.Id)
            .ThenByDescending(t => t.ValuePercent)
            .Select(t => Tax.ConvertEntityToRetrieveDto(t))
            .ToListAsync();

        if (existingTaxes is null || existingTaxes.Count == 0)
            return Result<IEnumerable<RetrieveTaxDto>>
                .Fail("Não encontrado.", "Não foram encontradas taxas.");

        return Result<IEnumerable<RetrieveTaxDto>>
            .Ok(existingTaxes);
    }

    public async Task<Result<RetrieveTaxDto>> UpdateAsync(UpdateTaxDto entityDto)
    {
        if (!Tax.IsValidTaxType(entityDto.Type))
            return Result<RetrieveTaxDto>
                    .Fail("Erro de Validação.", "Taxa inválida.");

        var existingTax = await _context.Taxes
            .FirstAsync(i => i.Id == entityDto.Id);

        if (existingTax is null)
            return Result<RetrieveTaxDto>
                .Fail("Não encontrado.", "Taxa para atualizar não encontrada.",
                StatusCodes.Status404NotFound);

        if (existingTax.Name != entityDto.Name
            && await _context.Taxes.AnyAsync(i => i.Name == entityDto.Name))
        {
            return Result<RetrieveTaxDto>
                .Fail("Erro de Validação", "Já existe uma taxa com estas caracteristicas");
        }

        // Selective field updates - only update fields that have changed
        bool hasChanges = false;

        // Update Name if changed
        if (!string.Equals(existingTax.Name, entityDto.Name))
        {
            existingTax.Name = entityDto.Name;
            hasChanges = true;
        }

        // Update ValuePercent if changed
        if (Math.Abs(existingTax.ValuePercent - entityDto.ValuePercent) > 0.01f)
        {
            existingTax.ValuePercent = entityDto.ValuePercent;
            hasChanges = true;
        }

        // Update IsActive if changed
        if (existingTax.IsActive != entityDto.IsActive)
        {
            existingTax.IsActive = entityDto.IsActive;
            hasChanges = true;
        }

        // Update Type if changed
        var newType = entityDto.Type.DehumanizeTo<TaxEnum>();
        if (existingTax.Type != newType)
        {
            existingTax.Type = newType;
            hasChanges = true;
        }

        // Return fail result if no changes were detected
        if (!hasChanges)
        {
            _logger.LogInformation("No changes detected for Tax with ID {id}. No update performed.", entityDto.Id);
            return Result<RetrieveTaxDto>
                .Fail("Nenhuma alteração detetada.", "Não foi alterado nenhum dado. Modifique os dados e tente novamente.",
                StatusCodes.Status400BadRequest);
        }

        // Update UpdatedAt and save changes
        existingTax.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Result<RetrieveTaxDto>
            .Ok(Tax.ConvertEntityToRetrieveDto(existingTax),
            "Taxa Atualizada.", "Taxa atualizada com sucesso.");
    }

    public async Task<Result<IEnumerable<RetrieveTaxDto>>> GetByTypeAndIsActiveAsync(string type)
    {
        if (!EnumHelp.IsValidEnum<TaxEnum>(type))
            return Result<IEnumerable<RetrieveTaxDto>>
                .Fail("Erro de Validação.", "Tipo de Taxa inválida.");

        var existingTaxes = await _context.Taxes
            .Where(t => t.Type == type.DehumanizeTo<TaxEnum>()
                && t.IsActive)
            .Select(t => Tax.ConvertEntityToRetrieveDto(t))
            .ToListAsync();

        if (existingTaxes is null || existingTaxes.Count == 0)
            return Result<IEnumerable<RetrieveTaxDto>>
                .Fail("Não encontrado.", "Não foram encontradas taxas ativas.",
                StatusCodes.Status404NotFound);

        return Result<IEnumerable<RetrieveTaxDto>>
            .Ok(existingTaxes);
    }

    public async Task<Result<RetrieveTaxDto>> GetByIdAsync(int id)
    {
        var existingTax = await _context.Taxes.FindAsync(id);

        if (existingTax is null)
            return Result<RetrieveTaxDto>
                .Fail("Não encontrado.", "Taxa não encontrada.",
                StatusCodes.Status404NotFound);

        return Result<RetrieveTaxDto>
            .Ok(Tax.ConvertEntityToRetrieveDto(existingTax));
    }
}