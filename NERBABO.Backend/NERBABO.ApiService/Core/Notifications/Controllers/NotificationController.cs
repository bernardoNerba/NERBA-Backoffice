using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Notifications.Dtos;
using NERBABO.ApiService.Core.Notifications.Services;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Notifications.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IResponseHandler _responseHandler;
        private readonly UserManager<User> _userManager;

        public NotificationController(
            INotificationService notificationService,
            IResponseHandler responseHandler,
            UserManager<User> userManager)
        {
            _notificationService = notificationService;
            _responseHandler = responseHandler;
            _userManager = userManager;
        }

        /// <summary>
        /// Gets all notifications.
        /// </summary>
        /// <response code="200">Notifications found. Returns a list of RetrieveNotificationDto.</response>
        /// <response code="404">No notifications found.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetAllNotificationsAsync()
        {
            Result<IEnumerable<RetrieveNotificationDto>> result = await _notificationService.GetAllAsync();
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Gets a notification by ID.
        /// </summary>
        /// <param name="id">The ID of the notification to retrieve.</param>
        /// <response code="200">Notification found. Returns a RetrieveNotificationDto.</response>
        /// <response code="404">Notification not found.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet("{id:long}")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetNotificationAsync(long id)
        {
            Result<RetrieveNotificationDto> result = await _notificationService.GetByIdAsync(id);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Gets all unread notifications.
        /// </summary>
        /// <response code="200">Unread notifications found. Returns a list of RetrieveNotificationDto.</response>
        /// <response code="404">No unread notifications found.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet("unread")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetUnreadNotificationsAsync()
        {
            Result<IEnumerable<RetrieveNotificationDto>> result = await _notificationService.GetUnreadNotificationsAsync();
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Gets the count of notifications (total and unread).
        /// </summary>
        /// <response code="200">Count retrieved successfully. Returns a NotificationCountDto.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet("count")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetNotificationCountAsync()
        {
            Result<NotificationCountDto> result = await _notificationService.GetNotificationCountAsync();
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Creates a new notification manually.
        /// </summary>
        /// <param name="notification">The CreateNotificationDto object containing the notification details.</param>
        /// <response code="201">Notification created successfully. Returns the created notification.</response>
        /// <response code="404">Related person not found.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpPost("create")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> CreateNotificationAsync([FromBody] CreateNotificationDto notification)
        {
            Result<RetrieveNotificationDto> result = await _notificationService.CreateAsync(notification);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Updates a notification.
        /// </summary>
        /// <param name="id">The ID of the notification to update.</param>
        /// <param name="notification">The UpdateNotificationDto object containing the updated notification details.</param>
        /// <response code="200">Notification updated successfully. Returns the updated notification.</response>
        /// <response code="404">Notification not found.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpPut("update/{id:long}")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> UpdateNotificationAsync(long id, [FromBody] UpdateNotificationDto notification)
        {
            if (id != notification.Id)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            Result<RetrieveNotificationDto> result = await _notificationService.UpdateAsync(notification);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Marks a notification as read.
        /// </summary>
        /// <param name="id">The ID of the notification to mark as read.</param>
        /// <response code="200">Notification marked as read successfully. Returns the updated notification.</response>
        /// <response code="404">Notification not found.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpPut("{id:long}/mark-read")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> MarkAsReadAsync(long id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                var userResult = Result<RetrieveNotificationDto>
                    .Fail("Utilizador inválido.", "Utilizador não autenticado.", StatusCodes.Status401Unauthorized);
                return _responseHandler.HandleResult(userResult);
            }

            Result<RetrieveNotificationDto> result = await _notificationService.MarkAsReadAsync(id, user.Id);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Marks all notifications as read.
        /// </summary>
        /// <response code="200">All notifications marked as read successfully.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpPut("mark-all-read")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> MarkAllAsReadAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                var userResult = Result
                    .Fail("Utilizador inválido.", "Utilizador não autenticado.", StatusCodes.Status401Unauthorized);
                return _responseHandler.HandleResult(userResult);
            }

            Result result = await _notificationService.MarkAllAsReadAsync(user.Id);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Deletes a notification by its ID. Only Admin users can delete notifications.
        /// </summary>
        /// <param name="id">The ID of the notification to delete.</param>
        /// <response code="200">Notification deleted successfully.</response>
        /// <response code="404">Notification not found.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="403">Forbidden. Only Admin users can delete notifications.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpDelete("delete/{id:long}")]
        [Authorize(Policy = "ActiveUser", Roles = "Admin")]
        public async Task<IActionResult> DeleteNotificationAsync(long id)
        {
            Result result = await _notificationService.DeleteAsync(id);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Generates notifications using all registered notification generators.
        /// This will create new notifications and cleanup resolved ones.
        /// </summary>
        /// <response code="200">Notifications generated successfully.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpPost("generate")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GenerateNotificationsAsync()
        {
            Result result = await _notificationService.GenerateNotificationsAsync();
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Deletes all notifications related to a specific person.
        /// </summary>
        /// <param name="personId">The ID of the person whose notifications should be deleted.</param>
        /// <response code="200">Notifications deleted successfully.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpDelete("person/{personId:long}")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> DeleteNotificationsByPersonIdAsync(long personId)
        {
            Result result = await _notificationService.DeleteNotificationsByPersonIdAsync(personId);
            return _responseHandler.HandleResult(result);
        }
    }
}
