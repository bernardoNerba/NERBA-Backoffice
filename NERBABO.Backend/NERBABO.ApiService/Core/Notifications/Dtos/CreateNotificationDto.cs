using System.ComponentModel.DataAnnotations;
using NERBABO.ApiService.Shared.Enums;

namespace NERBABO.ApiService.Core.Notifications.Dtos;

public class CreateNotificationDto
{
    [Required(ErrorMessage = "Título é um campo obrigatório.")]
    [MaxLength(200, ErrorMessage = "Título deve conter até 200 caracteres.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mensagem é um campo obrigatório.")]
    [MaxLength(1000, ErrorMessage = "Mensagem deve conter até 1000 caracteres.")]
    public string Message { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tipo é um campo obrigatório.")]
    public NotificationTypeEnum Type { get; set; }

    public long? RelatedPersonId { get; set; }

    [MaxLength(100, ErrorMessage = "Tipo de Entidade deve conter até 100 caracteres.")]
    public string? RelatedEntityType { get; set; }

    public long? RelatedEntityId { get; set; }

    [MaxLength(500, ErrorMessage = "URL de Ação deve conter até 500 caracteres.")]
    public string? ActionUrl { get; set; }
}
