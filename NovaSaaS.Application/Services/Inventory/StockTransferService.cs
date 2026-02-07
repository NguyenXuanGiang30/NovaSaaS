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
    public class StockTransferService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public StockTransferService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Guid> CreateAsync(CreateStockTransferDto dto)
        {
            if (dto.FromWarehouseId == dto.ToWarehouseId)
                throw new InvalidOperationException("Kho nguồn và kho đích không được giống nhau.");

            var fromWarehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.FromWarehouseId);
            if (fromWarehouse == null) throw new ArgumentException("Kho nguồn không tồn tại.");

            var toWarehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.ToWarehouseId);
            if (toWarehouse == null) throw new ArgumentException("Kho đích không tồn tại.");

            if (dto.Items == null || !dto.Items.Any())
                throw new InvalidOperationException("Phiếu chuyển kho phải có ít nhất một sản phẩm.");

            var transfer = new StockTransfer
            {
                TransferNumber = await GenerateNumberAsync(),
                FromWarehouseId = dto.FromWarehouseId,
                ToWarehouseId = dto.ToWarehouseId,
                Status = StockTransferStatus.Pending,
                Notes = dto.Notes,
                CreateAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId
            };

            foreach (var item in dto.Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product == null)
                    throw new ArgumentException($"Sản phẩm {item.ProductId} không tồn tại.");

                var transferItem = new StockTransferItem
                {
                    StockTransferId = transfer.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Notes = item.Notes,
                    CreateAt = DateTime.UtcNow,
                    CreatedBy = _currentUser.UserId
                };

                _unitOfWork.Repository<StockTransferItem>().Add(transferItem);
            }

            _unitOfWork.Repository<StockTransfer>().Add(transfer);
            await _unitOfWork.CompleteAsync();
            return transfer.Id;
        }

        public async Task ApproveAsync(Guid id)
        {
            var transfer = await _unitOfWork.Repository<StockTransfer>().GetByIdAsync(id);
            if (transfer == null) throw new ArgumentException("Phiếu chuyển kho không tồn tại.");
            if (transfer.Status != StockTransferStatus.Pending)
                throw new InvalidOperationException("Phiếu chuyển kho không ở trạng thái chờ duyệt.");

            transfer.Status = StockTransferStatus.Approved;
            transfer.ApprovedByUserId = Guid.TryParse(_currentUser.UserId, out var userId) ? userId : null;
            transfer.ApprovedAt = DateTime.UtcNow;
            transfer.UpdateAt = DateTime.UtcNow;
            transfer.UpdatedBy = _currentUser.UserId;

            _unitOfWork.Repository<StockTransfer>().Update(transfer);
            await _unitOfWork.CompleteAsync();
        }

        public async Task CompleteAsync(Guid id)
        {
            var transfer = await _unitOfWork.Repository<StockTransfer>().GetByIdAsync(id);
            if (transfer == null) throw new ArgumentException("Phiếu chuyển kho không tồn tại.");
            if (transfer.Status != StockTransferStatus.Approved && transfer.Status != StockTransferStatus.InTransit)
                throw new InvalidOperationException("Phiếu chuyển kho chưa được duyệt.");

            var items = await _unitOfWork.Repository<StockTransferItem>()
                .FindAsync(i => i.StockTransferId == id);

            foreach (var item in items)
            {
                // Trừ kho nguồn
                var fromStock = await _unitOfWork.Stocks.FirstOrDefaultAsync(
                    s => s.ProductId == item.ProductId && s.WarehouseId == transfer.FromWarehouseId);

                if (fromStock == null || fromStock.Quantity < item.Quantity)
                    throw new InvalidOperationException($"Không đủ tồn kho cho sản phẩm {item.ProductId} tại kho nguồn.");

                var fromBefore = fromStock.Quantity;
                fromStock.Quantity -= item.Quantity;
                _unitOfWork.Stocks.Update(fromStock);

                // Cộng kho đích
                var toStock = await _unitOfWork.Stocks.FirstOrDefaultAsync(
                    s => s.ProductId == item.ProductId && s.WarehouseId == transfer.ToWarehouseId);

                if (toStock == null)
                {
                    toStock = new Stock
                    {
                        ProductId = item.ProductId,
                        WarehouseId = transfer.ToWarehouseId,
                        Quantity = 0,
                        CreateAt = DateTime.UtcNow,
                        CreatedBy = _currentUser.UserId
                    };
                    _unitOfWork.Stocks.Add(toStock);
                }

                var toBefore = toStock.Quantity;
                toStock.Quantity += item.Quantity;
                _unitOfWork.Stocks.Update(toStock);

                item.ReceivedQuantity = item.Quantity;
                _unitOfWork.Repository<StockTransferItem>().Update(item);

                // StockMovement - Xuất
                _unitOfWork.StockMovements.Add(new StockMovement
                {
                    ProductId = item.ProductId,
                    WarehouseId = transfer.FromWarehouseId,
                    Quantity = -item.Quantity,
                    Type = StockMovementType.TransferOut,
                    QuantityBefore = fromBefore,
                    QuantityAfter = fromStock.Quantity,
                    ReferenceCode = transfer.TransferNumber,
                    ReferenceId = transfer.Id,
                    DestinationWarehouseId = transfer.ToWarehouseId,
                    Notes = $"Chuyển kho đến {transfer.ToWarehouseId}",
                    CreateAt = DateTime.UtcNow,
                    CreatedBy = _currentUser.UserId
                });

                // StockMovement - Nhập
                _unitOfWork.StockMovements.Add(new StockMovement
                {
                    ProductId = item.ProductId,
                    WarehouseId = transfer.ToWarehouseId,
                    Quantity = item.Quantity,
                    Type = StockMovementType.TransferIn,
                    QuantityBefore = toBefore,
                    QuantityAfter = toStock.Quantity,
                    ReferenceCode = transfer.TransferNumber,
                    ReferenceId = transfer.Id,
                    Notes = $"Nhận chuyển kho từ {transfer.FromWarehouseId}",
                    CreateAt = DateTime.UtcNow,
                    CreatedBy = _currentUser.UserId
                });
            }

            transfer.Status = StockTransferStatus.Completed;
            transfer.CompletedAt = DateTime.UtcNow;
            transfer.UpdateAt = DateTime.UtcNow;
            transfer.UpdatedBy = _currentUser.UserId;

            _unitOfWork.Repository<StockTransfer>().Update(transfer);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<PaginatedResult<StockTransferDto>> GetAllAsync(StockTransferFilterDto? filter = null)
        {
            filter ??= new StockTransferFilterDto();

            var all = await _unitOfWork.Repository<StockTransfer>().GetAllAsync();
            var query = all.AsQueryable();

            if (filter.FromWarehouseId.HasValue)
                query = query.Where(t => t.FromWarehouseId == filter.FromWarehouseId.Value);
            if (filter.ToWarehouseId.HasValue)
                query = query.Where(t => t.ToWarehouseId == filter.ToWarehouseId.Value);
            if (filter.Status.HasValue)
                query = query.Where(t => t.Status == filter.Status.Value);
            if (filter.FromDate.HasValue)
                query = query.Where(t => t.CreateAt >= filter.FromDate.Value);
            if (filter.ToDate.HasValue)
                query = query.Where(t => t.CreateAt <= filter.ToDate.Value);

            var totalCount = query.Count();
            var warehouses = await _unitOfWork.Warehouses.GetAllAsync();
            var warehouseDict = warehouses.ToDictionary(w => w.Id, w => w.Name);
            var allItems = await _unitOfWork.Repository<StockTransferItem>().GetAllAsync();

            var pagedTransfers = query
                .OrderByDescending(t => t.CreateAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var items = pagedTransfers.Select(t =>
            {
                var transferItems = allItems.Where(i => i.StockTransferId == t.Id).ToList();
                return new StockTransferDto
                {
                    Id = t.Id,
                    TransferNumber = t.TransferNumber,
                    FromWarehouseId = t.FromWarehouseId,
                    FromWarehouseName = warehouseDict.ContainsKey(t.FromWarehouseId) ? warehouseDict[t.FromWarehouseId] : "",
                    ToWarehouseId = t.ToWarehouseId,
                    ToWarehouseName = warehouseDict.ContainsKey(t.ToWarehouseId) ? warehouseDict[t.ToWarehouseId] : "",
                    Status = t.Status,
                    Notes = t.Notes,
                    ApprovedByUserId = t.ApprovedByUserId,
                    ApprovedAt = t.ApprovedAt,
                    CompletedAt = t.CompletedAt,
                    CreatedBy = t.CreatedBy,
                    CreateAt = t.CreateAt,
                    TotalItems = transferItems.Count,
                    TotalQuantity = transferItems.Sum(i => i.Quantity)
                };
            }).ToList();

            return new PaginatedResult<StockTransferDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<StockTransferDto?> GetByIdAsync(Guid id)
        {
            var t = await _unitOfWork.Repository<StockTransfer>().GetByIdAsync(id);
            if (t == null) return null;

            var fromWarehouse = await _unitOfWork.Warehouses.GetByIdAsync(t.FromWarehouseId);
            var toWarehouse = await _unitOfWork.Warehouses.GetByIdAsync(t.ToWarehouseId);
            var transferItems = await _unitOfWork.Repository<StockTransferItem>()
                .FindAsync(i => i.StockTransferId == id);
            var products = await _unitOfWork.Products.GetAllAsync();
            var productDict = products.ToDictionary(p => p.Id);

            return new StockTransferDto
            {
                Id = t.Id,
                TransferNumber = t.TransferNumber,
                FromWarehouseId = t.FromWarehouseId,
                FromWarehouseName = fromWarehouse?.Name ?? "",
                ToWarehouseId = t.ToWarehouseId,
                ToWarehouseName = toWarehouse?.Name ?? "",
                Status = t.Status,
                Notes = t.Notes,
                ApprovedByUserId = t.ApprovedByUserId,
                ApprovedAt = t.ApprovedAt,
                CompletedAt = t.CompletedAt,
                CreatedBy = t.CreatedBy,
                CreateAt = t.CreateAt,
                TotalItems = transferItems.Count(),
                TotalQuantity = transferItems.Sum(i => i.Quantity),
                Items = transferItems.Select(i => new StockTransferItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = productDict.ContainsKey(i.ProductId) ? productDict[i.ProductId].Name : "",
                    ProductSKU = productDict.ContainsKey(i.ProductId) ? productDict[i.ProductId].SKU : "",
                    Quantity = i.Quantity,
                    ReceivedQuantity = i.ReceivedQuantity,
                    Notes = i.Notes
                }).ToList()
            };
        }

        private async Task<string> GenerateNumberAsync()
        {
            var count = await _unitOfWork.Repository<StockTransfer>().CountAsync();
            return $"TRF-{DateTime.UtcNow:yyyyMM}-{(count + 1):D4}";
        }
    }
}
