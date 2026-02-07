using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Inventory
{
    /// <summary>
    /// SerialNumber - Quản lý số serial cho sản phẩm.
    /// </summary>
    public class SerialNumber : BaseEntity
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
        /// Mã serial.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Serial { get; set; } = string.Empty;

        /// <summary>
        /// Trạng thái.
        /// </summary>
        public SerialNumberStatus Status { get; set; } = SerialNumberStatus.Available;

        /// <summary>
        /// ID Lô hàng (tùy chọn).
        /// </summary>
        public Guid? LotNumberId { get; set; }
        public virtual LotNumber? LotNumber { get; set; }

        /// <summary>
        /// ID đơn hàng khi bán.
        /// </summary>
        public Guid? SoldOrderId { get; set; }

        /// <summary>
        /// Ngày bán.
        /// </summary>
        public DateTime? SoldDate { get; set; }

        /// <summary>
        /// Ngày bảo hành hết hạn.
        /// </summary>
        public DateTime? WarrantyExpiry { get; set; }

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
