# Notification System Feature - NERBA Backoffice

## Overview
This document describes the implemented notification system that alerts users about missing documentation for people in the NERBA Backoffice system.

## Business Requirements

### Notification Criteria
The system generates **one notification per person** when they meet ALL of the following conditions:
1. **Habilitation Level** is NOT "Sem Comprovativo" (WithoutProof)
2. **Missing Documents**: One or more required documents are not uploaded

### Required Documents
For each person with `Habilitation != WithoutProof`, the system checks:
- **Cópia do Documento de Identificação** (always required)
- **Comprovativo de Habilitações** (always required)
- **Comprovativo de IBAN** (required only if IBAN field is filled)

### Notification Behavior
- **Single Notification**: One notification per person lists ALL missing documents
- **Auto-Update**: Notification message updates automatically when documents are added/removed
- **Auto-Cleanup**: Notification is removed when all required documents are uploaded
- **Bulk Import**: Notifications are generated automatically after bulk imports

### User Experience
- The navbar "Notificações" menu displays a badge with unread notification count
- Clicking "Notificações" navigates to `/notifications` page
- The notifications page displays all notifications with filtering and sorting options
- Each notification links to the person's profile for easy resolution
- **Permissions**:
  - All authenticated users can view and mark notifications as read
  - Only **Admin** users can delete notifications

### Notification Message Format (Portuguese PT)

**Title** (adapts based on count):
- Single document: `"Documento em Falta"`
- Multiple documents: `"Documentos em Falta"`

**Message** (with bullet points):
```
Documentação em falta para {FirstName} {LastName}.
Os seguintes ficheiros não foram submetidos:
- Cópia do Documento de Identificação
- Comprovativo de Habilitações
- Comprovativo de IBAN
```

---

## Technical Implementation

### Architecture Overview

The notification system uses a **Generator Pattern** where:
1. `NotificationService` orchestrates notification generation
2. `MissingPersonDocumentNotificationGenerator` implements the business logic
3. `NotificationBackgroundService` runs generation periodically
4. Integration points trigger generation when data changes

### Backend Implementation

#### 1. Database Schema

**Notification Entity**
- **Location**: `NERBABO.Backend/NERBABO.ApiService/Core/Notifications/Models/Notification.cs`

```csharp
public class Notification : Entity<long>
{
    public string Title { get; set; }              // "Documento em Falta" or "Documentos em Falta"
    public string Message { get; set; }            // Multi-line message with bullet list
    public NotificationTypeEnum Type { get; set; }  // MissingDocument
    public NotificationStatusEnum Status { get; set; } // Unread/Read/Archived
    public long? RelatedPersonId { get; set; }     // Person with missing documents
    public string? RelatedEntityType { get; set; } // "MissingDocuments"
    public long? RelatedEntityId { get; set; }     // Person ID
    public string? ActionUrl { get; set; }         // "/people/{personId}"
    public DateTime? ReadAt { get; set; }          // When marked as read
    public string? ReadByUserId { get; set; }      // User who read it

    // Navigation Properties
    public Person? RelatedPerson { get; set; }
    public User? ReadByUser { get; set; }
}
```

**Database Configuration**
- **Location**: `NERBABO.Backend/NERBABO.ApiService/Data/Configurations/NotificationConfiguration.cs`
- Indexes on: `Status`, `Type`, `RelatedPersonId`, `CreatedAt`
- Cascade delete when person is deleted
- Message field max length: 1000 characters (supports multi-line content)

#### 2. Notification Generator

**MissingPersonDocumentNotificationGenerator**
- **Location**: `NERBABO.Backend/NERBABO.ApiService/Core/Notifications/Generators/MissingPersonDocumentNotificationGenerator.cs`

**Logic Flow**:
```csharp
1. Query all people where Habilitation != WithoutProof
2. For each person:
   a. Check IdentificationDocumentPdfId (always required)
   b. Check HabilitationComprovativePdfId (always required)
   c. Check IbanComprovativePdfId (if IBAN field is not empty)
3. Build bullet-point list of missing documents
4. Check if notification exists:
   - If exists: Update message if list changed
   - If not: Create new notification
5. Cleanup: Remove notifications where all documents are now present
```

**Key Features**:
- Avoids duplicates by checking existing notifications
- Updates existing notification instead of creating new ones
- Maintains backward compatibility with legacy notification formats
- Uses `RelatedEntityType = "MissingDocuments"` for tracking

#### 3. Service Layer

**NotificationService**
- **Location**: `NERBABO.Backend/NERBABO.ApiService/Core/Notifications/Services/NotificationService.cs`

