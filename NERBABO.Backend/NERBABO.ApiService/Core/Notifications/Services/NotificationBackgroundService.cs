using NERBABO.ApiService.Core.Notifications.Services;

namespace NERBABO.ApiService.Core.Notifications.BackgroundServices;

/// <summary>
/// Background service that periodically generates notifications for missing documents and other issues.
/// Runs on startup and then every 6 hours.
/// </summary>
public class NotificationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(6); // Run every 6 hours

    public NotificationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<NotificationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NotificationBackgroundService started.");

        // Run immediately on startup
        await GenerateNotificationsAsync();

        // Then run periodically
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_interval, stoppingToken);
                await GenerateNotificationsAsync();
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("NotificationBackgroundService is stopping.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NotificationBackgroundService");
            }
        }
    }

    private async Task GenerateNotificationsAsync()
    {
        try
        {
            _logger.LogInformation("Generating notifications...");

            using var scope = _serviceProvider.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var result = await notificationService.GenerateNotificationsAsync();

            if (result.Success)
            {
                _logger.LogInformation("Notifications generated successfully: {Message}", result.Message);
            }
            else
            {
                _logger.LogWarning("Failed to generate notifications: {Message}", result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating notifications in background service");
        }
    }
}
