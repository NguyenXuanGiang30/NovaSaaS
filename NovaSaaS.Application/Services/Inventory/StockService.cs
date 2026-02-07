using NovaSaaS.Application.DTOs.Business;
using NovaSaaS.Application.DTOs.Inventory;
using NovaSaaS.Domain.Entities.Inventory;
using NovaSaaS.Domain.Interfaces;
using NovaSaaS.Application.Interfaces; // Added for INotificationService
using NovaSaaS.Application.Interfaces.Inventory; // Added IStockService usage
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NovaSaaS.Application.Services.Inventory
{
    /// <summary>
    /// StockService - Quản lý tồn kho và biến động kho.
    /// 
    /// Xử lý logic:
    /// - Nhập/Xuất kho
    /// - Chuyển kho (Transfer) với Transaction
    /// - Lịch sử biến động
    /// </summary>
    public class StockService : IStockService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly INotificationService _notificationService;

        public StockService(
            IUnitOfWork unitOfWork, 
            ICurrentUserService currentUser,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _notificationService = notificationService;
        }

        #region Query Operations

        /// <summary>
        /// Lấy tồn kho của một sản phẩm tại tất cả các kho.
        /// </summary>
        public async Task<List<StockByWarehouseDto>> GetStockByProductAsync(Guid productId)
        {
            var stocks = await _unitOfWork.Repository<Stock>()
                .FindAsync(s => s.ProductId == productId, s => s.Warehouse);

            return stocks.Select(s => new StockByWarehouseDto
            {
                WarehouseId = s.WarehouseId,
                WarehouseName = s.Warehouse.Name,
                Quantity = s.Quantity,
                ReservedQuantity = s.ReservedQuantity,
                AvailableQuantity = s.AvailableQuantity,
                Location = s.Location
            }).ToList();
        }

        /// <summary>
        /// Lấy tồn kho tại một kho cụ thể.
        /// </summary>
        public async Task<int> GetStockQuantityAsync(Guid productId, Guid warehouseId)
        {
            var stock = await _unitOfWork.Repository<Stock>()
                .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseId == warehouseId);
            return stock?.Quantity ?? 0;
        }

        /// <summary>
        /// Lấy lịch sử biến động kho.
        /// </summary>
        public async Task<List<StockMovementDto>> GetMovementHistoryAsync(
            Guid? productId = null,
            Guid? warehouseId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int take = 50)
        {
            var movements = await _unitOfWork.StockMovements
                .FindAsync(_ => true, m => m.Product, m => m.Warehouse);

            var query = movements.AsQueryable();

            if (productId.HasValue)
                query = query.Where(m => m.ProductId == productId);

            if (warehouseId.HasValue)
                query = query.Where(m => m.WarehouseId == warehouseId);

            if (fromDate.HasValue)
                query = query.Where(m => m.CreateAt >= fromDate);

            if (toDate.HasValue)
                query = query.Where(m => m.CreateAt <= toDate);

            return query
                .OrderByDescending(m => m.CreateAt)
                .Take(take)
                .Select(m => new StockMovementDto
                {
                    Id = m.Id,
                    ProductId = m.ProductId,
                    ProductName = m.Product.Name,
                    ProductSKU = m.Product.SKU,
                    WarehouseName = m.Warehouse.Name,
                    Quantity = m.Quantity,
                    Type = m.Type.ToString(),
                    QuantityBefore = m.QuantityBefore,
                    QuantityAfter = m.QuantityAfter,
                    ReferenceCode = m.ReferenceCode,
                    Notes = m.Notes,
                    CreatedBy = m.CreatedBy,
                    CreateAt = m.CreateAt
                })
                .ToList();
        }

        #endregion

        #region Stock Adjustment (Nhập/Xuất)

        /// <summary>
        /// Nhập hoặc Xuất kho.
        /// </summary>
        public async Task<StockMovementDto> AdjustStockAsync(QuickStockAdjustmentDto dto)
        {
            // Validate product
            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
            if (product == null)
                throw new ArgumentException("Sản phẩm không tồn tại");

            // Validate warehouse
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId);
            if (warehouse == null)
                throw new ArgumentException("Kho hàng không tồn tại");

            // Get or create stock record
            var stock = await _unitOfWork.Repository<Stock>()
                .FirstOrDefaultAsync(s => s.ProductId == dto.ProductId && s.WarehouseId == dto.WarehouseId);

            var quantityBefore = stock?.Quantity ?? 0;
            var adjustmentQty = dto.IsInbound ? dto.Quantity : -dto.Quantity;

            if (stock == null)
            {
                if (!dto.IsInbound)
                    throw new InvalidOperationException("Không thể xuất kho khi chưa có tồn");

                stock = new Stock
                {
                    ProductId = dto.ProductId,
                    WarehouseId = dto.WarehouseId,
                    Quantity = dto.Quantity
                };
                stock.CreatedBy = _currentUser.UserId;
                _unitOfWork.Repository<Stock>().Add(stock);
            }
            else
            {
                if (stock.Quantity + adjustmentQty < 0)
                    throw new InvalidOperationException($"Tồn kho không đủ. Tồn hiện tại: {stock.Quantity}");

                stock.Quantity += adjustmentQty;
                stock.UpdatedBy = _currentUser.UserId;
                stock.UpdateAt = DateTime.UtcNow;
                _unitOfWork.Repository<Stock>().Update(stock);
            }

            // Update product total stock
            product.StockQuantity += adjustmentQty;
            product.UpdateAt = DateTime.UtcNow;
            _unitOfWork.Products.Update(product);

            // Create movement log
            var movement = new StockMovement
            {
                ProductId = dto.ProductId,
                WarehouseId = dto.WarehouseId,
                Quantity = adjustmentQty,
                Type = dto.IsInbound ? StockMovementType.In : StockMovementType.Out,
                QuantityBefore = quantityBefore,
                QuantityAfter = stock.Quantity,
                ReferenceCode = dto.ReferenceCode,
                Notes = dto.Notes,
                CreatedBy = _currentUser.UserId
            };
            _unitOfWork.StockMovements.Add(movement);

            await _unitOfWork.CompleteAsync();

            // Notify Real-time
            var tenantId = _currentUser.TenantId ?? Guid.Empty;
            _ = _notificationService.NotifyStockUpdatedAsync(
                tenantId,
                movement.ProductId,
                product.Name,
                product.SKU,
                movement.WarehouseId,
                warehouse.Name,
                movement.QuantityBefore,
                stock.Quantity,
                0, // TODO: Pass MinimumStock from Product
                movement.Type.ToString()
            );

            return new StockMovementDto
            {
                Id = movement.Id,
                ProductId = movement.ProductId,
                ProductName = product.Name,
                ProductSKU = product.SKU,
                WarehouseName = warehouse.Name,
                Quantity = movement.Quantity,
                Type = movement.Type.ToString(),
                QuantityBefore = movement.QuantityBefore,
                QuantityAfter = movement.QuantityAfter,
                ReferenceCode = movement.ReferenceCode,
                Notes = movement.Notes,
                CreatedBy = movement.CreatedBy,
                CreateAt = movement.CreateAt
            };
        }

        #endregion

        #region Stock Transfer (Chuyển kho)

        /// <summary>
        /// Chuyển kho - Sử dụng Transaction đảm bảo atomic.
        /// </summary>
        public async Task<(StockMovementDto outMovement, StockMovementDto inMovement)> TransferStockAsync(
            QuickStockTransferDto dto)
        {
            if (dto.FromWarehouseId == dto.ToWarehouseId)
                throw new ArgumentException("Kho nguồn và kho đích không được trùng nhau");

            // Validate
            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
            if (product == null)
                throw new ArgumentException("Sản phẩm không tồn tại");

            var fromWarehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.FromWarehouseId);
            if (fromWarehouse == null)
                throw new ArgumentException("Kho nguồn không tồn tại");

            var toWarehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.ToWarehouseId);
            if (toWarehouse == null)
                throw new ArgumentException("Kho đích không tồn tại");

            // Begin transaction
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // === 1. Xuất kho nguồn ===
                var fromStock = await _unitOfWork.Repository<Stock>()
                    .FirstOrDefaultAsync(s => s.ProductId == dto.ProductId && s.WarehouseId == dto.FromWarehouseId);

                if (fromStock == null || fromStock.Quantity < dto.Quantity)
                    throw new InvalidOperationException(
                        $"Tồn kho không đủ. Tồn hiện tại: {fromStock?.Quantity ?? 0}");

                var fromQtyBefore = fromStock.Quantity;
                fromStock.Quantity -= dto.Quantity;
                fromStock.UpdateAt = DateTime.UtcNow;
                _unitOfWork.Repository<Stock>().Update(fromStock);

                var outMovement = new StockMovement
                {
                    ProductId = dto.ProductId,
                    WarehouseId = dto.FromWarehouseId,
                    Quantity = -dto.Quantity,
                    Type = StockMovementType.TransferOut,
                    QuantityBefore = fromQtyBefore,
                    QuantityAfter = fromStock.Quantity,
                    DestinationWarehouseId = dto.ToWarehouseId,
                    Notes = dto.Notes,
                    CreatedBy = _currentUser.UserId
                };
                _unitOfWork.StockMovements.Add(outMovement);

                // === 2. Nhập kho đích ===
                var toStock = await _unitOfWork.Repository<Stock>()
                    .FirstOrDefaultAsync(s => s.ProductId == dto.ProductId && s.WarehouseId == dto.ToWarehouseId);

                var toQtyBefore = toStock?.Quantity ?? 0;

                if (toStock == null)
                {
                    toStock = new Stock
                    {
                        ProductId = dto.ProductId,
                        WarehouseId = dto.ToWarehouseId,
                        Quantity = dto.Quantity,
                        CreatedBy = _currentUser.UserId
                    };
                    _unitOfWork.Repository<Stock>().Add(toStock);
                }
                else
                {
                    toStock.Quantity += dto.Quantity;
                    toStock.UpdateAt = DateTime.UtcNow;
                    _unitOfWork.Repository<Stock>().Update(toStock);
                }

                var inMovement = new StockMovement
                {
                    ProductId = dto.ProductId,
                    WarehouseId = dto.ToWarehouseId,
                    Quantity = dto.Quantity,
                    Type = StockMovementType.TransferIn,
                    QuantityBefore = toQtyBefore,
                    QuantityAfter = toStock.Quantity,
                    Notes = dto.Notes,
                    CreatedBy = _currentUser.UserId
                };
                _unitOfWork.StockMovements.Add(inMovement);

                // Commit transaction
                await _unitOfWork.CommitTransactionAsync();

                // Notify Real-time for both warehouses
                var tenantId = _currentUser.TenantId ?? Guid.Empty;

                // Out notification
                _ = _notificationService.NotifyStockUpdatedAsync(
                    tenantId,
                    outMovement.ProductId,
                    product.Name,
                    product.SKU,
                    outMovement.WarehouseId,
                    fromWarehouse.Name,
                    fromStock.Quantity + dto.Quantity, // oldQuantity
                    fromStock.Quantity,
                    0,
                    StockMovementType.TransferOut.ToString()
                );

                // In notification
                _ = _notificationService.NotifyStockUpdatedAsync(
                    tenantId,
                    inMovement.ProductId,
                    product.Name,
                    product.SKU,
                    inMovement.WarehouseId,
                    toWarehouse.Name,
                    toStock.Quantity - dto.Quantity, // oldQuantity
                    toStock.Quantity,
                    0,
                    StockMovementType.TransferIn.ToString()
                );

                // Return results
                return (
                    new StockMovementDto
                    {
                        Id = outMovement.Id,
                        ProductId = dto.ProductId,
                        ProductName = product.Name,
                        ProductSKU = product.SKU,
                        WarehouseName = fromWarehouse.Name,
                        Quantity = outMovement.Quantity,
                        Type = outMovement.Type.ToString(),
                        QuantityBefore = outMovement.QuantityBefore,
                        QuantityAfter = outMovement.QuantityAfter,
                        Notes = outMovement.Notes,
                        CreatedBy = outMovement.CreatedBy,
                        CreateAt = outMovement.CreateAt
                    },
                    new StockMovementDto
                    {
                        Id = inMovement.Id,
                        ProductId = dto.ProductId,
                        ProductName = product.Name,
                        ProductSKU = product.SKU,
                        WarehouseName = toWarehouse.Name,
                        Quantity = inMovement.Quantity,
                        Type = inMovement.Type.ToString(),
                        QuantityBefore = inMovement.QuantityBefore,
                        QuantityAfter = inMovement.QuantityAfter,
                        Notes = inMovement.Notes,
                        CreatedBy = inMovement.CreatedBy,
                        CreateAt = inMovement.CreateAt
                    }
                );
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        #endregion

        #region Order Integration

        /// <summary>
        /// Trừ kho khi bán hàng (Integrate with OrderService).
        /// Method này không gọi SaveChanges để OrderService quản lý transaction.
        /// </summary>
        public async Task ReduceStockForOrderAsync(
            Guid orderId, 
            string orderNumber, 
            List<CreateOrderItemDto> items)
        {
            if (items == null || !items.Any()) return;

            foreach (var item in items)
            {
                // 1. Get Stock
                var stock = await _unitOfWork.Repository<Stock>()
                    .FirstOrDefaultAsync(s => s.ProductId == item.ProductId && s.WarehouseId == item.WarehouseId);

                // Stock validation
                if (stock == null || stock.Quantity < item.Quantity)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                    throw new InvalidOperationException($"Sản phẩm '{product?.Name ?? "Unknown"}' (ID: {item.ProductId}) không đủ tồn kho tại kho (ID: {item.WarehouseId}). Tồn: {stock?.Quantity ?? 0}, Yêu cầu: {item.Quantity}");
                }

                // 2. Reduce Stock
                var qtyBefore = stock.Quantity;
                stock.Quantity -= item.Quantity;
                stock.UpdateAt = DateTime.UtcNow;
                _unitOfWork.Repository<Stock>().Update(stock);

                // 3. Update Product Total Stock
                var productEntity = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (productEntity != null)
                {
                    productEntity.StockQuantity -= item.Quantity;
                    _unitOfWork.Products.Update(productEntity);
                }

                // 4. Log Movement
                var movement = new StockMovement
                {
                    ProductId = item.ProductId,
                    WarehouseId = item.WarehouseId,
                    Quantity = -item.Quantity, // Negative for sale
                    Type = StockMovementType.Sale,
                    QuantityBefore = qtyBefore,
                    QuantityAfter = stock.Quantity,
                    ReferenceCode = orderNumber,
                    ReferenceId = orderId,
                    Notes = $"Xuất bán đơn hàng {orderNumber}",
                    CreatedBy = _currentUser.UserId
                };
                _unitOfWork.StockMovements.Add(movement);
            }
        }

        #endregion
    }
}
