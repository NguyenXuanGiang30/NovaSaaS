using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Inventory
{
    /// <summary>
    /// StockTransfer - Phiếu chuyển kho giữa các kho hàng.
    /// </summary>
    public class StockTransfer : BaseEntity
    {
        /// <summary>
        /// Mã phiếu chuyển kho.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string TransferNumber { get; set; } = string.Empty;

        /// <summary>
        /// Kho nguồn.
        /// </summary>
        public Guid FromWarehouseId { get; set; }
        public virtual Warehouse FromWarehouse { get; set; } = null!;

        /// <summary>
        /// Kho đích.
        /// </summary>
        public Guid ToWarehouseId { get; set; }
        public virtual Warehouse ToWarehouse { get; set; } = null!;

        /// <summary>
        /// Trạng thái.
        /// </summary>
        public StockTransferStatus Status { get; set; } = StockTransferStatus.Pending;

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
        /// Ngày duyệt.
        /// </summary>
        public DateTime? ApprovedAt { get; set; }

        /// <summary>
        /// Ngày hoàn thành.
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        // Navigation
        public virtual ICollection<StockTransferItem> Items { get; set; } = new List<StockTransferItem>();
    }

    /// <summary>
    /// StockTransferItem - Chi tiết sản phẩm trong phiếu chuyển kho.
    /// </summary>
    public class StockTransferItem : BaseEntity
    {
        /// <summary>
        /// ID phiếu chuyển kho.
        /// </summary>
        public Guid StockTransferId { get; set; }
        public virtual StockTransfer StockTransfer { get; set; } = null!;

        /// <summary>
        /// ID sản phẩm.
        /// </summary>
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        /// <summary>
        /// Số lượng chuyển.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        /// <summary>
        /// Số lượng đã nhận (khi hoàn thành).
        /// </summary>
        public int ReceivedQuantity { get; set; } = 0;

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
