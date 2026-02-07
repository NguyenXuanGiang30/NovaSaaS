using NovaSaaS.Application.DTOs.Inventory;
using NovaSaaS.Domain.Entities.Inventory;
using NovaSaaS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NovaSaaS.Application.Services.Inventory
{
    public class LocationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public LocationService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Guid> CreateAsync(CreateLocationDto dto)
        {
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId);
            if (warehouse == null) throw new ArgumentException("Kho hàng không tồn tại.");

            var existing = await _unitOfWork.Repository<Location>()
                .FirstOrDefaultAsync(l => l.WarehouseId == dto.WarehouseId && l.Code == dto.Code);
            if (existing != null)
                throw new InvalidOperationException($"Mã vị trí '{dto.Code}' đã tồn tại trong kho này.");

            var location = new Location
            {
                WarehouseId = dto.WarehouseId,
                Code = dto.Code,
                Name = dto.Name,
                Aisle = dto.Aisle,
                Rack = dto.Rack,
                Shelf = dto.Shelf,
                MaxCapacity = dto.MaxCapacity,
                IsActive = true,
                CreateAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId
            };

            _unitOfWork.Repository<Location>().Add(location);
            await _unitOfWork.CompleteAsync();
            return location.Id;
        }

        public async Task UpdateAsync(Guid id, UpdateLocationDto dto)
        {
            var location = await _unitOfWork.Repository<Location>().GetByIdAsync(id);
            if (location == null) throw new ArgumentException("Vị trí không tồn tại.");

            location.Code = dto.Code;
            location.Name = dto.Name;
            location.Aisle = dto.Aisle;
            location.Rack = dto.Rack;
            location.Shelf = dto.Shelf;
            location.MaxCapacity = dto.MaxCapacity;
            location.IsActive = dto.IsActive;
            location.UpdateAt = DateTime.UtcNow;
            location.UpdatedBy = _currentUser.UserId;

            _unitOfWork.Repository<Location>().Update(location);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var location = await _unitOfWork.Repository<Location>().GetByIdAsync(id);
            if (location == null) return false;

            _unitOfWork.Repository<Location>().Remove(location);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<List<LocationDto>> GetByWarehouseIdAsync(Guid warehouseId)
        {
            var locations = await _unitOfWork.Repository<Location>()
                .FindAsync(l => l.WarehouseId == warehouseId);
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(warehouseId);

            return locations.Select(l => new LocationDto
            {
                Id = l.Id,
                WarehouseId = l.WarehouseId,
                WarehouseName = warehouse?.Name ?? "",
                Code = l.Code,
                Name = l.Name,
                Aisle = l.Aisle,
                Rack = l.Rack,
                Shelf = l.Shelf,
                IsActive = l.IsActive,
                MaxCapacity = l.MaxCapacity,
                CreateAt = l.CreateAt
            }).OrderBy(l => l.Code).ToList();
        }

        public async Task<LocationDto?> GetByIdAsync(Guid id)
        {
            var l = await _unitOfWork.Repository<Location>().GetByIdAsync(id);
            if (l == null) return null;

            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(l.WarehouseId);

            return new LocationDto
            {
                Id = l.Id,
                WarehouseId = l.WarehouseId,
                WarehouseName = warehouse?.Name ?? "",
                Code = l.Code,
                Name = l.Name,
                Aisle = l.Aisle,
                Rack = l.Rack,
                Shelf = l.Shelf,
                IsActive = l.IsActive,
                MaxCapacity = l.MaxCapacity,
                CreateAt = l.CreateAt
            };
        }
    }
}
