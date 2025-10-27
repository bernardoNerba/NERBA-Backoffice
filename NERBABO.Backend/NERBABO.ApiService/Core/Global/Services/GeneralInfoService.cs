using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Global.Dtos;
using NERBABO.ApiService.Core.Global.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;
using StackExchange.Redis;
using ZLinq;

namespace NERBABO.ApiService.Core.Global.Services;

public class GeneralInfoService(
    AppDbContext context,
    ILogger<GeneralInfoService> logger,
    IImageService imageService,
    IConnectionMultiplexer redis
    ) : IGeneralInfoService
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<GeneralInfoService> _logger = logger;
    private readonly IImageService _imageService = imageService;
    private readonly IConnectionMultiplexer _redis = redis;
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

        // Selective field updates - only update fields that have changed
        bool hasChanges = false;
        await UpdateConfigurationAsync(async c =>
        {

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

            // Update Email if changed
            if (!string.Equals(c.Email, updateGeneralInfo.Email))
            {
                c.Email = updateGeneralInfo.Email;
                hasChanges = true;
            }

            // Update Slug if changed
            if (!string.Equals(c.Slug, updateGeneralInfo.Slug))
            {
                c.Slug = updateGeneralInfo.Slug;
                hasChanges = true;
            }

            // Update PhoneNumber if changed
            if (!string.Equals(c.PhoneNumber, updateGeneralInfo.PhoneNumber))
            {
                c.PhoneNumber = updateGeneralInfo.PhoneNumber;
                hasChanges = true;
            }

            // Update Website if changed
            if (!string.Equals(c.Website, updateGeneralInfo.Website))
            {
                c.Website = updateGeneralInfo.Website;
                hasChanges = true;
            }

            // Update UpdatedAt if there are changes
            c.UpdatedAt = DateTime.UtcNow;
        });
        
        if (!hasChanges)
        {
            _logger.LogInformation("No changes detected for GeneralInfo. No update performed.");
            return Result
                .Fail("Erro de Validação", "Não foram detectadas mudanças nos dados fornecidos.");
        }

        return Result
            .Ok("Informação Atualizada.", "Foram atualizadas as configurações gerais.");
    }

    public async Task<Result<object>> HealthCheckAsync()
    {
        var healthStatus = new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            Checks = new List<object>()
        };

        var checks = healthStatus.Checks;
        var overallHealthy = true;

        // Check database connectivity
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            checks.Add(new
            {
                Name = "Database",
                Status = canConnect ? "Healthy" : "Unhealthy",
                Description = canConnect ? "Database connection successful" : "Cannot connect to database"
            });

            if (!canConnect)
                overallHealthy = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            checks.Add(new
            {
                Name = "Database",
                Status = "Unhealthy",
                Description = $"Database check failed: {ex.Message}"
            });
            overallHealthy = false;
        }

        // Check Redis connectivity
        try
        {
            var database = _redis.GetDatabase();
            await database.PingAsync();
            checks.Add(new
            {
                Name = "Redis",
                Status = "Healthy",
                Description = "Redis connection successful"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis health check failed");
            checks.Add(new
            {
                Name = "Redis",
                Status = "Unhealthy",
                Description = $"Redis check failed: {ex.Message}"
            });
            overallHealthy = false;
        }

        // Check disk space for file uploads
        try
        {
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (Directory.Exists(uploadPath))
            {
                var drive = new DriveInfo(Path.GetPathRoot(uploadPath)!);
                var freeSpaceGB = drive.AvailableFreeSpace / (1024 * 1024 * 1024);
                var isHealthy = freeSpaceGB > 1; // At least 1GB free space

                checks.Add(new
                {
                    Name = "DiskSpace",
                    Status = isHealthy ? "Healthy" : "Warning",
                    Description = $"Available space: {freeSpaceGB:F2} GB",
                    FreeSpaceGB = freeSpaceGB
                });

                if (!isHealthy)
                    overallHealthy = false;
            }
            else
            {
                checks.Add(new
                {
                    Name = "DiskSpace",
                    Status = "Warning",
                    Description = "Upload directory does not exist"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Disk space health check failed");
            checks.Add(new
            {
                Name = "DiskSpace",
                Status = "Warning",
                Description = $"Disk space check failed: {ex.Message}"
            });
        }

        // Update overall status
        var result = new
        {
            Status = overallHealthy ? "Healthy" : "Unhealthy",
            Timestamp = healthStatus.Timestamp,
            Version = healthStatus.Version,
            Environment = healthStatus.Environment,
            Checks = checks
        };

        if (overallHealthy)
        {
            return Result<object>.Ok(result, "Application is healthy and ready to serve requests.", "", StatusCodes.Status200OK);
        }
        else
        {
            return new Result<object>
            {
                Success = false,
                Title = "Application is not healthy - database or cache issues.",
                Message = "",
                StatusCode = StatusCodes.Status503ServiceUnavailable,
                Data = result
            };
        }
    }

    public Task<Result<object>> AliveAsync()
    {
        var result = new
        {
            Status = "Alive",
            Timestamp = DateTime.UtcNow,
            Message = "Application is running"
        };

        return Task.FromResult(Result<object>.Ok(result, "Application is alive and responding.", "", StatusCodes.Status200OK));
    }

    public async Task<Result<object>> ReadyAsync()
    {
        try
        {
            // Check if database is ready and migrated
            var canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                var notReadyResult = new
                {
                    Status = "NotReady",
                    Timestamp = DateTime.UtcNow,
                    Message = "Database not available"
                };
                return new Result<object>
                {
                    Success = false,
                    Title = "Application is not ready - still initializing.",
                    Message = "",
                    StatusCode = StatusCodes.Status503ServiceUnavailable,
                    Data = notReadyResult
                };
            }

            // Check if migrations are applied (look for migration history table)
            var migrationsApplied = await _context.Database
                .GetAppliedMigrationsAsync();
            
            if (!migrationsApplied.Any())
            {
                var notReadyResult = new
                {
                    Status = "NotReady",
                    Timestamp = DateTime.UtcNow,
                    Message = "Database migrations not applied"
                };
                return new Result<object>
                {
                    Success = false,
                    Title = "Application is not ready - still initializing.",
                    Message = "",
                    StatusCode = StatusCodes.Status503ServiceUnavailable,
                    Data = notReadyResult
                };
            }

            // Check Redis connectivity
            var database = _redis.GetDatabase();
            await database.PingAsync();

            var readyResult = new
            {
                Status = "Ready",
                Timestamp = DateTime.UtcNow,
                Message = "Application is ready to serve requests",
                MigrationsCount = migrationsApplied.Count()
            };

            return Result<object>.Ok(readyResult, "Application is ready to serve requests.", "", StatusCodes.Status200OK);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Readiness check failed");
            var errorResult = new
            {
                Status = "NotReady",
                Timestamp = DateTime.UtcNow,
                Message = $"Readiness check failed: {ex.Message}"
            };
            return new Result<object>
            {
                Success = false,
                Title = "Application is not ready - still initializing.",
                Message = "",
                StatusCode = StatusCodes.Status503ServiceUnavailable,
                Data = errorResult
            };
        }
    }


    #region Private Helper Methods
    private async Task UpdateConfigurationAsync(Func<GeneralInfo, Task> updateAction)
    {
        await _cacheLock.WaitAsync();
        try
        {
            var config = await _context.GeneralInfo.FirstAsync();
            await updateAction(config);
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