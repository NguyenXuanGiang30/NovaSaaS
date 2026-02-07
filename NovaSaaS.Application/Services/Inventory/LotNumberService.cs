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
    public class LotNumberService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public LotNumberService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Guid> CreateAsync(CreateLotNumberDto dto)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
            if (product == null) throw new ArgumentException("Sản phẩm không tồn tại.");

            var existing = await _unitOfWork.Repository<LotNumber>()
                .FirstOrDefaultAsync(l => l.LotCode == dto.LotCode && l.ProductId == dto.ProductId);
            if (existing != null)
                throw new InvalidOperationException($"Mã lô '{dto.LotCode}' đã tồn tại cho sản phẩm này.");

            var lot = new LotNumber
            {
                ProductId = dto.ProductId,
                WarehouseId = dto.WarehouseId,
                LotCode = dto.LotCode,
                ManufactureDate = dto.ManufactureDate,
                ExpiryDate = dto.ExpiryDate,
                Quantity = dto.Quantity,
                SupplierName = dto.SupplierName,
                Notes = dto.Notes,
                Status = LotStatus.Active,
                CreateAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId
            };

            _unitOfWork.Repository<LotNumber>().Add(lot);
            await _unitOfWork.CompleteAsync();
            return lot.Id;
        }

        public async Task<PaginatedResult<LotNumberDto>> GetAllAsync(LotNumberFilterDto? filter = null)
        {
            filter ??= new LotNumberFilterDto();

            var all = await _unitOfWork.Repository<LotNumber>().GetAllAsync();
            var query = all.AsQueryable();

            if (filter.ProductId.HasValue)
                query = query.Where(l => l.ProductId == filter.ProductId.Value);
            if (filter.WarehouseId.HasValue)
                query = query.Where(l => l.WarehouseId == filter.WarehouseId.Value);
            if (filter.Status.HasValue)
                query = query.Where(l => l.Status == filter.Status.Value);
            if (filter.IsExpired == true)
                query = query.Where(l => l.ExpiryDate.HasValue && l.ExpiryDate.Value < DateTime.UtcNow);
            if (filter.IsExpired == false)
                query = query.Where(l => !l.ExpiryDate.HasValue || l.ExpiryDate.Value >= DateTime.UtcNow);
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var term = filter.SearchTerm.ToLower();
                query = query.Where(l => l.LotCode.ToLower().Contains(term));
            }

            var totalCount = query.Count();
            var products = await _unitOfWork.Products.GetAllAsync();
            var warehouses = await _unitOfWork.Warehouses.GetAllAsync();
            var productDict = products.ToDictionary(p => p.Id);
            var warehouseDict = warehouses.ToDictionary(w => w.Id);

            var items = query
                .OrderByDescending(l => l.CreateAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(l => new LotNumberDto
                {
                    Id = l.Id,
                    ProductId = l.ProductId,
                    ProductName = productDict.ContainsKey(l.ProductId) ? productDict[l.ProductId].Name : "",
                    ProductSKU = productDict.ContainsKey(l.ProductId) ? productDict[l.ProductId].SKU : "",
                    WarehouseId = l.WarehouseId,
                    WarehouseName = warehouseDict.ContainsKey(l.WarehouseId) ? warehouseDict[l.WarehouseId].Name : "",
                    LotCode = l.LotCode,
                    ManufactureDate = l.ManufactureDate,
                    ExpiryDate = l.ExpiryDate,
                    Quantity = l.Quantity,
                    SoldQuantity = l.SoldQuantity,
                    Status = l.Status,
                    SupplierName = l.SupplierName,
                    Notes = l.Notes,
                    CreateAt = l.CreateAt
                }).ToList();

            return new PaginatedResult<LotNumberDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<LotNumberDto?> GetByIdAsync(Guid id)
        {
            var l = await _unitOfWork.Repository<LotNumber>().GetByIdAsync(id);
            if (l == null) return null;

            var product = await _unitOfWork.Products.GetByIdAsync(l.ProductId);
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(l.WarehouseId);

            return new LotNumberDto
            {
                Id = l.Id,
                ProductId = l.ProductId,
                ProductName = product?.Name ?? "",
                ProductSKU = product?.SKU ?? "",
                WarehouseId = l.WarehouseId,
                WarehouseName = warehouse?.Name ?? "",
                LotCode = l.LotCode,
                ManufactureDate = l.ManufactureDate,
                ExpiryDate = l.ExpiryDate,
                Quantity = l.Quantity,
                SoldQuantity = l.SoldQuantity,
                Status = l.Status,
                SupplierName = l.SupplierName,
                Notes = l.Notes,
                CreateAt = l.CreateAt
            };
        }
    }
}
