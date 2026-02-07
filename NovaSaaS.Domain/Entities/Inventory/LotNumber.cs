using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Inventory
{
    /// <summary>
    /// LotNumber - Quản lý lô hàng (batch tracking).
    /// </summary>
    public class LotNumber : BaseEntity
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
        /// Mã lô hàng.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string LotCode { get; set; } = string.Empty;

        /// <summary>
        /// Ngày sản xuất.
        /// </summary>
        public DateTime? ManufactureDate { get; set; }

        /// <summary>
        /// Ngày hết hạn.
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Số lượng trong lô.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        /// <summary>
        /// Số lượng đã bán/xuất.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int SoldQuantity { get; set; } = 0;

        /// <summary>
        /// Số lượng còn lại.
        /// </summary>
        public int RemainingQuantity => Quantity - SoldQuantity;

        /// <summary>
        /// Trạng thái.
        /// </summary>
        public LotStatus Status { get; set; } = LotStatus.Active;

        /// <summary>
        /// Nhà cung cấp.
        /// </summary>
        [MaxLength(200)]
        public string? SupplierName { get; set; }

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
