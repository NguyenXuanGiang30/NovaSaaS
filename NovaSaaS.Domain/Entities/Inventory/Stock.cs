using NovaSaaS.Domain.Entities.Common;
using System;

namespace NovaSaaS.Domain.Entities.Inventory
{
    /// <summary>
    /// Stock - Quản lý số lượng tồn kho thực tế tại từng kho.
    /// Mỗi bản ghi = 1 sản phẩm tại 1 kho.
    /// </summary>
    public class Stock : BaseEntity
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
        /// Số lượng tồn kho hiện tại.
        /// </summary>
        public int Quantity { get; set; } = 0;

        /// <summary>
        /// Số lượng đặt trước (đang xử lý đơn hàng).
        /// </summary>
        public int ReservedQuantity { get; set; } = 0;

        /// <summary>
        /// Số lượng khả dụng = Quantity - ReservedQuantity.
        /// </summary>
        public int AvailableQuantity => Quantity - ReservedQuantity;

        /// <summary>
        /// Vị trí trong kho (ví dụ: A-01-02).
        /// </summary>
        public string? Location { get; set; }
    }
}
