using NovaSaaS.Application.DTOs.Inventory;
using NovaSaaS.Domain.Entities.Inventory;
using NovaSaaS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NovaSaaS.Application.Services.Inventory
{
    /// <summary>
    /// WarehouseService - Quản lý kho hàng.
    /// </summary>
    public class WarehouseService : BaseService<Warehouse>
    {
        public WarehouseService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
            : base(unitOfWork, currentUser)
        {
        }

        /// <summary>
        /// Lấy danh sách kho hàng.
        /// </summary>
        public async Task<List<WarehouseDto>> GetAllWarehousesAsync()
        {
            var warehouses = await _repository.GetAllAsync();
            var stocks = await _unitOfWork.Repository<Stock>().GetAllAsync();

            return warehouses.Select(w => 
            {
                var warehouseStocks = stocks.Where(s => s.WarehouseId == w.Id);
                return new WarehouseDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    Address = w.Adress,
                    TotalProducts = warehouseStocks.Select(s => s.ProductId).Distinct().Count(),
                    TotalQuantity = warehouseStocks.Sum(s => s.Quantity),
                    CreateAt = w.CreateAt
                };
            })
            .OrderBy(w => w.Name)
            .ToList();
        }

        /// <summary>
        /// Lấy chi tiết kho hàng với danh sách sản phẩm.
        /// </summary>
        public async Task<WarehouseDto?> GetWarehouseByIdAsync(Guid id)
        {
            var warehouse = await _repository.GetByIdAsync(id);
            if (warehouse == null) return null;

            var stocks = await _unitOfWork.Repository<Stock>()
                .FindAsync(s => s.WarehouseId == id);

            return new WarehouseDto
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                Address = warehouse.Adress,
                TotalProducts = stocks.Select(s => s.ProductId).Distinct().Count(),
                TotalQuantity = stocks.Sum(s => s.Quantity),
                CreateAt = warehouse.CreateAt
            };
        }

        /// <summary>
        /// Tạo kho hàng mới.
        /// </summary>
        public async Task<WarehouseDto> CreateWarehouseAsync(CreateWarehouseDto dto)
        {
            // Kiểm tra trùng tên
            var exists = await _repository.AnyAsync(w => w.Name.ToLower() == dto.Name.ToLower());
            if (exists)
                throw new InvalidOperationException($"Kho '{dto.Name}' đã tồn tại");

            var warehouse = new Warehouse
            {
                Name = dto.Name,
                Adress = dto.Address ?? ""
            };

            await CreateAsync(warehouse);

            return new WarehouseDto
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                Address = warehouse.Adress,
                TotalProducts = 0,
                TotalQuantity = 0,
                CreateAt = warehouse.CreateAt
            };
        }

        /// <summary>
        /// Cập nhật kho hàng.
        /// </summary>
        public async Task<WarehouseDto?> UpdateWarehouseAsync(Guid id, UpdateWarehouseDto dto)
        {
            var warehouse = await _repository.GetByIdAsync(id);
            if (warehouse == null) return null;

            // Kiểm tra trùng tên (trừ chính nó)
            var exists = await _repository.AnyAsync(w => 
                w.Name.ToLower() == dto.Name.ToLower() && w.Id != id);
            if (exists)
                throw new InvalidOperationException($"Kho '{dto.Name}' đã tồn tại");

            warehouse.Name = dto.Name;
            warehouse.Adress = dto.Address ?? "";

            await UpdateAsync(warehouse);

            return await GetWarehouseByIdAsync(id);
        }

        protected override Task ValidateCreateAsync(Warehouse entity)
        {
            if (string.IsNullOrWhiteSpace(entity.Name))
                throw new ArgumentException("Tên kho không được để trống");
            return Task.CompletedTask;
        }
    }
}
