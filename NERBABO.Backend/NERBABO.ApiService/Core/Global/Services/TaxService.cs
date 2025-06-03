
using Humanizer;
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Global.Dtos;
using NERBABO.ApiService.Core.Global.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Enums;
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
    public async Task CreateTaxAsync(CreateTaxDto tax)
    {
        if (!IsValidTaxType(tax.Type))
        {
            throw new Exception("Tipo de taxa inválido.");
        }

        if (await _context.Taxes.AnyAsync(t => t.Name == tax.Name))
        {
            throw new Exception("Já existe uma taxa com este regime.");
        }

        try
        {
            _context.Taxes.Add(Tax.ConvertCreateDtoToEntity(tax));
            await _context.SaveChangesAsync();

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tax.");
            throw new Exception("Erro ao criar taxa.");
        }
    }

    public async Task DeleteTaxAsync(int id)
    {
        var taxIva = await _context.Taxes.FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new Exception("Taxa não encontrada.");

        if ((await _context.GeneralInfo.Where(g => g.Id == 1).FirstAsync()).IvaId == id)
        {
            throw new Exception("Não é possível eliminar esta taxa sendo que existem entidades associadas");
        }

        try
        {
            _context.Taxes.Remove(taxIva);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tax Iva");
            throw new Exception("Erro ao eliminar taxa de IVA.");
        }
    }

    public async Task<IEnumerable<RetrieveTaxDto>> GetAllTaxesAsync()
    {
        List<RetrieveTaxDto> taxes = [];
        try
        {
            var existingTaxes = await _context.Taxes
                .OrderBy(t => t.Id)
                .ToListAsync();

            foreach (var tax in existingTaxes)
            {
                taxes.Add(Tax.ConvertEntityToRetrieveDto(tax));
            }
            return taxes;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error :");
            return [];
        }
    }

    public async Task UpdateTaxAsync(UpdateTaxDto tax)
    {
        if (!IsValidTaxType(tax.Type))
        {
            throw new Exception("Tipo de taxa inválido.");
        }

        var existingTax = await _context.Taxes
            .FirstAsync(i => i.Id == tax.Id)
            ?? throw new Exception("Operação Inválida");

        if (existingTax.Name != tax.Name
            && !_context.Taxes.Any(i => i.Name == tax.Name))
        {
            throw new Exception("Já existe uma taxa com estas caracteristicas");
        }

        _context.Entry(existingTax).CurrentValues.SetValues(Tax.ConvertUpdateDtoToEntity(tax));
        await _context.SaveChangesAsync();
    }


    #region Private Helper Methods
    private static bool IsValidTaxType(string type)
    {
        return type.Equals(TaxEnum.IVA.Humanize(), StringComparison.OrdinalIgnoreCase) ||
               type.Equals(TaxEnum.IRS.Humanize(), StringComparison.OrdinalIgnoreCase);
    }
    #endregion  
}
