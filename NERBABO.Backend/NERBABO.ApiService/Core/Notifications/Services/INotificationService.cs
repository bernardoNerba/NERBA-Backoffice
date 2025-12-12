using NERBABO.ApiService.Core.Notifications.Dtos;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Notifications.Services;

public interface INotificationService
    : IGenericService<RetrieveNotificationDto, CreateNotificationDto, UpdateNotificationDto, long>
{
    /// <summary>
    /// Gets the count of notifications (total and unread).
    /// </summary>
    Task<Result<NotificationCountDto>> GetNotificationCountAsync();

    /// <summary>
    /// Gets all unread notifications.
    /// </summary>
    Task<Result<IEnumerable<RetrieveNotificationDto>>> GetUnreadNotificationsAsync();

    /// <summary>
    /// Marks a single notification as read.
    /// </summary>
    Task<Result<RetrieveNotificationDto>> MarkAsReadAsync(long notificationId, string userId);

    /// <summary>
    /// Marks all notifications as read.
    /// </summary>
    Task<Result> MarkAllAsReadAsync(string userId);

    /// <summary>
    /// Generates notifications using all registered notification generators.
    /// This method will create new notifications and cleanup resolved ones.
    /// </summary>
    Task<Result> GenerateNotificationsAsync();

    /// <summary>
    /// Deletes all notifications related to a specific person.
    /// Useful when a person's document is uploaded.
    /// </summary>
    Task<Result> DeleteNotificationsByPersonIdAsync(long personId);
}
