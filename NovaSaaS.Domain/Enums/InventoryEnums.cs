namespace NovaSaaS.Domain.Enums
{
    /// <summary>
    /// Trạng thái Stock Transfer.
    /// </summary>
    public enum StockTransferStatus
    {
        Pending = 0,        // Chờ duyệt
        Approved = 1,       // Đã duyệt
        InTransit = 2,      // Đang vận chuyển
        Completed = 3,      // Hoàn thành
        Cancelled = 4       // Hủy
    }

    /// <summary>
    /// Loại Stock Adjustment.
    /// </summary>
    public enum StockAdjustmentType
    {
        Addition = 0,       // Cộng thêm
        Subtraction = 1,    // Trừ bớt
        Damaged = 2,        // Hư hỏng
        Lost = 3,           // Mất mát
        Found = 4,          // Tìm thấy
        Return = 5          // Trả hàng
    }

    /// <summary>
    /// Trạng thái Stock Adjustment.
    /// </summary>
    public enum StockAdjustmentStatus
    {
        Pending = 0,        // Chờ duyệt
        Approved = 1,       // Đã duyệt
        Rejected = 2,       // Từ chối
        Completed = 3       // Hoàn thành
    }

    /// <summary>
    /// Trạng thái Inventory Count (kiểm kê).
    /// </summary>
    public enum InventoryCountStatus
    {
        Draft = 0,          // Nháp
        InProgress = 1,     // Đang kiểm kê
        Completed = 2,      // Hoàn thành
        Cancelled = 3       // Hủy
    }

    /// <summary>
    /// Trạng thái Lot.
    /// </summary>
    public enum LotStatus
    {
        Active = 0,         // Đang sử dụng
        Expired = 1,        // Hết hạn
        Quarantined = 2,    // Cách ly
        Disposed = 3        // Đã hủy
    }

    /// <summary>
    /// Trạng thái Serial Number.
    /// </summary>
    public enum SerialNumberStatus
    {
        Available = 0,      // Có sẵn
        Sold = 1,           // Đã bán
        Reserved = 2,       // Đã đặt
        Defective = 3,      // Lỗi
        Returned = 4        // Trả lại
    }
}
