using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Inventory
{
    /// <summary>
    /// InventoryCount - Phiếu kiểm kê kho hàng.
    /// </summary>
    public class InventoryCount : BaseEntity
    {
        /// <summary>
        /// Mã phiếu kiểm kê.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string CountNumber { get; set; } = string.Empty;

        /// <summary>
        /// ID kho hàng kiểm kê.
        /// </summary>
        public Guid WarehouseId { get; set; }
        public virtual Warehouse Warehouse { get; set; } = null!;

        /// <summary>
        /// Trạng thái.
        /// </summary>
        public InventoryCountStatus Status { get; set; } = InventoryCountStatus.Draft;

        /// <summary>
        /// Ngày kiểm kê.
        /// </summary>
        public DateTime CountDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }

        /// <summary>
        /// Người duyệt.
        /// </summary>
        public Guid? ApprovedByUserId { get; set; }

        /// <summary>
        /// Ngày hoàn thành.
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Tổng số chênh lệch.
        /// </summary>
        public int TotalDiscrepancy { get; set; } = 0;

        // Navigation
        public virtual ICollection<InventoryCountItem> Items { get; set; } = new List<InventoryCountItem>();
    }

    /// <summary>
    /// InventoryCountItem - Chi tiết sản phẩm trong phiếu kiểm kê.
    /// </summary>
    public class InventoryCountItem : BaseEntity
    {
        /// <summary>
        /// ID phiếu kiểm kê.
        /// </summary>
        public Guid InventoryCountId { get; set; }
        public virtual InventoryCount InventoryCount { get; set; } = null!;

        /// <summary>
        /// ID sản phẩm.
        /// </summary>
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        /// <summary>
        /// Tồn kho hệ thống (system count).
        /// </summary>
        public int SystemQuantity { get; set; }

        /// <summary>
        /// Tồn kho thực tế (physical count).
        /// </summary>
        public int ActualQuantity { get; set; }

        /// <summary>
        /// Chênh lệch = Actual - System.
        /// </summary>
        public int Discrepancy => ActualQuantity - SystemQuantity;

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
