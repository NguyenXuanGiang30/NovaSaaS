using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace NovaSaaS.Application.Services
{
    /// <summary>
    /// NotificationManagementService - Quản lý thông báo cho người dùng.
    /// Service khác với INotificationService (SignalR) - service này quản lý CRUD notifications.
    /// </summary>
    public class NotificationManagementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Interfaces.INotificationService? _realTimeService;
        private readonly ILogger<NotificationManagementService> _logger;

        public NotificationManagementService(
            IUnitOfWork unitOfWork,
            ILogger<NotificationManagementService> logger,
            Interfaces.INotificationService? realTimeService = null)
        {
            _unitOfWork = unitOfWork;
            _realTimeService = realTimeService;
            _logger = logger;
        }

        /// <summary>
        /// Tạo thông báo mới.
        /// </summary>
        public async Task<Notification> CreateAsync(CreateNotificationRequest request)
        {
            var notification = new Notification
            {
                UserId = request.UserId,
                Type = request.Type,
                Title = request.Title,
                Message = request.Message,
                Category = request.Category ?? "System",
                RelatedEntityId = request.RelatedEntityId,
                RelatedEntityType = request.RelatedEntityType,
                ActionUrl = request.ActionUrl
            };

            _unitOfWork.Notifications.Add(notification);
            await _unitOfWork.CompleteAsync();

            // Send real-time notification via SignalR
            if (_realTimeService != null)
            {
                try
                {
                    await _realTimeService.NotifyUserAsync(
                        request.UserId,
                        request.Type.ToString(),
                        request.Title,
                        request.Message ?? "",
                        new
                        {
                            notification.Id,
                            notification.Category,
                            notification.ActionUrl,
                            notification.CreateAt
                        });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send SignalR notification to user {UserId}", request.UserId);
                }
            }

            _logger.LogInformation("Created notification for user {UserId}: {Title}", 
                request.UserId, request.Title);

            return notification;
        }

        /// <summary>
        /// Tạo thông báo cho nhiều users.
        /// </summary>
        public async Task CreateBulkAsync(List<Guid> userIds, CreateNotificationRequest template)
        {
            var notifications = new List<Notification>();
            foreach (var userId in userIds)
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Type = template.Type,
                    Title = template.Title,
                    Message = template.Message,
                    Category = template.Category ?? "System",
                    RelatedEntityId = template.RelatedEntityId,
                    RelatedEntityType = template.RelatedEntityType,
                    ActionUrl = template.ActionUrl
                };
                notifications.Add(notification);
            }

            _unitOfWork.Notifications.AddRange(notifications);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Created bulk notifications for {Count} users", userIds.Count);
        }

        /// <summary>
        /// Lấy danh sách thông báo của user.
        /// </summary>
        public async Task<List<Notification>> GetUserNotificationsAsync(
            Guid userId, 
            bool unreadOnly = false,
            int page = 1,
            int pageSize = 20)
        {
            var notifications = await _unitOfWork.Notifications.FindAsync(n =>
                n.UserId == userId && (!unreadOnly || !n.IsRead));

            return notifications
                .OrderByDescending(n => n.CreateAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        /// <summary>
        /// Đếm số thông báo chưa đọc.
        /// </summary>
        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _unitOfWork.Notifications.CountAsync(n => 
                n.UserId == userId && !n.IsRead);
        }

        /// <summary>
        /// Đánh dấu thông báo đã đọc.
        /// </summary>
        public async Task<bool> MarkAsReadAsync(Guid notificationId)
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
            if (notification == null) return false;

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            _unitOfWork.Notifications.Update(notification);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        /// <summary>
        /// Đánh dấu tất cả thông báo của user đã đọc.
        /// </summary>
        public async Task<int> MarkAllAsReadAsync(Guid userId)
        {
            var notifications = await _unitOfWork.Notifications.FindAsync(n =>
                n.UserId == userId && !n.IsRead);

            var notificationList = notifications.ToList();
            foreach (var notification in notificationList)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                _unitOfWork.Notifications.Update(notification);
            }

            await _unitOfWork.CompleteAsync();
            return notificationList.Count;
        }

        /// <summary>
        /// Xóa thông báo.
        /// </summary>
        public async Task<bool> DeleteAsync(Guid notificationId)
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
            if (notification == null) return false;

            _unitOfWork.Notifications.Remove(notification);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        /// <summary>
        /// Xóa tất cả thông báo đã đọc của user.
        /// </summary>
        public async Task<int> DeleteReadNotificationsAsync(Guid userId)
        {
            var notifications = await _unitOfWork.Notifications.FindAsync(n =>
                n.UserId == userId && n.IsRead);

            var notificationList = notifications.ToList();
            _unitOfWork.Notifications.RemoveRange(notificationList);
            await _unitOfWork.CompleteAsync();
            
            return notificationList.Count;
        }
    }

    #region DTOs

    public class CreateNotificationRequest
    {
        public Guid UserId { get; set; }
        public NotificationType Type { get; set; } = NotificationType.Info;
        public string Title { get; set; } = string.Empty;
        public string? Message { get; set; }
        public string? Category { get; set; }
        public Guid? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
        public string? ActionUrl { get; set; }
    }

    #endregion
}
