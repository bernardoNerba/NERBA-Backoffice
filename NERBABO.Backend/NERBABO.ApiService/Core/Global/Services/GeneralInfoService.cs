using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Global.Dtos;
using NERBABO.ApiService.Core.Global.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Dtos;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;
using ZLinq;

namespace NERBABO.ApiService.Core.Global.Services;

public class GeneralInfoService(
    AppDbContext context,
    ILogger<GeneralInfoService> logger,
    IImageService imageService
    ) : IGeneralInfoService
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<GeneralInfoService> _logger = logger;
    private readonly IImageService _imageService = imageService;
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

        await UpdateConfigurationAsync(async c =>
        {
            // Selective field updates - only update fields that have changed
            bool hasChanges = false;

            // Update Designation if changed
            if (!string.Equals(c.Designation, updateGeneralInfo.Designation))
            {
                c.Designation = updateGeneralInfo.Designation;
                hasChanges = true;
            }

            // Update Site if changed
            if (!string.Equals(c.Site, updateGeneralInfo.Site))
            {
                c.Site = updateGeneralInfo.Site;
                hasChanges = true;
            }

            // Update HourValueTeacher if changed
            if (Math.Abs(c.HourValueTeacher - updateGeneralInfo.HourValueTeacher) > 0.01f)
            {
                c.HourValueTeacher = updateGeneralInfo.HourValueTeacher;
                hasChanges = true;
            }

            // Update HourValueAlimentation if changed
            if (Math.Abs(c.HourValueAlimentation - updateGeneralInfo.HourValueAlimentation) > 0.01f)
            {
                c.HourValueAlimentation = updateGeneralInfo.HourValueAlimentation;
                hasChanges = true;
            }

            // Update BankEntity if changed
            if (!string.Equals(c.BankEntity, updateGeneralInfo.BankEntity))
            {
                c.BankEntity = updateGeneralInfo.BankEntity;
                hasChanges = true;
            }

            // Update Iban if changed
            if (!string.Equals(c.Iban, updateGeneralInfo.Iban))
            {
                c.Iban = updateGeneralInfo.Iban;
                hasChanges = true;
            }

            // Update Nipc if changed
            if (!string.Equals(c.Nipc, updateGeneralInfo.Nipc))
            {
                c.Nipc = updateGeneralInfo.Nipc;
                hasChanges = true;
            }

            // Update Logo if file provided
            if (updateGeneralInfo.Logo is not null)
            {
                if (!_imageService.IsValidImageFile(updateGeneralInfo.Logo))
                {
                    throw new InvalidOperationException("O logo deve ser uma imagem válida (JPG, PNG, GIF, BMP) e menor que 5MB");
                }

                // Delete existing logo if it exists
                if (!string.IsNullOrEmpty(c.Logo))
                {
                    await _imageService.DeleteImageAsync(c.Logo);
                }

                // Save new logo
                var logoResult = await _imageService.SaveImageAsync(updateGeneralInfo.Logo, $"generalinfo/{c.Id}/logo");
                if (logoResult.Success)
                {
                    c.Logo = logoResult.Data;
                    hasChanges = true;
                }
            }

            // Update IvaId if changed
            if (c.IvaId != updateGeneralInfo.IvaId)
            {
                c.IvaId = updateGeneralInfo.IvaId;
                hasChanges = true;
            }

            // If no changes were detected, return early.
            // There is not a higher reason to alarm the user here.
            if (hasChanges)
            {
                _logger.LogInformation("No changes detected for GeneralInfo. No update performed.");
                return;
            }

            // Update UpdatedAt if there are changes
            c.UpdatedAt = DateTime.UtcNow;
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
