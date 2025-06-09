
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.Latency;
using NERBABO.ApiService.Core.Global.Dtos;
using NERBABO.ApiService.Core.Global.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;
using System.Collections.Generic;
using ZLinq;

namespace NERBABO.ApiService.Core.Global.Services;

public class TaxService : ITaxService
{
    private readonly AppDbContext _context;
    private readonly ILogger<TaxService> _logger;
    public TaxService(
        AppDbContext context,
        ILogger<TaxService> logger)
    {
        _context = context;
        _logger = logger;
    }
    public async Task<Result> CreateTaxAsync(CreateTaxDto tax)
    {
        if (!IsValidTaxType(tax.Type))
            return Result
                .Fail("Erro de Validação.", "Taxa inválida.");

        if (await _context.Taxes.AnyAsync(t => t.Name == tax.Name))
            return Result
                .Fail("Erro de Validação", "Já existe uma taxa com este regime.",
                StatusCodes.Status404NotFound);

        _context.Taxes.Add(Tax.ConvertCreateDtoToEntity(tax));
        await _context.SaveChangesAsync();

        return Result
            .Ok("Taxa Criada.", $"Taxa {tax.Name} criada com sucesso.", StatusCodes.Status201Created);
    }

    public async Task<Result> DeleteTaxAsync(int id)
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

    public async Task<Result<IEnumerable<RetrieveTaxDto>>> GetAllTaxesAsync()
    {
        
        var existingTaxes = await _context.Taxes
            .Select(t => Tax.ConvertEntityToRetrieveDto(t))
            .OrderBy(t => t.Id)
            .ToListAsync();

        if (existingTaxes is null || existingTaxes.Count == 0)
            return Result<IEnumerable<RetrieveTaxDto>>
                .Fail("Não encontrado.", "Não foram encontradas taxas.");

        return Result<IEnumerable<RetrieveTaxDto>>
            .Ok(existingTaxes);
    }

    public async Task<Result> UpdateTaxAsync(UpdateTaxDto tax)
    {
        if (!IsValidTaxType(tax.Type))
            return Result
                    .Fail("Erro de Validação.", "Taxa inválida.");

        var existingTax = await _context.Taxes
            .FirstAsync(i => i.Id == tax.Id);

        if (existingTax is null)
            return Result
                .Fail("Não encontrado.", "Taxa para atualizar não encontrada.",
                StatusCodes.Status404NotFound);

        if (existingTax.Name != tax.Name
            && !_context.Taxes.Any(i => i.Name == tax.Name))
        {
            return Result
                .Fail("Erro de Validação", "Já existe uma taxa com estas caracteristicas");
        }

        _context.Entry(existingTax).CurrentValues.SetValues(Tax.ConvertUpdateDtoToEntity(tax));
        await _context.SaveChangesAsync();
        return Result
            .Ok("Taxa Atualizada.", "Taxa atualizada com sucesso.");
    }

    public async Task<Result<IEnumerable<RetrieveTaxDto>>> GetTaxesByTypeAsync(string type)
    {
        if (!IsValidTaxType(type))
            return Result<IEnumerable<RetrieveTaxDto>>
                .Fail("Erro de Validação.", "Taxa inválida.");

        var taxType = type.DehumanizeTo<TaxEnum>();

        var existingTaxes = await _context.Taxes
            .Where(t => t.Type == taxType)
            .Select(t => Tax.ConvertEntityToRetrieveDto(t))
            .ToListAsync();

        if (existingTaxes is null || existingTaxes.Count == 0)
            return Result<IEnumerable<RetrieveTaxDto>>
                .Fail("Não encontrado.", "Não foram encontradas taxas",
                StatusCodes.Status404NotFound);

        return Result<IEnumerable<RetrieveTaxDto>>
            .Ok(existingTaxes);
    }

    #region Private Helper Methods
    private static bool IsValidTaxType(string type)
    {
        return type.Equals(TaxEnum.IVA.Humanize(), StringComparison.OrdinalIgnoreCase) ||
               type.Equals(TaxEnum.IRS.Humanize(), StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}
