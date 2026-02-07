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
    public class InventoryCountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public InventoryCountService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Guid> CreateAsync(CreateInventoryCountDto dto)
        {
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId);
            if (warehouse == null) throw new ArgumentException("Kho hàng không tồn tại.");

            var count = new InventoryCount
            {
                CountNumber = await GenerateNumberAsync(),
                WarehouseId = dto.WarehouseId,
                Status = InventoryCountStatus.Draft,
                CountDate = dto.CountDate ?? DateTime.UtcNow,
                Notes = dto.Notes,
                CreateAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId
            };

            int totalDiscrepancy = 0;

            foreach (var item in dto.Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product == null)
                    throw new ArgumentException($"Sản phẩm {item.ProductId} không tồn tại.");

                var stock = await _unitOfWork.Stocks.FirstOrDefaultAsync(
                    s => s.ProductId == item.ProductId && s.WarehouseId == dto.WarehouseId);
                var systemQty = stock?.Quantity ?? 0;

                var countItem = new InventoryCountItem
                {
                    InventoryCountId = count.Id,
                    ProductId = item.ProductId,
                    SystemQuantity = systemQty,
                    ActualQuantity = item.ActualQuantity,
                    Notes = item.Notes,
                    CreateAt = DateTime.UtcNow,
                    CreatedBy = _currentUser.UserId
                };

                totalDiscrepancy += Math.Abs(item.ActualQuantity - systemQty);
                _unitOfWork.Repository<InventoryCountItem>().Add(countItem);
            }

            count.TotalDiscrepancy = totalDiscrepancy;
            _unitOfWork.Repository<InventoryCount>().Add(count);
            await _unitOfWork.CompleteAsync();
            return count.Id;
        }

        public async Task CompleteAsync(Guid id)
        {
            var count = await _unitOfWork.Repository<InventoryCount>().GetByIdAsync(id);
            if (count == null) throw new ArgumentException("Phiếu kiểm kê không tồn tại.");
            if (count.Status == InventoryCountStatus.Completed)
                throw new InvalidOperationException("Phiếu kiểm kê đã hoàn thành.");

            var items = await _unitOfWork.Repository<InventoryCountItem>()
                .FindAsync(i => i.InventoryCountId == id);

            foreach (var item in items)
            {
                if (item.ActualQuantity == item.SystemQuantity) continue;

                var stock = await _unitOfWork.Stocks.FirstOrDefaultAsync(
                    s => s.ProductId == item.ProductId && s.WarehouseId == count.WarehouseId);

                if (stock == null)
                {
                    stock = new Stock
                    {
                        ProductId = item.ProductId,
                        WarehouseId = count.WarehouseId,
                        Quantity = 0,
                        CreateAt = DateTime.UtcNow,
                        CreatedBy = _currentUser.UserId
                    };
                    _unitOfWork.Stocks.Add(stock);
                }

                var before = stock.Quantity;
                stock.Quantity = item.ActualQuantity;
                _unitOfWork.Stocks.Update(stock);

                var movementType = item.ActualQuantity > item.SystemQuantity
                    ? StockMovementType.AdjustmentAdd
                    : StockMovementType.AdjustmentSubtract;

                _unitOfWork.StockMovements.Add(new StockMovement
                {
                    ProductId = item.ProductId,
                    WarehouseId = count.WarehouseId,
                    Quantity = item.ActualQuantity - item.SystemQuantity,
                    Type = movementType,
                    QuantityBefore = before,
                    QuantityAfter = item.ActualQuantity,
                    ReferenceCode = count.CountNumber,
                    ReferenceId = count.Id,
                    Notes = $"Kiểm kê: Hệ thống={item.SystemQuantity}, Thực tế={item.ActualQuantity}",
                    CreateAt = DateTime.UtcNow,
                    CreatedBy = _currentUser.UserId
                });

                // Update product total stock
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    var allStocks = await _unitOfWork.Stocks.FindAsync(s => s.ProductId == product.Id);
                    product.StockQuantity = allStocks.Sum(s => s.Quantity);
                    _unitOfWork.Products.Update(product);
                }
            }

            count.Status = InventoryCountStatus.Completed;
            count.CompletedAt = DateTime.UtcNow;
            count.ApprovedByUserId = Guid.TryParse(_currentUser.UserId, out var userId) ? userId : null;
            count.UpdateAt = DateTime.UtcNow;
            count.UpdatedBy = _currentUser.UserId;

            _unitOfWork.Repository<InventoryCount>().Update(count);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<PaginatedResult<InventoryCountDto>> GetAllAsync(InventoryCountFilterDto? filter = null)
        {
            filter ??= new InventoryCountFilterDto();

            var all = await _unitOfWork.Repository<InventoryCount>().GetAllAsync();
            var query = all.AsQueryable();

            if (filter.WarehouseId.HasValue)
                query = query.Where(c => c.WarehouseId == filter.WarehouseId.Value);
            if (filter.Status.HasValue)
                query = query.Where(c => c.Status == filter.Status.Value);
            if (filter.FromDate.HasValue)
                query = query.Where(c => c.CountDate >= filter.FromDate.Value);
            if (filter.ToDate.HasValue)
                query = query.Where(c => c.CountDate <= filter.ToDate.Value);

            var totalCount = query.Count();
            var warehouses = await _unitOfWork.Warehouses.GetAllAsync();
            var warehouseDict = warehouses.ToDictionary(w => w.Id, w => w.Name);
            var allItems = await _unitOfWork.Repository<InventoryCountItem>().GetAllAsync();

            var items = query
                .OrderByDescending(c => c.CreateAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(c => new InventoryCountDto
                {
                    Id = c.Id,
                    CountNumber = c.CountNumber,
                    WarehouseId = c.WarehouseId,
                    WarehouseName = warehouseDict.ContainsKey(c.WarehouseId) ? warehouseDict[c.WarehouseId] : "",
                    Status = c.Status,
                    CountDate = c.CountDate,
                    Notes = c.Notes,
                    TotalDiscrepancy = c.TotalDiscrepancy,
                    ApprovedByUserId = c.ApprovedByUserId,
                    CompletedAt = c.CompletedAt,
                    CreatedBy = c.CreatedBy,
                    CreateAt = c.CreateAt,
                    TotalItems = allItems.Count(i => i.InventoryCountId == c.Id)
                }).ToList();

            return new PaginatedResult<InventoryCountDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<InventoryCountDto?> GetByIdAsync(Guid id)
        {
            var c = await _unitOfWork.Repository<InventoryCount>().GetByIdAsync(id);
            if (c == null) return null;

            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(c.WarehouseId);
            var countItems = await _unitOfWork.Repository<InventoryCountItem>()
                .FindAsync(i => i.InventoryCountId == id);
            var products = await _unitOfWork.Products.GetAllAsync();
            var productDict = products.ToDictionary(p => p.Id);

            return new InventoryCountDto
            {
                Id = c.Id,
                CountNumber = c.CountNumber,
                WarehouseId = c.WarehouseId,
                WarehouseName = warehouse?.Name ?? "",
                Status = c.Status,
                CountDate = c.CountDate,
                Notes = c.Notes,
                TotalDiscrepancy = c.TotalDiscrepancy,
                ApprovedByUserId = c.ApprovedByUserId,
                CompletedAt = c.CompletedAt,
                CreatedBy = c.CreatedBy,
                CreateAt = c.CreateAt,
                TotalItems = countItems.Count(),
                Items = countItems.Select(i => new InventoryCountItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = productDict.ContainsKey(i.ProductId) ? productDict[i.ProductId].Name : "",
                    ProductSKU = productDict.ContainsKey(i.ProductId) ? productDict[i.ProductId].SKU : "",
                    SystemQuantity = i.SystemQuantity,
                    ActualQuantity = i.ActualQuantity,
                    Notes = i.Notes
                }).ToList()
            };
        }

        private async Task<string> GenerateNumberAsync()
        {
            var count = await _unitOfWork.Repository<InventoryCount>().CountAsync();
            return $"CNT-{DateTime.UtcNow:yyyyMM}-{(count + 1):D4}";
        }
    }
}
