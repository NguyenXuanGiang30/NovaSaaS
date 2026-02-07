using NovaSaaS.Application.DTOs.Business;
using NovaSaaS.Domain.Entities.Business;
using NovaSaaS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NovaSaaS.Application.Services.Business
{
    public class CustomerGroupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public CustomerGroupService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Guid> CreateAsync(CreateCustomerGroupDto dto)
        {
            var existing = await _unitOfWork.Repository<CustomerGroup>()
                .FirstOrDefaultAsync(g => g.Name == dto.Name);
            if (existing != null)
                throw new InvalidOperationException($"Nhóm khách hàng '{dto.Name}' đã tồn tại.");

            var group = new CustomerGroup
            {
                Name = dto.Name,
                Description = dto.Description,
                DiscountPercent = dto.DiscountPercent,
                CreateAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId
            };

            _unitOfWork.Repository<CustomerGroup>().Add(group);
            await _unitOfWork.CompleteAsync();
            return group.Id;
        }

        public async Task UpdateAsync(Guid id, UpdateCustomerGroupDto dto)
        {
            var group = await _unitOfWork.Repository<CustomerGroup>().GetByIdAsync(id);
            if (group == null) throw new ArgumentException("Nhóm khách hàng không tồn tại.");

            group.Name = dto.Name;
            group.Description = dto.Description;
            group.DiscountPercent = dto.DiscountPercent;
            group.IsActive = dto.IsActive;
            group.UpdateAt = DateTime.UtcNow;
            group.UpdatedBy = _currentUser.UserId;

            _unitOfWork.Repository<CustomerGroup>().Update(group);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var group = await _unitOfWork.Repository<CustomerGroup>().GetByIdAsync(id);
            if (group == null) return false;

            var hasCustomers = await _unitOfWork.Customers.AnyAsync(c => c.CustomerGroupId == id);
            if (hasCustomers)
                throw new InvalidOperationException("Không thể xóa nhóm khách hàng đang có khách hàng.");

            _unitOfWork.Repository<CustomerGroup>().Remove(group);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<List<CustomerGroupDto>> GetAllAsync()
        {
            var groups = await _unitOfWork.Repository<CustomerGroup>().GetAllAsync();
            var customers = await _unitOfWork.Customers.GetAllAsync();

            return groups.Select(g => new CustomerGroupDto
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                DiscountPercent = g.DiscountPercent,
                IsActive = g.IsActive,
                CustomerCount = customers.Count(c => c.CustomerGroupId == g.Id),
                CreateAt = g.CreateAt
            }).OrderBy(g => g.Name).ToList();
        }

        public async Task<CustomerGroupDto?> GetByIdAsync(Guid id)
        {
            var g = await _unitOfWork.Repository<CustomerGroup>().GetByIdAsync(id);
            if (g == null) return null;

            var customerCount = await _unitOfWork.Customers.CountAsync(c => c.CustomerGroupId == id);

            return new CustomerGroupDto
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                DiscountPercent = g.DiscountPercent,
                IsActive = g.IsActive,
                CustomerCount = customerCount,
                CreateAt = g.CreateAt
            };
        }
    }
}
