using Humanizer;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Notifications.Dtos;
using NERBABO.ApiService.Core.People.Models;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Notifications.Models;

public class Notification : Entity<long>
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationTypeEnum Type { get; set; } = NotificationTypeEnum.GeneralAlert;
    public NotificationStatusEnum Status { get; set; } = NotificationStatusEnum.Unread;
    public long? RelatedPersonId { get; set; }
    public string? RelatedEntityType { get; set; } = string.Empty;
    public long? RelatedEntityId { get; set; }
    public string? ActionUrl { get; set; } = string.Empty;
    public DateTime? ReadAt { get; set; }
    public string? ReadByUserId { get; set; }

    // Navigation Properties
    public Person? RelatedPerson { get; set; }
    public User? ReadByUser { get; set; }

    public Notification() { }

    public static Notification ConvertCreateDtoToEntity(CreateNotificationDto notificationDto)
    {
        return new Notification
        {
            Title = notificationDto.Title,
            Message = notificationDto.Message,
            Type = notificationDto.Type,
            RelatedPersonId = notificationDto.RelatedPersonId,
            RelatedEntityType = notificationDto.RelatedEntityType,
            RelatedEntityId = notificationDto.RelatedEntityId,
            ActionUrl = notificationDto.ActionUrl,
            Status = NotificationStatusEnum.Unread,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static RetrieveNotificationDto ConvertEntityToRetrieveDto(Notification notification)
    {
        return new RetrieveNotificationDto
        {
            Id = notification.Id,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type.Humanize().Transform(To.TitleCase),
            Status = notification.Status.Humanize().Transform(To.TitleCase),
            RelatedPersonId = notification.RelatedPersonId,
            RelatedPersonName = notification.RelatedPerson != null ? notification.RelatedPerson.FullName : null,
            RelatedEntityType = notification.RelatedEntityType,
            RelatedEntityId = notification.RelatedEntityId,
            ActionUrl = notification.ActionUrl,
            CreatedAt = notification.CreatedAt,
            ReadAt = notification.ReadAt,
            ReadByUserId = notification.ReadByUserId
        };
    }
}
