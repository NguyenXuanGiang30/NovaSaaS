using System;

namespace NovaSaaS.Application.Constants
{
    /// <summary>
    /// Notification Types - Các loại thông báo real-time.
    /// Moved to Application layer for shared access.
    /// </summary>
    public static class NotificationTypes
    {
        // ========================================
        // AI Document Processing
        // ========================================
        public const string DocumentProcessingStarted = "DocumentProcessingStarted";
        public const string DocumentProcessingProgress = "DocumentProcessingProgress";
        public const string DocumentProcessingCompleted = "DocumentProcessingCompleted";
        public const string DocumentProcessingFailed = "DocumentProcessingFailed";

        // ========================================
        // Order & Sales
        // ========================================
        public const string NewOrderCreated = "NewOrderCreated";
        public const string OrderStatusChanged = "OrderStatusChanged";
        public const string OrderCancelled = "OrderCancelled";

        // ========================================
        // Inventory
        // ========================================
        public const string StockUpdated = "StockUpdated";
        public const string LowStockAlert = "LowStockAlert";
        public const string OutOfStockAlert = "OutOfStockAlert";
        public const string StockMovementCreated = "StockMovementCreated";

        // ========================================
        // Invoice & Payment
        // ========================================
        public const string InvoiceCreated = "InvoiceCreated";
        public const string InvoicePaid = "InvoicePaid";
        public const string PaymentReceived = "PaymentReceived";
        public const string PaymentFailed = "PaymentFailed";

        // ========================================
        // System
        // ========================================
        public const string SystemAnnouncement = "SystemAnnouncement";
        public const string MaintenanceScheduled = "MaintenanceScheduled";

        // ========================================
        // User Activity
        // ========================================
        public const string UserConnected = "UserConnected";
        public const string UserDisconnected = "UserDisconnected";
    }

    /// <summary>
    /// Notification Payload - Cấu trúc dữ liệu thông báo.
    /// </summary>
    public class NotificationPayload
    {
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Severity { get; set; } // info, warning, error, success
        public string? ActionUrl { get; set; }
    }

    /// <summary>
    /// Document Processing Notification
    /// </summary>
    public class DocumentNotification
    {
        public Guid DocumentId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? Progress { get; set; } // 0-100
        public int? TotalChunks { get; set; }
        public int? ProcessedChunks { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Order Notification
    /// </summary>
    public class OrderNotification
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
        public int ItemCount { get; set; }
    }

    /// <summary>
    /// Stock Notification
    /// </summary>
    public class StockNotification
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? SKU { get; set; }
        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public decimal OldQuantity { get; set; }
        public decimal NewQuantity { get; set; }
        public decimal MinimumStock { get; set; }
        public string MovementType { get; set; } = string.Empty; // Import, Export, Adjustment
    }
}
