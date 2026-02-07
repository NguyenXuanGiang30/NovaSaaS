using NovaSaaS.Application.DTOs.Business;
using NovaSaaS.Application.DTOs.Inventory;
using NovaSaaS.Domain.Entities.Inventory;
using NovaSaaS.Domain.Enums;
using NovaSaaS.Domain.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NovaSaaS.Application.Services.Inventory
{
    public class StockAdjustmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public StockAdjustmentService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Guid> CreateAsync(CreateStockAdjustmentDto dto)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
            if (product == null) throw new ArgumentException("Sản phẩm không tồn tại.");

            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId);
            if (warehouse == null) throw new ArgumentException("Kho hàng không tồn tại.");

            var stock = await _unitOfWork.Stocks.FirstOrDefaultAsync(
                s => s.ProductId == dto.ProductId && s.WarehouseId == dto.WarehouseId);
            var currentQty = stock?.Quantity ?? 0;

            var adjustment = new StockAdjustment
            {
                AdjustmentNumber = await GenerateNumberAsync(),
                ProductId = dto.ProductId,
                WarehouseId = dto.WarehouseId,
                Type = dto.Type,
                Quantity = dto.Quantity,
                QuantityBefore = currentQty,
                QuantityAfter = IsAdditionType(dto.Type) ? currentQty + dto.Quantity : currentQty - dto.Quantity,
                Reason = dto.Reason,
                Status = StockAdjustmentStatus.Pending,
                Notes = dto.Notes,
                CreateAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId
            };

            _unitOfWork.Repository<StockAdjustment>().Add(adjustment);
            await _unitOfWork.CompleteAsync();
            return adjustment.Id;
        }

        public async Task ApproveAsync(Guid id)
        {
            var adjustment = await _unitOfWork.Repository<StockAdjustment>().GetByIdAsync(id);
            if (adjustment == null) throw new ArgumentException("Phiếu điều chỉnh không tồn tại.");
            if (adjustment.Status != StockAdjustmentStatus.Pending)
                throw new InvalidOperationException("Phiếu điều chỉnh không ở trạng thái chờ duyệt.");

            var stock = await _unitOfWork.Stocks.FirstOrDefaultAsync(
                s => s.ProductId == adjustment.ProductId && s.WarehouseId == adjustment.WarehouseId);

            if (stock == null)
            {
                stock = new Stock
                {
                    ProductId = adjustment.ProductId,
                    WarehouseId = adjustment.WarehouseId,
                    Quantity = 0,
                    CreateAt = DateTime.UtcNow,
                    CreatedBy = _currentUser.UserId
                };
                _unitOfWork.Stocks.Add(stock);
            }

            adjustment.QuantityBefore = stock.Quantity;

            if (IsAdditionType(adjustment.Type))
                stock.Quantity += adjustment.Quantity;
            else
                stock.Quantity = Math.Max(0, stock.Quantity - adjustment.Quantity);

            adjustment.QuantityAfter = stock.Quantity;
            adjustment.Status = StockAdjustmentStatus.Completed;
            adjustment.ApprovedByUserId = Guid.TryParse(_currentUser.UserId, out var userId) ? userId : null;
            adjustment.ApprovedAt = DateTime.UtcNow;
            adjustment.UpdateAt = DateTime.UtcNow;
            adjustment.UpdatedBy = _currentUser.UserId;

            // Tạo StockMovement
            var movementType = IsAdditionType(adjustment.Type) 
                ? StockMovementType.AdjustmentAdd 
                : StockMovementType.AdjustmentSubtract;

            var movement = new StockMovement
            {
                ProductId = adjustment.ProductId,
                WarehouseId = adjustment.WarehouseId,
                Quantity = IsAdditionType(adjustment.Type) ? adjustment.Quantity : -adjustment.Quantity,
                Type = movementType,
                QuantityBefore = adjustment.QuantityBefore,
                QuantityAfter = adjustment.QuantityAfter,
                ReferenceCode = adjustment.AdjustmentNumber,
                ReferenceId = adjustment.Id,
                Notes = adjustment.Reason,
                CreateAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId
            };

            _unitOfWork.Stocks.Update(stock);
            _unitOfWork.Repository<StockAdjustment>().Update(adjustment);
            _unitOfWork.StockMovements.Add(movement);

            // Update product total stock
            var product = await _unitOfWork.Products.GetByIdAsync(adjustment.ProductId);
            if (product != null)
            {
                var allStocks = await _unitOfWork.Stocks.FindAsync(s => s.ProductId == product.Id);
                product.StockQuantity = allStocks.Sum(s => s.Quantity);
                _unitOfWork.Products.Update(product);
            }

            await _unitOfWork.CompleteAsync();
        }

        public async Task RejectAsync(Guid id, string? reason = null)
        {
            var adjustment = await _unitOfWork.Repository<StockAdjustment>().GetByIdAsync(id);
            if (adjustment == null) throw new ArgumentException("Phiếu điều chỉnh không tồn tại.");
            if (adjustment.Status != StockAdjustmentStatus.Pending)
                throw new InvalidOperationException("Phiếu điều chỉnh không ở trạng thái chờ duyệt.");

            adjustment.Status = StockAdjustmentStatus.Rejected;
            adjustment.ApprovedByUserId = Guid.TryParse(_currentUser.UserId, out var userId) ? userId : null;
            adjustment.ApprovedAt = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(reason))
                adjustment.Notes = (adjustment.Notes ?? "") + $"\nLý do từ chối: {reason}";
            adjustment.UpdateAt = DateTime.UtcNow;
            adjustment.UpdatedBy = _currentUser.UserId;

            _unitOfWork.Repository<StockAdjustment>().Update(adjustment);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<PaginatedResult<StockAdjustmentDetailDto>> GetAllAsync(StockAdjustmentFilterDto? filter = null)
        {
            filter ??= new StockAdjustmentFilterDto();

            var all = await _unitOfWork.Repository<StockAdjustment>().GetAllAsync();
            var query = all.AsQueryable();

            if (filter.ProductId.HasValue)
                query = query.Where(a => a.ProductId == filter.ProductId.Value);
            if (filter.WarehouseId.HasValue)
                query = query.Where(a => a.WarehouseId == filter.WarehouseId.Value);
            if (filter.Type.HasValue)
                query = query.Where(a => a.Type == filter.Type.Value);
            if (filter.Status.HasValue)
                query = query.Where(a => a.Status == filter.Status.Value);
            if (filter.FromDate.HasValue)
                query = query.Where(a => a.CreateAt >= filter.FromDate.Value);
            if (filter.ToDate.HasValue)
                query = query.Where(a => a.CreateAt <= filter.ToDate.Value);

            var totalCount = query.Count();
            var products = await _unitOfWork.Products.GetAllAsync();
            var warehouses = await _unitOfWork.Warehouses.GetAllAsync();
            var productDict = products.ToDictionary(p => p.Id);
            var warehouseDict = warehouses.ToDictionary(w => w.Id);

            var items = query
                .OrderByDescending(a => a.CreateAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(a => new StockAdjustmentDetailDto
                {
                    Id = a.Id,
                    AdjustmentNumber = a.AdjustmentNumber,
                    ProductId = a.ProductId,
                    ProductName = productDict.ContainsKey(a.ProductId) ? productDict[a.ProductId].Name : "",
                    ProductSKU = productDict.ContainsKey(a.ProductId) ? productDict[a.ProductId].SKU : "",
                    WarehouseId = a.WarehouseId,
                    WarehouseName = warehouseDict.ContainsKey(a.WarehouseId) ? warehouseDict[a.WarehouseId].Name : "",
                    Type = a.Type,
                    Quantity = a.Quantity,
                    QuantityBefore = a.QuantityBefore,
                    QuantityAfter = a.QuantityAfter,
                    Reason = a.Reason,
                    Status = a.Status,
                    ApprovedByUserId = a.ApprovedByUserId,
                    ApprovedAt = a.ApprovedAt,
                    Notes = a.Notes,
                    CreatedBy = a.CreatedBy,
                    CreateAt = a.CreateAt
                }).ToList();

            return new PaginatedResult<StockAdjustmentDetailDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<StockAdjustmentDetailDto?> GetByIdAsync(Guid id)
        {
            var a = await _unitOfWork.Repository<StockAdjustment>().GetByIdAsync(id);
            if (a == null) return null;

            var product = await _unitOfWork.Products.GetByIdAsync(a.ProductId);
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(a.WarehouseId);

            return new StockAdjustmentDetailDto
            {
                Id = a.Id,
                AdjustmentNumber = a.AdjustmentNumber,
                ProductId = a.ProductId,
                ProductName = product?.Name ?? "",
                ProductSKU = product?.SKU ?? "",
                WarehouseId = a.WarehouseId,
                WarehouseName = warehouse?.Name ?? "",
                Type = a.Type,
                Quantity = a.Quantity,
                QuantityBefore = a.QuantityBefore,
                QuantityAfter = a.QuantityAfter,
                Reason = a.Reason,
                Status = a.Status,
                ApprovedByUserId = a.ApprovedByUserId,
                ApprovedAt = a.ApprovedAt,
                Notes = a.Notes,
                CreatedBy = a.CreatedBy,
                CreateAt = a.CreateAt
            };
        }

        private static bool IsAdditionType(StockAdjustmentType type)
        {
            return type == StockAdjustmentType.Addition || 
                   type == StockAdjustmentType.Found || 
                   type == StockAdjustmentType.Return;
        }

        private async Task<string> GenerateNumberAsync()
        {
            var count = await _unitOfWork.Repository<StockAdjustment>().CountAsync();
            return $"ADJ-{DateTime.UtcNow:yyyyMM}-{(count + 1):D4}";
        }
    }
}