**Key Methods**:
- `GenerateNotificationsAsync()` - Runs all registered generators
- `GetNotificationCountAsync()` - Returns total and unread counts
- `GetUnreadNotificationsAsync()` - Returns unread notifications
- `MarkAsReadAsync(id, userId)` - Marks single notification as read
- `MarkAllAsReadAsync(userId)` - Marks all as read
- `DeleteAsync(id)` - Deletes notification (Admin only)

#### 4. API Endpoints

**NotificationController**
- **Location**: `NERBABO.Backend/NERBABO.ApiService/Core/Notifications/Controllers/NotificationController.cs`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/notifications` | ActiveUser | Get all notifications |
| GET | `/api/notifications/{id}` | ActiveUser | Get notification by ID |
| GET | `/api/notifications/unread` | ActiveUser | Get unread notifications |
| GET | `/api/notifications/count` | ActiveUser | Get notification counts |
| POST | `/api/notifications/create` | ActiveUser | Create notification manually |
| PUT | `/api/notifications/update/{id}` | ActiveUser | Update notification |
| PUT | `/api/notifications/{id}/mark-read` | ActiveUser | Mark as read |
| PUT | `/api/notifications/mark-all-read` | ActiveUser | Mark all as read |
| DELETE | `/api/notifications/delete/{id}` | **Admin** | Delete notification |
| POST | `/api/notifications/generate` | ActiveUser | Trigger generation |

#### 5. Integration Points

**PeopleService Integration**
- **Location**: `NERBABO.Backend/NERBABO.ApiService/Core/People/Services/PeopleService.cs`

Notification generation is triggered after:
- `UploadIdentificationDocumentPdfAsync()` - Regenerates to update/remove notification
- `DeleteIdentificationDocumentPdfAsync()` - Regenerates to create/update notification
- `UploadHabilitationPdfAsync()` - Regenerates to update/remove notification
- `DeleteHabilitationPdfAsync()` - Regenerates to create/update notification
- `UploadIbanPdfAsync()` - Regenerates to update/remove notification
- `DeleteIbanPdfAsync()` - Regenerates to create/update notification

**Bulk Import Integration**
- **Location**: `NERBABO.Backend/NERBABO.ApiService/Core/People/BulkImport/Services/PeopleBulkImportService.cs`

After successful bulk import:
```csharp
if (result.SuccessCount > 0)
{
    _logger.LogInformation("Generating notifications for imported people...");
    await _notificationService.GenerateNotificationsAsync();
}
```

#### 6. Background Service

**NotificationBackgroundService**
- **Location**: `NERBABO.Backend/NERBABO.ApiService/Core/Notifications/Services/NotificationBackgroundService.cs`
- Runs immediately on startup, then every 6 hours
- Automatically scans for missing documents and updates notifications
- Uses `BackgroundService` base class
- Graceful cancellation support
- Configurable interval via `TimeSpan.FromHours(6)`

---

### Frontend Implementation

#### 1. Models & Types

**Location**: `NERBABO.Frontend/src/app/core/models/notification.ts`

```typescript
export type Notification = {
  id: number;
  title: string;
  message: string;               // Multi-line with \n characters
  type: string;
  status: string;
  relatedPersonId?: number;
  relatedPersonName?: string;
  relatedEntityType?: string;    // "MissingDocuments"
  relatedEntityId?: number;
  actionUrl?: string;            // "/people/{id}"
  createdAt: string;
  readAt?: string;
  readByUserId?: string;
};
```

#### 2. Notification Service

**Location**: `NERBABO.Frontend/src/app/core/services/notification.service.ts`

**Features**:
- Reactive state management with RxJS BehaviorSubjects
- Automatic polling (every 60 seconds) for notification count
- Centralized loading state
- Error handling with SharedService integration

**Key Methods**:
- `getAllNotifications()` - Fetches all notifications
- `getUnreadNotifications()` - Fetches unread only
- `getNotificationCount()` - Returns count (total/unread)
- `markAsRead(id)` - Marks single as read
- `markAllAsRead()` - Marks all as read
- `deleteNotification(id)` - Deletes (Admin only)
- `generateNotifications()` - Triggers generation manually

#### 3. navbar Integration

**Location**: `NERBABO.Frontend/src/app/shared/components/navbar/`

**Changes**:
- Subscribes to `notificationCount$` observable
- Displays badge with unread count
- Badge styled with red background (#dc3545)
- Auto-updates in real-time via polling

#### 4. Notifications Page

**Location**: `NERBABO.Frontend/src/app/features/notifications/index-notifications/`

**Features Implemented**:
- **Manual Refresh Button**: Allows users to manually trigger system-wide notification generation
  - Located next to the page title
  - Shows loading spinner during processing
  - Displays success/error messages
  - Auto-refreshes notification list after completion
  - Useful for forcing checks without waiting for the 6-hour automatic cycle
- **Two View Modes**: List view and Grid view (user can toggle)
- **Sorting**: By date (newest/oldest) or status (unread/read first)
- **Pagination**: Configurable rows per page (10, 20, 50)
- **Status Tags**: Color-coded tags for type and status
- **Action Buttons**:
  - Mark as read (only for unread notifications)
  - Navigate to person profile
  - Delete (visible only to Admin users)
- **Empty State**: Friendly message when no notifications
- **Loading State**: Spinner during data fetch
- **Responsive Design**: Works on mobile, tablet, and desktop

**Message Display**:
```html
<p class="text-muted small mb-2" style="white-space: pre-line;">
  {{ notification.message }}
