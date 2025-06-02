
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Global.Dtos;
using NERBABO.ApiService.Core.Global.Models;
using NERBABO.ApiService.Data;
using ZLinq;

namespace NERBABO.ApiService.Core.Global.Services;

public class IvaTaxService : IIvaTaxService
{
    private readonly AppDbContext _context;
    private readonly ILogger<IvaTaxService> _logger;
    public IvaTaxService(
        AppDbContext context,
        ILogger<IvaTaxService> logger)
    {
        _context = context;
        _logger = logger;
    }
    public async Task CreateTaxIvaAsync(CreateIvaTaxDto tax)
    {
        if (await _context.IvaTaxes.AnyAsync(t => t.Name == tax.Name))
        {
            throw new Exception("Já existe uma taxa de IVA com este nome.");
        }

        try
        {
            _context.IvaTaxes.Add(IvaTax.ConvertCreateDtoToEntity(tax));
            await _context.SaveChangesAsync();

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tax Iva");
            throw new Exception("Erro ao criar taxa de IVA.");
        }
    }

    public async Task DeleteTaxIvaAsync(int id)
    {
        var taxIva = await _context.IvaTaxes.FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new Exception("Taxa de IVA não encontrada.");

        if ((await _context.GeneralInfo.Where(g => g.Id == 1).FirstAsync()).IvaId == id)
        {
            throw new Exception("Não é possível eliminar esta taxa de IVA sendo que existem entidades associadas");
        }

        try
        {
            _context.IvaTaxes.Remove(taxIva);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tax Iva");
            throw new Exception("Erro ao eliminar taxa de IVA.");
        }
    }

    public async Task<IEnumerable<RetrieveIvaTaxDto>> GetAllIvaTaxesAsync()
    {
        List<RetrieveIvaTaxDto> taxIvaList = [];
        try
        {

            var taxes = await _context.IvaTaxes
                .OrderBy(t => t.Id)
                .ToListAsync();

            foreach (var tax in taxes)
            {
                taxIvaList.Add(IvaTax.ConvertEntityToRetrieveDto(tax));
            }
            return taxIvaList;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error TaxIva :");
            return [];
        }
    }

    public async Task UpdateIvaTaxAsync(UpdateIvaTaxDto updateIvaTax)
    {
        var existingTax = await _context.IvaTaxes
            .FirstAsync(i => i.Id == updateIvaTax.Id)
            ?? throw new Exception("Operação Inválida");

        if (existingTax.Name != updateIvaTax.Name
            && !_context.IvaTaxes.Any(i => i.Name == updateIvaTax.Name))
        {
            throw new Exception("Já existe uma taxa de iva com estas caracteristicas");
        }

        _context.Entry(existingTax).CurrentValues.SetValues(IvaTax.ConvertUpdateDtoToEntity(updateIvaTax));
        await _context.SaveChangesAsync();
    }
}
