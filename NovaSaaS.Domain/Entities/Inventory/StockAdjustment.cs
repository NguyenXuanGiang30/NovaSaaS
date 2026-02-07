using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Inventory
{
    /// <summary>
    /// StockAdjustment - Điều chỉnh tồn kho (manual corrections).
    /// </summary>
    public class StockAdjustment : BaseEntity
    {
        /// <summary>
        /// Mã phiếu điều chỉnh.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string AdjustmentNumber { get; set; } = string.Empty;

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
        /// Loại điều chỉnh.
        /// </summary>
        public StockAdjustmentType Type { get; set; }

        /// <summary>
        /// Số lượng điều chỉnh (luôn dương).
        /// </summary>
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        /// <summary>
        /// Số lượng trước điều chỉnh.
        /// </summary>
        public int QuantityBefore { get; set; }

        /// <summary>
        /// Số lượng sau điều chỉnh.
        /// </summary>
        public int QuantityAfter { get; set; }

        /// <summary>
        /// Lý do điều chỉnh.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Trạng thái.
        /// </summary>
        public StockAdjustmentStatus Status { get; set; } = StockAdjustmentStatus.Pending;

        /// <summary>
        /// Người duyệt.
        /// </summary>
        public Guid? ApprovedByUserId { get; set; }

        /// <summary>
        /// Ngày duyệt.
        /// </summary>
        public DateTime? ApprovedAt { get; set; }

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
