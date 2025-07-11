using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Global.Dtos;
using NERBABO.ApiService.Core.Global.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Models;
using ZLinq;

namespace NERBABO.ApiService.Core.Global.Services;

public class GeneralInfoService(
    AppDbContext context,
    ILogger<GeneralInfoService> logger
    ) : IGeneralInfoService
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<GeneralInfoService> _logger = logger;
    private GeneralInfo? _cachedConfig;

    // https://stackoverflow.com/questions/20056727/need-to-understand-the-usage-of-semaphoreslim 
    // Good SemaphoreSlim explanation above
    private readonly SemaphoreSlim _cacheLock = new(1, 1);
    private bool _disposed;

    public async Task<Result<RetrieveGeneralInfoDto>> GetGeneralInfoAsync()
    {
        if (_cachedConfig is not null)
            return Result<RetrieveGeneralInfoDto>
                .Ok(GeneralInfo.ConvertEntityToRetrieveDto(_cachedConfig));

        await _cacheLock.WaitAsync();
        try
        {
            // Double-check in case another thread loaded it while we were waiting
            if (_cachedConfig is not null)
                return Result<RetrieveGeneralInfoDto>
                    .Ok(GeneralInfo.ConvertEntityToRetrieveDto(_cachedConfig));

            _cachedConfig = await _context.GeneralInfo
                .Include(c => c.IvaTax)
                .AsNoTracking()
                .Where(c => c.Id == 1)
                .FirstAsync(); // Will throw if no records exist

            return Result<RetrieveGeneralInfoDto>
                    .Ok(GeneralInfo.ConvertEntityToRetrieveDto(_cachedConfig));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Configuration record not found in database");
            throw new ApplicationException("Configuração de Informação Geral em falta.", ex);
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _cacheLock.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    public async Task<Result> UpdateGeneralInfoAsync(UpdateGeneralInfoDto updateGeneralInfo)
    {
        if (await _context.Taxes.FindAsync(updateGeneralInfo.IvaId) is null)
        {
            return Result
                .Fail("Não encontrado.", "Taxa de Iva não encontrada.",
                StatusCodes.Status404NotFound);
        }

        var config = GeneralInfo.ConvertUpdateDtoToEntity(updateGeneralInfo);
        await UpdateConfigurationAsync(c =>
        {
            c.Designation = config.Designation;
            c.Site = config.Site;
            c.HourValueTeacher = config.HourValueTeacher;
            c.HourValueAlimentation = config.HourValueAlimentation;
            c.BankEntity = config.BankEntity;
            c.Iban = config.Iban;
            c.Nipc = config.Nipc;
            c.LogoFinancing = config.LogoFinancing;
            c.IvaId = config.IvaId;
        });

        return Result
            .Ok("Informação Atualizada.", "Foram atualizadas as configurações gerais.");
    }


    #region Private Helper Methods
    private async Task UpdateConfigurationAsync(Action<GeneralInfo> updateAction)
    {
        await _cacheLock.WaitAsync();
        try
        {
            var config = await _context.GeneralInfo.FirstAsync();
            updateAction(config);
            await _context.SaveChangesAsync();
            _cachedConfig = null; // Invalidate cache
        }
        finally
        {
            _cacheLock.Release();
        }
    }
    #endregion
}
