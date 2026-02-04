using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaSaaS.Application.Interfaces;
using NovaSaaS.Application.Services;
using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Interfaces;
using NovaSaaS.WebApi.Models;

namespace NovaSaaS.WebApi.Controllers
{
    /// <summary>
    /// Notifications Controller - Quản lý thông báo người dùng.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Tags("Notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationManagementService _notificationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            NotificationManagementService notificationService,
            ICurrentUserService currentUserService,
            ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        private Guid GetCurrentUserId()
        {
            if (!string.IsNullOrEmpty(_currentUserService.UserId) &&
                Guid.TryParse(_currentUserService.UserId, out var userId))
            {
                return userId;
            }
            return Guid.Empty;
        }

        /// <summary>
        /// Lấy danh sách thông báo của user hiện tại.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<NotificationDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetNotifications([FromQuery] bool? unreadOnly = null)
        {
            var userId = GetCurrentUserId();
            var notifications = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly ?? false);
            
            var result = notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type.ToString(),
                Title = n.Title,
                Message = n.Message ?? "",
                IsRead = n.IsRead,
                CreatedAt = n.CreateAt
            }).ToList();
            
            return Ok(ApiResponse<List<NotificationDto>>.SuccessResult(result));
        }

        /// <summary>
        /// Lấy số lượng thông báo chưa đọc.
        /// </summary>
        [HttpGet("unread-count")]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetCurrentUserId();
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(ApiResponse<int>.SuccessResult(count));
        }

        /// <summary>
        /// Đánh dấu thông báo đã đọc.
        /// </summary>
        [HttpPost("{id}/mark-read")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var result = await _notificationService.MarkAsReadAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse<object>.FailResult("Không tìm thấy thông báo."));
            }
            return Ok(ApiResponse<bool>.SuccessResult(true, "Đã đánh dấu đã đọc."));
        }

        /// <summary>
        /// Đánh dấu tất cả thông báo đã đọc.
        /// </summary>
        [HttpPost("mark-all-read")]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            var count = await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(ApiResponse<int>.SuccessResult(count, $"Đã đánh dấu {count} thông báo đã đọc."));
        }

        /// <summary>
        /// Xóa thông báo.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteNotification(Guid id)
        {
            var result = await _notificationService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse<object>.FailResult("Không tìm thấy thông báo."));
            }
            return Ok(ApiResponse<bool>.SuccessResult(true, "Đã xóa thông báo."));
        }
    }

    #region DTOs

    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    #endregion
}
