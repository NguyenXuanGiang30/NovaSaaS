using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NovaSaaS.Infrastructure.SignalR
{
    /// <summary>
    /// NotificationHub - Hub trung tâm cho real-time notifications.
    /// Quản lý kết nối theo Tenant Groups để đảm bảo không rò rỉ dữ liệu.
    /// Moved to Infrastructure to resolve circular dependency.
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Khi client kết nối, tự động đưa vào group theo TenantId.
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var userName = GetUserName();

            if (!string.IsNullOrEmpty(tenantId))
            {
                // Thêm vào Tenant Group
                await Groups.AddToGroupAsync(Context.ConnectionId, $"tenant_{tenantId}");
                
                // Thêm vào User Group (cho private messages)
                if (!string.IsNullOrEmpty(userId))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                }

                _logger.LogInformation(
                    "🔌 SignalR Connected: User={UserName} ({UserId}), Tenant={TenantId}, ConnectionId={ConnectionId}",
                    userName, userId, tenantId, Context.ConnectionId);

                // Thông báo cho các user khác trong tenant
                await Clients.OthersInGroup($"tenant_{tenantId}")
                    .SendAsync("UserConnected", new
                    {
                        UserId = userId,
                        UserName = userName,
                        Timestamp = DateTime.UtcNow
                    });
            }

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Khi client ngắt kết nối.
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();
            var userName = GetUserName();

            if (!string.IsNullOrEmpty(tenantId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"tenant_{tenantId}");

                if (!string.IsNullOrEmpty(userId))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
                }

                _logger.LogInformation(
                    "🔌 SignalR Disconnected: User={UserName}, Tenant={TenantId}, Reason={Reason}",
                    userName, tenantId, exception?.Message ?? "Normal disconnect");

                await Clients.OthersInGroup($"tenant_{tenantId}")
                    .SendAsync("UserDisconnected", new
                    {
                        UserId = userId,
                        UserName = userName,
                        Timestamp = DateTime.UtcNow
                    });
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Join vào một room cụ thể (ví dụ: chat room, dashboard room).
        /// </summary>
        public async Task JoinRoom(string roomName)
        {
            var tenantId = GetTenantId();
            if (!string.IsNullOrEmpty(tenantId))
            {
                // Room name được prefix bằng tenantId để ensure isolation
                var fullRoomName = $"tenant_{tenantId}_room_{roomName}";
                await Groups.AddToGroupAsync(Context.ConnectionId, fullRoomName);

                _logger.LogDebug("User {UserId} joined room {RoomName}", GetUserId(), fullRoomName);
            }
        }

        /// <summary>
        /// Leave một room cụ thể.
        /// </summary>
        public async Task LeaveRoom(string roomName)
        {
            var tenantId = GetTenantId();
            if (!string.IsNullOrEmpty(tenantId))
            {
                var fullRoomName = $"tenant_{tenantId}_room_{roomName}";
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, fullRoomName);

                _logger.LogDebug("User {UserId} left room {RoomName}", GetUserId(), fullRoomName);
            }
        }

        /// <summary>
        /// Ping để kiểm tra kết nối.
        /// </summary>
        public async Task Ping()
        {
            await Clients.Caller.SendAsync("Pong", new
            {
                Timestamp = DateTime.UtcNow,
                ConnectionId = Context.ConnectionId
            });
        }

        #region Helper Methods

        private string? GetTenantId()
        {
            return Context.User?.FindFirst("TenantId")?.Value
                ?? Context.User?.FindFirst("tenant_id")?.Value;
        }

        private string? GetUserId()
        {
            return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? Context.User?.FindFirst("sub")?.Value;
        }

        private string? GetUserName()
        {
            return Context.User?.FindFirst(ClaimTypes.Name)?.Value
                ?? Context.User?.FindFirst("name")?.Value
                ?? "Unknown";
        }

        #endregion
    }
}
