using NovaSaaS.Application.DTOs.Inventory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NovaSaaS.Application.Interfaces.Inventory
{
    public interface IStockService
    {
        Task<List<StockByWarehouseDto>> GetStockByProductAsync(Guid productId);
        Task<int> GetStockQuantityAsync(Guid productId, Guid warehouseId);
        Task<List<StockMovementDto>> GetMovementHistoryAsync(
            Guid? productId = null,
            Guid? warehouseId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int take = 50);
        Task<StockMovementDto> AdjustStockAsync(StockAdjustmentDto dto);
        Task<(StockMovementDto outMovement, StockMovementDto inMovement)> TransferStockAsync(StockTransferDto dto);
        Task ReduceStockForOrderAsync(Guid orderId, string orderNumber, List<NovaSaaS.Application.DTOs.Business.CreateOrderItemDto> items);
    }
}
