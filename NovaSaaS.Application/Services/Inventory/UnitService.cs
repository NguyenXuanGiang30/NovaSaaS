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
    /// UnitService - Quản lý đơn vị tính.
    /// </summary>
    public class UnitService : BaseService<Unit>
    {
        public UnitService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
            : base(unitOfWork, currentUser)
        {
        }

        /// <summary>
        /// Lấy tất cả đơn vị tính.
        /// </summary>
        public async Task<List<UnitDto>> GetAllUnitsAsync()
        {
            var units = await _repository.GetAllAsync();
            return units.Select(u => new UnitDto
            {
                Id = u.Id,
                Name = u.Name,
                ShortName = u.ShortName,
                CreateAt = u.CreateAt
            }).ToList();
        }

        /// <summary>
        /// Tạo đơn vị tính mới.
        /// </summary>
        public async Task<UnitDto> CreateUnitAsync(CreateUnitDto dto)
        {
            // Kiểm tra trùng tên
            var exists = await _repository.AnyAsync(u => u.Name.ToLower() == dto.Name.ToLower());
            if (exists)
                throw new InvalidOperationException($"Đơn vị tính '{dto.Name}' đã tồn tại");

            var unit = new Unit
            {
                Name = dto.Name,
                ShortName = dto.ShortName
            };

            await CreateAsync(unit);

            return new UnitDto
            {
                Id = unit.Id,
                Name = unit.Name,
                ShortName = unit.ShortName,
                CreateAt = unit.CreateAt
            };
        }

        /// <summary>
        /// Cập nhật đơn vị tính.
        /// </summary>
        public async Task<UnitDto?> UpdateUnitAsync(Guid id, UpdateUnitDto dto)
        {
            var unit = await _repository.GetByIdAsync(id);
            if (unit == null) return null;

            // Kiểm tra trùng tên (trừ chính nó)
            var exists = await _repository.AnyAsync(u => 
                u.Name.ToLower() == dto.Name.ToLower() && u.Id != id);
            if (exists)
                throw new InvalidOperationException($"Đơn vị tính '{dto.Name}' đã tồn tại");

            unit.Name = dto.Name;
            unit.ShortName = dto.ShortName;

            await UpdateAsync(unit);

            return new UnitDto
            {
                Id = unit.Id,
                Name = unit.Name,
                ShortName = unit.ShortName,
                CreateAt = unit.CreateAt
            };
        }

        protected override Task ValidateCreateAsync(Unit entity)
        {
            if (string.IsNullOrWhiteSpace(entity.Name))
                throw new ArgumentException("Tên đơn vị không được để trống");
            return Task.CompletedTask;
        }
    }
}
