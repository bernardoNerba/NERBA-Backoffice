using NERBABO.ApiService.Core.Notifications.Models;

namespace NERBABO.ApiService.Core.Notifications.Generators;

/// <summary>
/// Interface for notification generators.
/// Each generator is responsible for checking specific conditions and generating notifications.
/// Implementing this interface allows for easy extension of notification types without modifying existing code.
/// </summary>
public interface INotificationGenerator
{
    /// <summary>
    /// Gets the unique identifier for this generator.
    /// Used for logging and identification purposes.
    /// </summary>
    string GeneratorId { get; }

    /// <summary>
    /// Gets a description of what this generator checks and notifies about.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Generates notifications based on the specific conditions this generator checks.
    /// Returns a list of notifications that should be created.
    /// </summary>
    /// <returns>A list of notifications to be created, or an empty list if no notifications are needed.</returns>
    Task<IEnumerable<Notification>> GenerateNotificationsAsync();

    /// <summary>
    /// Optional: Cleans up notifications that are no longer valid.
    /// For example, if a missing document is now uploaded, this can remove the notification.
    /// Returns the IDs of notifications that should be deleted.
    /// </summary>
    /// <returns>A list of notification IDs to be deleted, or an empty list if no cleanup is needed.</returns>
    Task<IEnumerable<long>> GetNotificationsToCleanupAsync();
}