</p>
```
- Uses `white-space: pre-line` to preserve line breaks
- Displays bullet points correctly

**Manual Refresh Button**:
```html
<p-button
  icon="pi pi-refresh"
  [text]="true"
  [rounded]="true"
  severity="secondary"
  tooltipPosition="left"
  [style]="{ 'font-size': '1.25rem' }"
  (onClick)="onRefreshNotifications()"
  [loading]="isRefreshing"
></p-button>
```

**Refresh Logic**:
```typescript
onRefreshNotifications(): void {
  this.isRefreshing = true;
  this.notificationService.generateNotifications().pipe(
    takeUntil(this.destroy$)
  ).subscribe({
    next: () => {
      this.isRefreshing = false;
      this.sharedService.showSuccess(
        'Sistema de notificações atualizado com sucesso.'
      );
      this.notificationService.refreshNotifications();
    },
    error: () => {
      this.isRefreshing = false;
      this.sharedService.showError(
        'Ocorreu um erro ao atualizar o sistema de notificações.'
      );
    }
  });
}
```

**Admin-Only Delete Button**:
```typescript
get isAdmin(): boolean {
  return this.authService.isUserAdmin;
}
```
```html
<p-button
  *ngIf="isAdmin"
  icon="pi pi-trash"
  ...
></p-button>
```

#### 5. API Endpoints Configuration

**Location**: `NERBABO.Frontend/src/app/core/objects/apiEndpoints.ts`

```typescript
notifications: '/api/notifications/',
notification_count: '/api/notifications/count',
notification_unread: '/api/notifications/unread',
notification_mark_read: (id: number) => `/api/notifications/${id}/mark-read`,
notification_mark_all_read: '/api/notifications/mark-all-read',
notification_generate: '/api/notifications/generate',
```

---

## How It Works

### Notification Lifecycle

1. **Creation/Update**
   - Background service runs every 6 hours (and immediately on startup) OR
   - Triggered manually via refresh button OR
   - Triggered manually via API endpoint OR
   - Triggered after bulk import OR
   - Triggered after document upload/delete

2. **Generation Process**
   ```
   For each person with Habilitation != WithoutProof:
   ├─ Check IdentificationDocumentPdfId
   ├─ Check HabilitationComprovativePdfId
   ├─ Check IbanComprovativePdfId (if IBAN exists)
   ├─ Build list of missing documents
   └─ Create/Update notification with bullet list
   ```

3. **Display**
   ```
   Frontend polls every 60 seconds
   ├─ Updates badge count in navbar
   ├─ Refreshes notification list if page is open
   └─ Shows real-time updates
   ```

4. **Resolution**
   ```
   User uploads missing document
   ├─ PeopleService triggers notification regeneration
   ├─ Generator updates notification (removes uploaded doc from list)
   └─ If all docs present, notification is deleted
   ```

### Example Scenario

**Initial State**: João Silva imported via bulk import
- Habilitation: "Ensino Secundário"
- IBAN: "PT50..." (filled)
- Missing: All 3 documents

**Result**: One notification created
```
Title: Documentos em Falta

Message:
Documentação em falta para João Silva.
Os seguintes ficheiros não foram submetidos:
- Cópia do Documento de Identificação
- Comprovativo de Habilitações
- Comprovativo de IBAN
```

**User Action**: Uploads Identification Document

**Result**: Notification updated
```
Title: Documentos em Falta

