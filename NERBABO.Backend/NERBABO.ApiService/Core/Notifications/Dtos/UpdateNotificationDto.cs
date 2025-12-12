using System.ComponentModel.DataAnnotations;
using NERBABO.ApiService.Shared.Enums;

namespace NERBABO.ApiService.Core.Notifications.Dtos;

public class UpdateNotificationDto
{
    [Required(ErrorMessage = "Id é um campo obrigatório.")]
    public long Id { get; set; }

    public NotificationStatusEnum? Status { get; set; }
}
