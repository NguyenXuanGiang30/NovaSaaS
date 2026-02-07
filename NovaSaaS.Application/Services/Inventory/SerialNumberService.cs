using NovaSaaS.Application.DTOs.Business;
using NovaSaaS.Application.DTOs.Inventory;
using NovaSaaS.Domain.Entities.Inventory;
using NovaSaaS.Domain.Enums;
using NovaSaaS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NovaSaaS.Application.Services.Inventory
{
    public class SerialNumberService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public SerialNumberService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Guid> CreateAsync(CreateSerialNumberDto dto)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
            if (product == null) throw new ArgumentException("Sản phẩm không tồn tại.");

            var existing = await _unitOfWork.Repository<SerialNumber>()
                .FirstOrDefaultAsync(s => s.Serial == dto.Serial && s.ProductId == dto.ProductId);
            if (existing != null)
                throw new InvalidOperationException($"Serial '{dto.Serial}' đã tồn tại cho sản phẩm này.");

            var serial = new SerialNumber
            {
                ProductId = dto.ProductId,
                WarehouseId = dto.WarehouseId,
                Serial = dto.Serial,
                Status = SerialNumberStatus.Available,
                LotNumberId = dto.LotNumberId,
                WarrantyExpiry = dto.WarrantyExpiry,
                Notes = dto.Notes,
                CreateAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId
            };

            _unitOfWork.Repository<SerialNumber>().Add(serial);
            await _unitOfWork.CompleteAsync();
            return serial.Id;
        }

        public async Task<List<Guid>> CreateBatchAsync(CreateSerialNumberBatchDto dto)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
            if (product == null) throw new ArgumentException("Sản phẩm không tồn tại.");

            var ids = new List<Guid>();

            foreach (var serialCode in dto.Serials)
            {
                var existing = await _unitOfWork.Repository<SerialNumber>()
                    .FirstOrDefaultAsync(s => s.Serial == serialCode && s.ProductId == dto.ProductId);
                if (existing != null)
                    throw new InvalidOperationException($"Serial '{serialCode}' đã tồn tại cho sản phẩm này.");

                var serial = new SerialNumber
                {
                    ProductId = dto.ProductId,
                    WarehouseId = dto.WarehouseId,
                    Serial = serialCode,
                    Status = SerialNumberStatus.Available,
                    LotNumberId = dto.LotNumberId,
                    WarrantyExpiry = dto.WarrantyExpiry,
                    CreateAt = DateTime.UtcNow,
                    CreatedBy = _currentUser.UserId
                };

                _unitOfWork.Repository<SerialNumber>().Add(serial);
                ids.Add(serial.Id);
            }

            await _unitOfWork.CompleteAsync();
            return ids;
        }

        public async Task<PaginatedResult<SerialNumberDto>> GetAllAsync(SerialNumberFilterDto? filter = null)
        {
            filter ??= new SerialNumberFilterDto();

            var all = await _unitOfWork.Repository<SerialNumber>().GetAllAsync();
            var query = all.AsQueryable();

            if (filter.ProductId.HasValue)
                query = query.Where(s => s.ProductId == filter.ProductId.Value);
            if (filter.WarehouseId.HasValue)
                query = query.Where(s => s.WarehouseId == filter.WarehouseId.Value);
            if (filter.Status.HasValue)
                query = query.Where(s => s.Status == filter.Status.Value);
            if (filter.LotNumberId.HasValue)
                query = query.Where(s => s.LotNumberId == filter.LotNumberId.Value);
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var term = filter.SearchTerm.ToLower();
                query = query.Where(s => s.Serial.ToLower().Contains(term));
            }

            var totalCount = query.Count();
            var products = await _unitOfWork.Products.GetAllAsync();
            var warehouses = await _unitOfWork.Warehouses.GetAllAsync();
            var lots = await _unitOfWork.Repository<LotNumber>().GetAllAsync();
            var productDict = products.ToDictionary(p => p.Id);
            var warehouseDict = warehouses.ToDictionary(w => w.Id);
            var lotDict = lots.ToDictionary(l => l.Id, l => l.LotCode);

            var items = query
                .OrderByDescending(s => s.CreateAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(s => new SerialNumberDto
                {
                    Id = s.Id,
                    ProductId = s.ProductId,
                    ProductName = productDict.ContainsKey(s.ProductId) ? productDict[s.ProductId].Name : "",
                    ProductSKU = productDict.ContainsKey(s.ProductId) ? productDict[s.ProductId].SKU : "",
                    WarehouseId = s.WarehouseId,
                    WarehouseName = warehouseDict.ContainsKey(s.WarehouseId) ? warehouseDict[s.WarehouseId].Name : "",
                    Serial = s.Serial,
                    Status = s.Status,
                    LotNumberId = s.LotNumberId,
                    LotCode = s.LotNumberId.HasValue && lotDict.ContainsKey(s.LotNumberId.Value) ? lotDict[s.LotNumberId.Value] : null,
                    SoldOrderId = s.SoldOrderId,
                    SoldDate = s.SoldDate,
                    WarrantyExpiry = s.WarrantyExpiry,
                    Notes = s.Notes,
                    CreateAt = s.CreateAt
                }).ToList();

            return new PaginatedResult<SerialNumberDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<SerialNumberDto?> GetByIdAsync(Guid id)
        {
            var s = await _unitOfWork.Repository<SerialNumber>().GetByIdAsync(id);
            if (s == null) return null;

            var product = await _unitOfWork.Products.GetByIdAsync(s.ProductId);
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(s.WarehouseId);
            string? lotCode = null;
            if (s.LotNumberId.HasValue)
            {
                var lot = await _unitOfWork.Repository<LotNumber>().GetByIdAsync(s.LotNumberId.Value);
                lotCode = lot?.LotCode;
            }

            return new SerialNumberDto
            {
                Id = s.Id,
                ProductId = s.ProductId,
                ProductName = product?.Name ?? "",
                ProductSKU = product?.SKU ?? "",
                WarehouseId = s.WarehouseId,
                WarehouseName = warehouse?.Name ?? "",
                Serial = s.Serial,
                Status = s.Status,
                LotNumberId = s.LotNumberId,
                LotCode = lotCode,
                SoldOrderId = s.SoldOrderId,
                SoldDate = s.SoldDate,
                WarrantyExpiry = s.WarrantyExpiry,
                Notes = s.Notes,
                CreateAt = s.CreateAt
            };
        }
    }
}
