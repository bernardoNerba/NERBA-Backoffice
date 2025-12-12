using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Notifications.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Enums;

namespace NERBABO.ApiService.Core.Notifications.Generators;

/// <summary>
/// Generates notifications for people who are missing required documents:
/// - Identification Document (required when Habilitation != WithoutProof)
/// - Habilitation Comprovative (required when Habilitation != WithoutProof)
/// - IBAN Comprovative (required when IBAN is not null/empty)
/// </summary>
public class MissingPersonDocumentNotificationGenerator : INotificationGenerator
{
    private readonly AppDbContext _context;
    private readonly ILogger<MissingPersonDocumentNotificationGenerator> _logger;

    public string GeneratorId => "MissingPersonDocument";
    public string Description => "Verifica pessoas com documentos em falta (Identificação, Habilitações, IBAN).";

    public MissingPersonDocumentNotificationGenerator(
        AppDbContext context,
        ILogger<MissingPersonDocumentNotificationGenerator> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Notification>> GenerateNotificationsAsync()
    {
        try
        {
            _logger.LogInformation("Generating missing person document notifications...");

            // Find all people with habilitation != "Sem Comprovativo"
            var people = await _context.People
                .Where(p => p.Habilitation != HabilitationEnum.WithoutProof)
                .ToListAsync();

            _logger.LogInformation($"Checking {people.Count} people for missing documents.");

            var notificationsToCreate = new List<Notification>();

            foreach (var person in people)
            {
                // Check each document type and collect missing ones
                var missingDocuments = new List<string>();

                // 1. Check Identification Document
                if (person.IdentificationDocumentPdfId == null)
                {
                    missingDocuments.Add("Cópia do Documento de Identificação");
                }

                // 2. Check Habilitation Comprovative
                if (person.HabilitationComprovativePdfId == null)
                {
                    missingDocuments.Add("Comprovativo de Habilitações");
                }

                // 3. Check IBAN Comprovative (only if IBAN is filled)
                if (!string.IsNullOrEmpty(person.IBAN) && person.IbanComprovativePdfId == null)
                {
                    missingDocuments.Add("Comprovativo de IBAN");
                }

                // Check if ANY notification already exists for this person (including both read and unread)
                // We check ALL statuses to prevent duplicates when users mark notifications as read
                var allExistingNotifications = await _context.Notifications
                    .Where(n => n.RelatedPersonId == person.Id
                        && n.Type == NotificationTypeEnum.MissingDocument)
                    .ToListAsync();

                // If no missing documents, remove all existing notifications for this person
                if (!missingDocuments.Any())
                {
                    if (allExistingNotifications.Any())
                    {
                        _context.Notifications.RemoveRange(allExistingNotifications);
                        _logger.LogInformation($"Removed {allExistingNotifications.Count} notifications for person {person.Id} - all documents now present");
                    }
                    continue;
                }

                // Only create/update notification if there are missing documents
                if (missingDocuments.Any())
                {
                    // Build the message with the list of missing documents as bullet points
                    var documentList = string.Join("\n", missingDocuments.Select(doc => $"- {doc}"));

                    var title = missingDocuments.Count == 1
                        ? "Documento em Falta"
                        : "Documentos em Falta";

                    var message = $"Documentação em falta para {person.FirstName} {person.LastName}.\n" +
                                $"Os seguintes ficheiros não foram submetidos:\n{documentList}";

                    // Normalize message for comparison
                    var normalizedMessage = message.Replace("\r\n", "\n").Trim();

                    // Find the current format unread notification if it exists
                    var unreadNotifications = allExistingNotifications
                        .Where(n => n.RelatedEntityType == "MissingDocuments" && n.Status == NotificationStatusEnum.Unread)
                        .ToList();

                    var currentUnreadNotification = unreadNotifications.FirstOrDefault();

                    if (unreadNotifications.Count > 1)
                    {
                        var duplicates = unreadNotifications.Skip(1).ToList();
                        _context.Notifications.RemoveRange(duplicates);
                        _logger.LogInformation($"Removed {duplicates.Count} duplicate unread notifications for person {person.Id}");
                    }

                    // Find any legacy notifications (old format) - both read and unread
                    var legacyNotifications = allExistingNotifications
                        .Where(n => n.RelatedEntityType != "MissingDocuments")
                        .ToList();

                    // Find read notifications with current format
                    var readNotifications = allExistingNotifications
                        .Where(n => n.RelatedEntityType == "MissingDocuments" && n.Status != NotificationStatusEnum.Unread)
                        .ToList();

                    // Remove all legacy notifications for this person
                    if (legacyNotifications.Any())
                    {
                        _context.Notifications.RemoveRange(legacyNotifications);
                        _logger.LogInformation($"Removed {legacyNotifications.Count} legacy notifications for person {person.Id}");
                    }

                    // Remove old read notifications if they have the same content as what we'd create
                    // This prevents duplicates when users mark notifications as read
                    var duplicateReadNotifications = readNotifications
                        .Where(n => n.Message != null && n.Message.Replace("\r\n", "\n").Trim() == normalizedMessage)
                        .ToList();

                    if (duplicateReadNotifications.Any())
                    {
                        // If there's a read notification with the exact same content, don't create a new unread one
                        _logger.LogInformation($"Found {duplicateReadNotifications.Count} read notifications with same content for person {person.Id} - skipping creation");

                        // If there's no unread notification but there's a read one with same content, we're done
                        if (currentUnreadNotification == null)
                        {
                            continue;
                        }
                    }

                    // Remove read notifications that are outdated (different content)
                    var outdatedReadNotifications = readNotifications
                        .Where(n => n.Message == null || n.Message.Replace("\r\n", "\n").Trim() != normalizedMessage)
                        .ToList();

                    if (outdatedReadNotifications.Any())
                    {
                        _context.Notifications.RemoveRange(outdatedReadNotifications);
                        _logger.LogInformation($"Removed {outdatedReadNotifications.Count} outdated read notifications for person {person.Id}");
                    }

                    if (currentUnreadNotification != null)
                    {
                        // Update existing unread notification if the list of missing documents changed
                        if (currentUnreadNotification.Message?.Replace("\r\n", "\n").Trim() != normalizedMessage)
                        {
                            currentUnreadNotification.Title = title;
                            currentUnreadNotification.Message = message;
                            currentUnreadNotification.UpdatedAt = DateTime.UtcNow;
                            _logger.LogInformation($"Updated notification for person {person.Id} with new missing documents list");
                        }
                    }
                    else
                    {
                        // Only create new notification if there isn't already a read one with the same content
                        if (!duplicateReadNotifications.Any())
                        {
                            // Create new notification
                            var notification = new Notification
                            {
                                Title = title,
                                Message = message,
                                Type = NotificationTypeEnum.MissingDocument,
                                Status = NotificationStatusEnum.Unread,
                                RelatedPersonId = person.Id,
                                RelatedEntityType = "MissingDocuments",
                                RelatedEntityId = person.Id,
                                ActionUrl = $"/people/{person.Id}",
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };

                            notificationsToCreate.Add(notification);
                        }
                    }
                }
            }

            _logger.LogInformation($"Created {notificationsToCreate.Count} new notifications.");

            return notificationsToCreate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating missing person document notifications.");
            return Enumerable.Empty<Notification>();
        }
    }

    public async Task<IEnumerable<long>> GetNotificationsToCleanupAsync()
    {
        try
        {
            _logger.LogInformation("Cleaning up resolved missing person document notifications...");

            var notificationsToCleanup = new List<long>();

            // Get all unread missing document notifications
            var notifications = await _context.Notifications
                .Where(n => n.Type == NotificationTypeEnum.MissingDocument
                    && n.RelatedPersonId != null
                    && n.Status == NotificationStatusEnum.Unread)
                .Join(_context.People,
                    notification => notification.RelatedPersonId,
                    person => (long?)person.Id,
                    (notification, person) => new { Notification = notification, Person = person })
                .ToListAsync();

            foreach (var np in notifications)
            {
                var shouldCleanup = false;

                // For the new format with RelatedEntityType = "MissingDocuments"
                if (np.Notification.RelatedEntityType == "MissingDocuments")
                {
                    // Check if all documents are now present or habilitation changed to WithoutProof
                    var hasAllRequiredDocuments = np.Person.IdentificationDocumentPdfId != null
                        && np.Person.HabilitationComprovativePdfId != null
                        && (string.IsNullOrEmpty(np.Person.IBAN) || np.Person.IbanComprovativePdfId != null);

                    shouldCleanup = hasAllRequiredDocuments || np.Person.Habilitation == HabilitationEnum.WithoutProof;
                }
                else
                {
                    // Legacy notifications - cleanup based on document type
                    switch (np.Notification.RelatedEntityType)
                    {
                        case "IdentificationDocument":
                            shouldCleanup = np.Person.IdentificationDocumentPdfId != null
                                || np.Person.Habilitation == HabilitationEnum.WithoutProof;
                            break;

                        case "HabilitationComprovative":
                            shouldCleanup = np.Person.HabilitationComprovativePdfId != null
                                || np.Person.Habilitation == HabilitationEnum.WithoutProof;
                            break;

                        case "IbanComprovative":
                            shouldCleanup = np.Person.IbanComprovativePdfId != null
                                || string.IsNullOrEmpty(np.Person.IBAN);
                            break;

                        default:
                            // Very old format with RelatedEntityType = "Person"
                            shouldCleanup = np.Person.IdentificationDocumentPdfId != null
                                || np.Person.Habilitation == HabilitationEnum.WithoutProof;
                            break;
                    }
                }

                if (shouldCleanup)
                {
                    notificationsToCleanup.Add(np.Notification.Id);
                }
            }

            _logger.LogInformation($"Found {notificationsToCleanup.Count} notifications to cleanup.");

            return notificationsToCleanup;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up missing person document notifications.");
            return Enumerable.Empty<long>();
        }
    }
}
