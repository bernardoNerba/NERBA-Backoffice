using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Notifications.Dtos;
using NERBABO.ApiService.Core.Notifications.Generators;
using NERBABO.ApiService.Core.Notifications.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Notifications.Services;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly AppDbContext _context;
    private readonly IEnumerable<INotificationGenerator> _notificationGenerators;

    public NotificationService(
        ILogger<NotificationService> logger,
        AppDbContext context,
        IEnumerable<INotificationGenerator> notificationGenerators)
    {
        _logger = logger;
        _context = context;
        _notificationGenerators = notificationGenerators;
    }

    public async Task<Result<RetrieveNotificationDto>> CreateAsync(CreateNotificationDto entityDto)
    {
        try
        {
            _logger.LogInformation("Creating new notification...");

            // Validate related person if provided
            if (entityDto.RelatedPersonId.HasValue)
            {
                var personExists = await _context.People.AnyAsync(p => p.Id == entityDto.RelatedPersonId.Value);
                if (!personExists)
                {
                    return Result<RetrieveNotificationDto>
                        .Fail("Não encontrado.", "Pessoa relacionada não encontrada.", StatusCodes.Status404NotFound);
                }
            }

            var notification = Notification.ConvertCreateDtoToEntity(entityDto);
            var createdNotification = _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var notificationToRetrieve = await _context.Notifications
                .Include(n => n.RelatedPerson)
                .Include(n => n.ReadByUser)
                .FirstOrDefaultAsync(n => n.Id == createdNotification.Entity.Id);

            if (notificationToRetrieve == null)
            {
                return Result<RetrieveNotificationDto>
                    .Fail("Erro ao criar notificação.", "Não foi possível recuperar a notificação criada.", StatusCodes.Status500InternalServerError);
            }

            var retrieveDto = Notification.ConvertEntityToRetrieveDto(notificationToRetrieve);

            _logger.LogInformation("Notification created successfully with ID {NotificationId}", retrieveDto.Id);

            return Result<RetrieveNotificationDto>
                .Ok(retrieveDto, "Notificação Criada.", $"Foi criada uma notificação: {retrieveDto.Title}.", StatusCodes.Status201Created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification");
            return Result<RetrieveNotificationDto>
                .Fail("Erro ao criar notificação.", "Ocorreu um erro ao criar a notificação.", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result> DeleteAsync(long id)
    {
        try
        {
            _logger.LogInformation("Deleting notification with ID {NotificationId}", id);

            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return Result
                    .Fail("Não encontrado.", "Notificação não encontrada.", StatusCodes.Status404NotFound);
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Notification deleted successfully with ID {NotificationId}", id);

            return Result
                .Ok("Notificação Eliminada.", $"A notificação foi eliminada com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification with ID {NotificationId}", id);
            return Result
                .Fail("Erro ao eliminar notificação.", "Ocorreu um erro ao eliminar a notificação.", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<IEnumerable<RetrieveNotificationDto>>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Getting all notifications...");

            var notifications = await _context.Notifications
                .Include(n => n.RelatedPerson)
                .Include(n => n.ReadByUser)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            if (!notifications.Any())
            {
                return Result<IEnumerable<RetrieveNotificationDto>>
                    .Fail("Não encontrado.", "Não existem notificações no sistema.", StatusCodes.Status404NotFound);
            }

            var retrieveDtos = notifications.Select(Notification.ConvertEntityToRetrieveDto);

            return Result<IEnumerable<RetrieveNotificationDto>>
                .Ok(retrieveDtos, "Notificações Encontradas.", $"Foram encontradas {notifications.Count} notificações.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all notifications");
            return Result<IEnumerable<RetrieveNotificationDto>>
                .Fail("Erro ao obter notificações.", "Ocorreu um erro ao obter as notificações.", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<RetrieveNotificationDto>> GetByIdAsync(long id)
    {
        try
        {
            _logger.LogInformation("Getting notification with ID {NotificationId}", id);

            var notification = await _context.Notifications
                .Include(n => n.RelatedPerson)
                .Include(n => n.ReadByUser)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (notification == null)
            {
                return Result<RetrieveNotificationDto>
                    .Fail("Não encontrado.", "Notificação não encontrada.", StatusCodes.Status404NotFound);
            }

            var retrieveDto = Notification.ConvertEntityToRetrieveDto(notification);

            return Result<RetrieveNotificationDto>
                .Ok(retrieveDto, "Notificação Encontrada.", $"Foi encontrada a notificação: {retrieveDto.Title}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification with ID {NotificationId}", id);
            return Result<RetrieveNotificationDto>
                .Fail("Erro ao obter notificação.", "Ocorreu um erro ao obter a notificação.", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<RetrieveNotificationDto>> UpdateAsync(UpdateNotificationDto entityDto)
    {
        try
        {
            _logger.LogInformation("Updating notification with ID {NotificationId}", entityDto.Id);

            var notification = await _context.Notifications.FindAsync(entityDto.Id);
            if (notification == null)
            {
                return Result<RetrieveNotificationDto>
                    .Fail("Não encontrado.", "Notificação não encontrada.", StatusCodes.Status404NotFound);
            }

            // Update only the status if provided
            if (entityDto.Status.HasValue)
            {
                notification.Status = entityDto.Status.Value;
                notification.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            var updatedNotification = await _context.Notifications
                .Include(n => n.RelatedPerson)
                .Include(n => n.ReadByUser)
                .FirstOrDefaultAsync(n => n.Id == entityDto.Id);

            if (updatedNotification == null)
            {
                return Result<RetrieveNotificationDto>
                    .Fail("Erro ao atualizar notificação.", "Não foi possível recuperar a notificação atualizada.", StatusCodes.Status500InternalServerError);
            }

            var retrieveDto = Notification.ConvertEntityToRetrieveDto(updatedNotification);

            _logger.LogInformation("Notification updated successfully with ID {NotificationId}", retrieveDto.Id);

            return Result<RetrieveNotificationDto>
                .Ok(retrieveDto, "Notificação Atualizada.", $"A notificação foi atualizada com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating notification with ID {NotificationId}", entityDto.Id);
            return Result<RetrieveNotificationDto>
                .Fail("Erro ao atualizar notificação.", "Ocorreu um erro ao atualizar a notificação.", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<NotificationCountDto>> GetNotificationCountAsync()
    {
        try
        {
            _logger.LogInformation("Getting notification count...");

            var totalCount = await _context.Notifications.CountAsync();
            var unreadCount = await _context.Notifications
                .Where(n => n.Status == NotificationStatusEnum.Unread)
                .CountAsync();

            var countDto = new NotificationCountDto
            {
                TotalCount = totalCount,
                UnreadCount = unreadCount
            };

            return Result<NotificationCountDto>
                .Ok(countDto, "Contagem de Notificações.", $"Total: {totalCount}, Não Lidas: {unreadCount}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification count");
            return Result<NotificationCountDto>
                .Fail("Erro ao obter contagem.", "Ocorreu um erro ao obter a contagem de notificações.", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<IEnumerable<RetrieveNotificationDto>>> GetUnreadNotificationsAsync()
    {
        try
        {
            _logger.LogInformation("Getting unread notifications...");

            var notifications = await _context.Notifications
                .Include(n => n.RelatedPerson)
                .Include(n => n.ReadByUser)
                .Where(n => n.Status == NotificationStatusEnum.Unread)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            if (!notifications.Any())
            {
                return Result<IEnumerable<RetrieveNotificationDto>>
                    .Fail("Não encontrado.", "Não existem notificações não lidas.", StatusCodes.Status404NotFound);
            }

            var retrieveDtos = notifications.Select(Notification.ConvertEntityToRetrieveDto);

            return Result<IEnumerable<RetrieveNotificationDto>>
                .Ok(retrieveDtos, "Notificações Não Lidas Encontradas.", $"Foram encontradas {notifications.Count} notificações não lidas.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread notifications");
            return Result<IEnumerable<RetrieveNotificationDto>>
                .Fail("Erro ao obter notificações.", "Ocorreu um erro ao obter as notificações não lidas.", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<RetrieveNotificationDto>> MarkAsReadAsync(long notificationId, string userId)
    {
        try
        {
            _logger.LogInformation("Marking notification {NotificationId} as read by user {UserId}", notificationId, userId);

            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null)
            {
                return Result<RetrieveNotificationDto>
                    .Fail("Não encontrado.", "Notificação não encontrada.", StatusCodes.Status404NotFound);
            }

            notification.Status = NotificationStatusEnum.Read;
            notification.ReadAt = DateTime.UtcNow;
            notification.ReadByUserId = userId;
            notification.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var updatedNotification = await _context.Notifications
                .Include(n => n.RelatedPerson)
                .Include(n => n.ReadByUser)
                .FirstOrDefaultAsync(n => n.Id == notificationId);

            if (updatedNotification == null)
            {
                return Result<RetrieveNotificationDto>
                    .Fail("Erro ao marcar como lida.", "Não foi possível recuperar a notificação atualizada.", StatusCodes.Status500InternalServerError);
            }

            var retrieveDto = Notification.ConvertEntityToRetrieveDto(updatedNotification);

            _logger.LogInformation("Notification {NotificationId} marked as read successfully", notificationId);

            return Result<RetrieveNotificationDto>
                .Ok(retrieveDto, "Notificação Marcada Como Lida.", "A notificação foi marcada como lida com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as read", notificationId);
            return Result<RetrieveNotificationDto>
                .Fail("Erro ao marcar como lida.", "Ocorreu um erro ao marcar a notificação como lida.", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result> MarkAllAsReadAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Marking all notifications as read for user {UserId}", userId);

            var unreadNotifications = await _context.Notifications
                .Where(n => n.Status == NotificationStatusEnum.Unread)
                .ToListAsync();

            if (!unreadNotifications.Any())
            {
                return Result
                    .Ok("Sem Notificações.", "Não existem notificações não lidas para marcar como lidas.");
            }

            foreach (var notification in unreadNotifications)
            {
                notification.Status = NotificationStatusEnum.Read;
                notification.ReadAt = DateTime.UtcNow;
                notification.ReadByUserId = userId;
                notification.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Marked {Count} notifications as read", unreadNotifications.Count);

            return Result
                .Ok("Notificações Marcadas Como Lidas.", $"{unreadNotifications.Count} notificações foram marcadas como lidas.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read");
            return Result
                .Fail("Erro ao marcar como lidas.", "Ocorreu um erro ao marcar todas as notificações como lidas.", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result> GenerateNotificationsAsync()
    {
        try
        {
            _logger.LogInformation("Starting notification generation process...");

            var totalGenerated = 0;
            var totalCleaned = 0;

            foreach (var generator in _notificationGenerators)
            {
                _logger.LogInformation("Running generator: {GeneratorId} - {Description}",
                    generator.GeneratorId, generator.Description);

                // Generate new notifications
                var notificationsToCreate = await generator.GenerateNotificationsAsync();
                if (notificationsToCreate.Any())
                {
                    _context.Notifications.AddRange(notificationsToCreate);
                    totalGenerated += notificationsToCreate.Count();
                    _logger.LogInformation("Generator {GeneratorId} created {Count} notifications",
                        generator.GeneratorId, notificationsToCreate.Count());
                }

                // Cleanup resolved notifications
                var notificationIdsToCleanup = await generator.GetNotificationsToCleanupAsync();
                if (notificationIdsToCleanup.Any())
                {
                    var notificationsToDelete = await _context.Notifications
                        .Where(n => notificationIdsToCleanup.Contains(n.Id))
                        .ToListAsync();

                    _context.Notifications.RemoveRange(notificationsToDelete);
                    totalCleaned += notificationsToDelete.Count;
                    _logger.LogInformation("Generator {GeneratorId} cleaned up {Count} notifications",
                        generator.GeneratorId, notificationsToDelete.Count);
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Notification generation completed. Generated: {Generated}, Cleaned: {Cleaned}",
                totalGenerated, totalCleaned);

            return Result
                .Ok("Notificações Geradas.", $"Foram criadas {totalGenerated} notificações e removidas {totalCleaned} notificações resolvidas.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating notifications");
            return Result
                .Fail("Erro ao gerar notificações.", "Ocorreu um erro ao gerar as notificações.", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result> DeleteNotificationsByPersonIdAsync(long personId)
    {
        try
        {
            _logger.LogInformation("Deleting notifications for person with ID {PersonId}", personId);

            var notifications = await _context.Notifications
                .Where(n => n.RelatedPersonId == personId)
                .ToListAsync();

            if (!notifications.Any())
            {
                _logger.LogInformation("No notifications found for person with ID {PersonId}", personId);
                return Result.Ok("Sem Notificações.", "Não existem notificações para esta pessoa.");
            }

            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted {Count} notifications for person with ID {PersonId}",
                notifications.Count, personId);

            return Result
                .Ok("Notificações Eliminadas.", $"{notifications.Count} notificações foram eliminadas para esta pessoa.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notifications for person with ID {PersonId}", personId);
            return Result
                .Fail("Erro ao eliminar notificações.", "Ocorreu um erro ao eliminar as notificações.", StatusCodes.Status500InternalServerError);
        }
    }
}
