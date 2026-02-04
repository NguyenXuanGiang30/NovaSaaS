namespace NovaSaaS.Application.Interfaces
{
    /// <summary>
    /// INotificationService - Interface cho real-time notifications.
    /// Cho phép các services gửi thông báo mà không cần biết SignalR.
    /// </summary>
    public interface INotificationService
    {
        // ========================================
        // Document Processing
        // ========================================
        
        /// <summary>
        /// Thông báo bắt đầu xử lý document.
        /// </summary>
        Task NotifyDocumentProcessingStartedAsync(
            Guid tenantId, 
            Guid documentId, 
            string fileName);

        /// <summary>
        /// Thông báo tiến độ xử lý document.
        /// </summary>
        Task NotifyDocumentProcessingProgressAsync(
            Guid tenantId, 
            Guid documentId, 
            string fileName,
            int processedChunks,
            int totalChunks);

        /// <summary>
        /// Thông báo xử lý document hoàn thành.
        /// </summary>
        Task NotifyDocumentProcessingCompletedAsync(
            Guid tenantId, 
            Guid documentId, 
            string fileName,
            int totalChunks);

        /// <summary>
        /// Thông báo xử lý document thất bại.
        /// </summary>
        Task NotifyDocumentProcessingFailedAsync(
            Guid tenantId, 
            Guid documentId, 
            string fileName,
            string errorMessage);

        // ========================================
        // Order & Sales
        // ========================================

        /// <summary>
        /// Thông báo có đơn hàng mới.
        /// </summary>
        Task NotifyNewOrderAsync(
            Guid tenantId,
            Guid orderId,
            string orderNumber,
            string? customerName,
            decimal totalAmount,
            int itemCount);

        /// <summary>
        /// Thông báo thay đổi trạng thái đơn hàng.
        /// </summary>
        Task NotifyOrderStatusChangedAsync(
            Guid tenantId,
            Guid orderId,
            string orderNumber,
            string oldStatus,
            string newStatus);

        // ========================================
        // Inventory
        // ========================================

        /// <summary>
        /// Thông báo cập nhật tồn kho.
        /// </summary>
        Task NotifyStockUpdatedAsync(
            Guid tenantId,
            Guid productId,
            string productName,
            string? sku,
            Guid warehouseId,
            string warehouseName,
            decimal oldQuantity,
            decimal newQuantity,
            decimal minimumStock,
            string movementType);

        /// <summary>
        /// Cảnh báo sản phẩm sắp hết hàng.
        /// </summary>
        Task NotifyLowStockAlertAsync(
            Guid tenantId,
            Guid productId,
            string productName,
            string? sku,
            decimal currentQuantity,
            decimal minimumStock);

        // ========================================
        // Invoice & Payment
        // ========================================

        /// <summary>
        /// Thông báo thanh toán thành công.
        /// </summary>
        Task NotifyPaymentReceivedAsync(
            Guid tenantId,
            Guid invoiceId,
            string invoiceNumber,
            decimal amount,
            string paymentMethod);

        // ========================================
        // System
        // ========================================

        /// <summary>
        /// Gửi thông báo tới toàn bộ tenant.
        /// </summary>
        Task BroadcastToTenantAsync(
            Guid tenantId,
            string type,
            string title,
            string message,
            object? data = null,
            string severity = "info");

        /// <summary>
        /// Gửi thông báo tới một user cụ thể.
        /// </summary>
        Task NotifyUserAsync(
            Guid userId,
            string type,
            string title,
            string message,
            object? data = null,
            string severity = "info");
    }
}
