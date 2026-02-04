using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NovaSaaS.Application.Interfaces;
using NovaSaaS.Infrastructure.SignalR;
using NovaSaaS.Application.Constants;

namespace NovaSaaS.Infrastructure.Services
{
    /// <summary>
    /// SignalRNotificationService - Implementation của INotificationService sử dụng SignalR.
    /// </summary>
    public class SignalRNotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<SignalRNotificationService> _logger;

        public SignalRNotificationService(
            IHubContext<NotificationHub> hubContext,
            ILogger<SignalRNotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        #region Document Processing

        public async Task NotifyDocumentProcessingStartedAsync(
            Guid tenantId, 
            Guid documentId, 
            string fileName)
        {
            var notification = new NotificationPayload
            {
                Type = NotificationTypes.DocumentProcessingStarted,
                Title = "Đang xử lý tài liệu",
                Message = $"Tài liệu '{fileName}' đang được xử lý...",
                Severity = "info",
                Data = new DocumentNotification
                {
                    DocumentId = documentId,
                    FileName = fileName,
                    Status = "Processing",
                    Progress = 0
                }
            };

            await SendToTenantAsync(tenantId, NotificationTypes.DocumentProcessingStarted, notification);
            _logger.LogInformation("📄 Document processing started: {FileName} for Tenant {TenantId}", fileName, tenantId);
        }

        public async Task NotifyDocumentProcessingProgressAsync(
            Guid tenantId, 
            Guid documentId, 
            string fileName,
            int processedChunks,
            int totalChunks)
        {
            var progress = totalChunks > 0 ? (int)((processedChunks * 100.0) / totalChunks) : 0;

            var notification = new NotificationPayload
            {
                Type = NotificationTypes.DocumentProcessingProgress,
                Title = "Đang xử lý tài liệu",
                Message = $"Đã xử lý {processedChunks}/{totalChunks} chunks ({progress}%)",
                Severity = "info",
                Data = new DocumentNotification
                {
                    DocumentId = documentId,
                    FileName = fileName,
                    Status = "Processing",
                    Progress = progress,
                    ProcessedChunks = processedChunks,
                    TotalChunks = totalChunks
                }
            };

            await SendToTenantAsync(tenantId, NotificationTypes.DocumentProcessingProgress, notification);
        }

        public async Task NotifyDocumentProcessingCompletedAsync(
            Guid tenantId, 
            Guid documentId, 
            string fileName,
            int totalChunks)
        {
            var notification = new NotificationPayload
            {
                Type = NotificationTypes.DocumentProcessingCompleted,
                Title = "Tài liệu đã sẵn sàng! 🎉",
                Message = $"Tài liệu '{fileName}' đã được xử lý thành công với {totalChunks} chunks. Bạn có thể bắt đầu chat!",
                Severity = "success",
                Data = new DocumentNotification
                {
                    DocumentId = documentId,
                    FileName = fileName,
                    Status = "Completed",
                    Progress = 100,
                    TotalChunks = totalChunks,
                    ProcessedChunks = totalChunks
                }
            };

            await SendToTenantAsync(tenantId, NotificationTypes.DocumentProcessingCompleted, notification);
            _logger.LogInformation("✅ Document processing completed: {FileName} with {Chunks} chunks", fileName, totalChunks);
        }

        public async Task NotifyDocumentProcessingFailedAsync(
            Guid tenantId, 
            Guid documentId, 
            string fileName,
            string errorMessage)
        {
            var notification = new NotificationPayload
            {
                Type = NotificationTypes.DocumentProcessingFailed,
                Title = "Xử lý tài liệu thất bại ❌",
                Message = $"Không thể xử lý tài liệu '{fileName}': {errorMessage}",
                Severity = "error",
                Data = new DocumentNotification
                {
                    DocumentId = documentId,
                    FileName = fileName,
                    Status = "Failed",
                    ErrorMessage = errorMessage
                }
            };

            await SendToTenantAsync(tenantId, NotificationTypes.DocumentProcessingFailed, notification);
            _logger.LogWarning("❌ Document processing failed: {FileName} - {Error}", fileName, errorMessage);
        }

        #endregion

        #region Order & Sales

        public async Task NotifyNewOrderAsync(
            Guid tenantId,
            Guid orderId,
            string orderNumber,
            string? customerName,
            decimal totalAmount,
            int itemCount)
        {
            var notification = new NotificationPayload
            {
                Type = NotificationTypes.NewOrderCreated,
                Title = "Đơn hàng mới! 🛒",
                Message = $"Đơn hàng #{orderNumber} từ {customerName ?? "Khách lẻ"} - {totalAmount:N0}đ ({itemCount} sản phẩm)",
                Severity = "success",
                ActionUrl = $"/orders/{orderId}",
                Data = new OrderNotification
                {
                    OrderId = orderId,
                    OrderNumber = orderNumber,
                    CustomerName = customerName,
                    TotalAmount = totalAmount,
                    ItemCount = itemCount,
                    Status = "New"
                }
            };

            await SendToTenantAsync(tenantId, NotificationTypes.NewOrderCreated, notification);
            _logger.LogInformation("🛒 New order: #{OrderNumber} - {Amount:C}", orderNumber, totalAmount);
        }

        public async Task NotifyOrderStatusChangedAsync(
            Guid tenantId,
            Guid orderId,
            string orderNumber,
            string oldStatus,
            string newStatus)
        {
            var notification = new NotificationPayload
            {
                Type = NotificationTypes.OrderStatusChanged,
                Title = "Cập nhật đơn hàng",
                Message = $"Đơn hàng #{orderNumber}: {oldStatus} → {newStatus}",
                Severity = "info",
                ActionUrl = $"/orders/{orderId}",
                Data = new OrderNotification
                {
                    OrderId = orderId,
                    OrderNumber = orderNumber,
                    Status = newStatus
                }
            };

            await SendToTenantAsync(tenantId, NotificationTypes.OrderStatusChanged, notification);
        }

        #endregion

        #region Inventory

        public async Task NotifyStockUpdatedAsync(
            Guid tenantId,
            Guid productId,
            string productName,
            string? sku,
            Guid warehouseId,
            string warehouseName,
            decimal oldQuantity,
            decimal newQuantity,
            decimal minimumStock,
            string movementType)
        {
            var change = newQuantity - oldQuantity;
            var changeText = change >= 0 ? $"+{change:N0}" : $"{change:N0}";

            var notification = new NotificationPayload
            {
                Type = NotificationTypes.StockUpdated,
                Title = "Cập nhật tồn kho",
                Message = $"{productName} ({sku}): {changeText} → Còn {newQuantity:N0} tại {warehouseName}",
                Severity = newQuantity <= minimumStock ? "warning" : "info",
                Data = new StockNotification
                {
                    ProductId = productId,
                    ProductName = productName,
                    SKU = sku,
                    WarehouseId = warehouseId,
                    WarehouseName = warehouseName,
                    OldQuantity = oldQuantity,
                    NewQuantity = newQuantity,
                    MinimumStock = minimumStock,
                    MovementType = movementType
                }
            };

            await SendToTenantAsync(tenantId, NotificationTypes.StockUpdated, notification);

            // Gửi cảnh báo nếu sắp hết hàng
            if (newQuantity <= minimumStock && newQuantity > 0)
            {
                await NotifyLowStockAlertAsync(tenantId, productId, productName, sku, newQuantity, minimumStock);
            }
            else if (newQuantity <= 0)
            {
                await SendOutOfStockAlertAsync(tenantId, productId, productName, sku);
            }
        }

        public async Task NotifyLowStockAlertAsync(
            Guid tenantId,
            Guid productId,
            string productName,
            string? sku,
            decimal currentQuantity,
            decimal minimumStock)
        {
            var notification = new NotificationPayload
            {
                Type = NotificationTypes.LowStockAlert,
                Title = "⚠️ Cảnh báo sắp hết hàng",
                Message = $"Sản phẩm '{productName}' ({sku}) chỉ còn {currentQuantity:N0} (tối thiểu: {minimumStock:N0})",
                Severity = "warning",
                ActionUrl = $"/products/{productId}",
                Data = new StockNotification
                {
                    ProductId = productId,
                    ProductName = productName,
                    SKU = sku,
                    NewQuantity = currentQuantity,
                    MinimumStock = minimumStock
                }
            };

            await SendToTenantAsync(tenantId, NotificationTypes.LowStockAlert, notification);
            _logger.LogWarning("⚠️ Low stock alert: {Product} ({SKU}) - {Quantity}/{Minimum}", 
                productName, sku, currentQuantity, minimumStock);
        }

        private async Task SendOutOfStockAlertAsync(
            Guid tenantId,
            Guid productId,
            string productName,
            string? sku)
        {
            var notification = new NotificationPayload
            {
                Type = NotificationTypes.OutOfStockAlert,
                Title = "🚨 Hết hàng!",
                Message = $"Sản phẩm '{productName}' ({sku}) đã HẾT HÀNG!",
                Severity = "error",
                ActionUrl = $"/products/{productId}",
                Data = new StockNotification
                {
                    ProductId = productId,
                    ProductName = productName,
                    SKU = sku,
                    NewQuantity = 0
                }
            };

            await SendToTenantAsync(tenantId, NotificationTypes.OutOfStockAlert, notification);
            _logger.LogError("🚨 Out of stock: {Product} ({SKU})", productName, sku);
        }

        #endregion

        #region Invoice & Payment

        public async Task NotifyPaymentReceivedAsync(
            Guid tenantId,
            Guid invoiceId,
            string invoiceNumber,
            decimal amount,
            string paymentMethod)
        {
            var notification = new NotificationPayload
            {
                Type = NotificationTypes.PaymentReceived,
                Title = "💰 Thanh toán thành công!",
                Message = $"Hóa đơn #{invoiceNumber} đã được thanh toán {amount:N0}đ qua {paymentMethod}",
                Severity = "success",
                ActionUrl = $"/invoices/{invoiceId}"
            };

            await SendToTenantAsync(tenantId, NotificationTypes.PaymentReceived, notification);
            _logger.LogInformation("💰 Payment received: Invoice #{InvoiceNumber} - {Amount:C}", invoiceNumber, amount);
        }

        #endregion

        #region Generic Methods

        public async Task BroadcastToTenantAsync(
            Guid tenantId,
            string type,
            string title,
            string message,
            object? data = null,
            string severity = "info")
        {
            var notification = new NotificationPayload
            {
                Type = type,
                Title = title,
                Message = message,
                Severity = severity,
                Data = data
            };

            await SendToTenantAsync(tenantId, type, notification);
        }

        public async Task NotifyUserAsync(
            Guid userId,
            string type,
            string title,
            string message,
            object? data = null,
            string severity = "info")
        {
            var notification = new NotificationPayload
            {
                Type = type,
                Title = title,
                Message = message,
                Severity = severity,
                Data = data
            };

            try
            {
                await _hubContext.Clients
                    .Group($"user_{userId}")
                    .SendAsync(type, notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to user {UserId}", userId);
            }
        }

        private async Task SendToTenantAsync(Guid tenantId, string methodName, object notification)
        {
            try
            {
                await _hubContext.Clients
                    .Group($"tenant_{tenantId}")
                    .SendAsync(methodName, notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send {Method} notification to tenant {TenantId}", methodName, tenantId);
            }
        }

        #endregion
    }
}