Message:
Documentação em falta para João Silva.
Os seguintes ficheiros não foram submetidos:
- Comprovativo de Habilitações
- Comprovativo de IBAN
```

**User Action**: Uploads remaining documents

**Result**: Notification deleted (all documents present)

---

## Testing

### Backend Tests

1. **Generator Logic**
   - Person with no missing documents → No notification
   - Person with Habilitation = WithoutProof → No notification
   - Person missing 1 document → Notification with 1 bullet point
   - Person missing all 3 → Notification with 3 bullet points
   - IBAN empty + IBAN doc missing → IBAN not in list

2. **Integration Tests**
   - Upload document → Notification updates
   - Delete document → Notification re-appears/updates
   - Bulk import → Notifications created
   - Delete person → Notification cascade deleted

3. **API Tests**
   - Non-admin tries to delete → 403 Forbidden
   - Admin deletes → 200 Success
   - Mark as read → Updates ReadAt and ReadByUserId

### Frontend Tests

1. **Display Tests**
   - Bullet points render correctly
   - Line breaks preserved
   - Badge shows correct count
   - Empty state appears when no notifications

2. **Permission Tests**
   - Admin sees delete button
   - Non-admin does not see delete button
   - Both can mark as read

3. **Responsive Tests**
   - List view works on mobile
   - Grid view works on tablet
   - Pagination works on all devices

---

## Database Migration

```bash
cd NERBABO.Backend/NERBABO.ApiService
dotnet ef migrations add AddNotificationSystem
dotnet ef database update
```

---

## Deployment Checklist

- [ ] Run database migrations
- [ ] Verify NotificationBackgroundService is running (runs on startup and every 6 hours)
- [ ] Test manual refresh button on notifications page
- [ ] Manually trigger `POST /api/notifications/generate` via API to create initial notifications
- [ ] Verify navbar badge displays correctly
- [ ] Test bulk import triggers notifications
- [ ] Test document upload/delete updates notifications
- [ ] Verify Admin-only delete permission
- [ ] Check notification count polling (should update every 60s)
- [ ] Verify refresh button shows loading state and success/error messages

---

## Performance Considerations

### Optimizations Implemented
- **Batch Processing**: Notifications generated in batches during bulk import
- **Efficient Queries**: Uses joins and indexed fields
- **Update vs Create**: Updates existing notifications instead of creating duplicates
- **Pagination**: Frontend paginates results (10/20/50 per page)
- **Polling Interval**: 60 seconds prevents excessive API calls
- **Caching**: People cache cleared after imports

### Scalability
- Generator pattern allows easy addition of new notification types
- Background service prevents blocking user operations
- Indexed database fields ensure fast queries
- Frontend polling can be replaced with SignalR for real-time updates

---

## Future Enhancements

### Planned Features
1. **Additional Document Checks**
   - Expired identification documents (based on IdentificationValidationDate)
   - Missing profile fields (email, phone, address)

2. **Notification Categories**
   - Critical (missing documents)
   - Warning (expiring documents)
   - Info (reminders)

3. **User Preferences**
   - Toggle notification types
   - Email digest option
   - Custom notification frequency

4. **Real-Time Updates**
   - SignalR integration
   - WebSocket support
   - Push notifications

5. **Advanced Features**
   - Notification grouping
   - Bulk actions
   - Archive functionality
   - Search and filter

---

## Monitoring & Maintenance

### Logs to Monitor
- `"Generating missing person document notifications..."` - Generator start
- `"Checking {count} people for missing documents."` - Processing count
- `"Created {count} new notifications."` - Creation count
- `"Found {count} notifications to cleanup."` - Cleanup count
- `"Generating notifications for imported people..."` - Bulk import trigger

### Metrics to Track
- Total notification count
- Unread notification count
- Average time to resolution
- Notification generation frequency
- API endpoint response times

### Alerts to Configure
- Notification count exceeds threshold (possible data issue)
- Background service stops running
- API errors on generation endpoint
- Excessive unread notifications (training needed)

---

## Conclusion

The NERBA Backoffice notification system provides a robust, user-friendly solution for tracking missing documentation. The implementation follows project patterns, uses existing infrastructure, and requires no new dependencies. The modular design allows for easy extension with additional notification types while maintaining performance and scalability.

**Key Achievements**:
✅ Single notification per person with comprehensive document list
✅ Automatic generation via multiple trigger points
✅ Admin-only deletion for data integrity
✅ Proper line break display with bullet points
✅ Responsive UI with list/grid views
✅ Real-time count updates via polling
✅ Seamless integration with bulk imports
✅ Backward-compatible with future enhancements
