using NovaSaaS.Domain.Entities.Common;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Inventory
{
    /// <summary>
    /// Loại biến động kho.
    /// </summary>
    public enum StockMovementType
    {
        /// <summary>
        /// Nhập kho.
        /// </summary>
        In,

        /// <summary>
        /// Xuất kho.
        /// </summary>
        Out,

        /// <summary>
        /// Chuyển kho (xuất).
        /// </summary>
        TransferOut,

        /// <summary>
        /// Chuyển kho (nhập).
        /// </summary>
        TransferIn,

        /// <summary>
        /// Điều chỉnh (cộng).
        /// </summary>
        AdjustmentAdd,

        /// <summary>
        /// Điều chỉnh (trừ).
        /// </summary>
        AdjustmentSubtract,

        /// <summary>
        /// Bán hàng.
        /// </summary>
        Sale,

        /// <summary>
        /// Trả hàng.
        /// </summary>
        Return
    }

    /// <summary>
    /// StockMovement - Lưu vết mọi biến động kho hàng.
    /// </summary>
    public class StockMovement : BaseEntity
    {
        /// <summary>
        /// ID sản phẩm.
        /// </summary>
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        /// <summary>
        /// ID kho hàng.
        /// </summary>
        public Guid WarehouseId { get; set; }
        public virtual Warehouse Warehouse { get; set; } = null!;

        /// <summary>
        /// Số lượng biến động (có thể âm).
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Loại biến động.
        /// </summary>
        public StockMovementType Type { get; set; }

        /// <summary>
        /// Số lượng tồn trước khi thay đổi.
        /// </summary>
        public int QuantityBefore { get; set; }

        /// <summary>
        /// Số lượng tồn sau khi thay đổi.
        /// </summary>
        public int QuantityAfter { get; set; }

        /// <summary>
        /// Mã tham chiếu (Số PO, Số đơn hàng...).
        /// </summary>
        [MaxLength(100)]
        public string? ReferenceCode { get; set; }

        /// <summary>
        /// ID tham chiếu (OrderId, PurchaseOrderId...).
        /// </summary>
        public Guid? ReferenceId { get; set; }

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// Kho đích (cho Transfer).
        /// </summary>
        public Guid? DestinationWarehouseId { get; set; }
    }
}
