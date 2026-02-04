using NovaSaaS.Application.DTOs.Business;
using NovaSaaS.Domain.Entities.Business;
using NovaSaaS.Domain.Enums;
using NovaSaaS.Domain.Interfaces;
using NovaSaaS.Application.Interfaces.Business; // Added ICustomerService usage
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NovaSaaS.Application.Services.Business
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        // Rank thresholds (VND)
        private static readonly Dictionary<CustomerRank, decimal> RankThresholds = new()
        {
            { CustomerRank.Standard, 0 },
            { CustomerRank.Bronze, 1_000_000 },
            { CustomerRank.Silver, 5_000_000 },
            { CustomerRank.Gold, 20_000_000 },
            { CustomerRank.Platinum, 50_000_000 },
            { CustomerRank.Diamond, 100_000_000 }
        };

        public CustomerService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        #region CRUD Operations

        public async Task<Guid> CreateAsync(CreateCustomerDto dto)
        {
            // Validate TaxCode unique
            if (!string.IsNullOrEmpty(dto.TaxCode))
            {
                var existing = await _unitOfWork.Customers.FirstOrDefaultAsync(c => c.TaxCode == dto.TaxCode);
                if (existing != null)
                    throw new InvalidOperationException($"Mã số thuế '{dto.TaxCode}' đã tồn tại.");
            }

            var customer = new Customer
            {
                Name = dto.Name,
                Phone = dto.Phone,
                TaxCode = dto.TaxCode,
                Address = dto.Address,
                Email = dto.Email,
                Type = dto.Type,
                Rank = CustomerRank.Standard,
                TotalSpending = 0,
                CreateAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId
            };

            _unitOfWork.Customers.Add(customer);
            await _unitOfWork.CompleteAsync();
            return customer.Id;
        }

        public async Task UpdateAsync(Guid id, UpdateCustomerDto dto)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            if (customer == null)
                throw new ArgumentException("Khách hàng không tồn tại.");

            // Validate TaxCode unique (exclude current customer)
            if (!string.IsNullOrEmpty(dto.TaxCode))
            {
                var existing = await _unitOfWork.Customers.FirstOrDefaultAsync(
                    c => c.TaxCode == dto.TaxCode && c.Id != id);
                if (existing != null)
                    throw new InvalidOperationException($"Mã số thuế '{dto.TaxCode}' đã tồn tại.");
            }

            customer.Name = dto.Name;
            customer.Phone = dto.Phone;
            customer.TaxCode = dto.TaxCode;
            customer.Address = dto.Address;
            customer.Email = dto.Email;
            customer.Type = dto.Type;
            customer.UpdateAt = DateTime.UtcNow;
            customer.UpdatedBy = _currentUser.UserId;

            _unitOfWork.Customers.Update(customer);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            if (customer == null)
                return false;

            // Check if customer has any orders
            var hasOrders = (await _unitOfWork.Orders.FindAsync(o => o.CustomerId == id)).Any();
            if (hasOrders)
                throw new InvalidOperationException("Không thể xóa khách hàng đã có đơn hàng. Hãy chuyển sang trạng thái ngưng hoạt động.");

            _unitOfWork.Customers.Remove(customer);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        #endregion

        #region Query Operations

        public async Task<PaginatedResult<CustomerDto>> GetAllAsync(CustomerFilterDto? filter = null)
        {
            filter ??= new CustomerFilterDto();

            var allCustomers = await _unitOfWork.Customers.GetAllAsync();
            var query = allCustomers.AsQueryable();

            // Apply filters
            if (filter.Type.HasValue)
                query = query.Where(c => c.Type == filter.Type.Value);

            if (filter.Rank.HasValue)
                query = query.Where(c => c.Rank == filter.Rank.Value);

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var term = filter.SearchTerm.ToLower();
                query = query.Where(c => 
                    c.Name.ToLower().Contains(term) || 
                    c.Phone.Contains(term) ||
                    (c.Email != null && c.Email.ToLower().Contains(term)));
            }

            var totalCount = query.Count();

            var items = query
                .OrderByDescending(c => c.CreateAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(c => MapToDto(c))
                .ToList();

            return new PaginatedResult<CustomerDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<CustomerDto?> GetByIdAsync(Guid id)
        {
            var c = await _unitOfWork.Customers.GetByIdAsync(id);
            if (c == null) return null;
            
            return MapToDto(c);
        }

        public async Task<List<OrderDto>> GetHistoryAsync(Guid customerId)
        {
            var orders = await _unitOfWork.Orders.FindAsync(o => o.CustomerId == customerId);
            return orders
                .OrderByDescending(o => o.CreateAt)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    CreateAt = o.CreateAt
                }).ToList();
        }

        #endregion

        #region Rank Management

        /// <summary>
        /// Tính toán lại rank dựa trên TotalSpending.
        /// Gọi method này sau mỗi đơn hàng hoàn thành.
        /// </summary>
        public async Task RecalculateRankAsync(Guid customerId)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null) return;

            var newRank = CalculateRank(customer.TotalSpending);
            
            if (customer.Rank != newRank)
            {
                customer.Rank = newRank;
                customer.UpdateAt = DateTime.UtcNow;
                _unitOfWork.Customers.Update(customer);
                await _unitOfWork.CompleteAsync();
            }
        }

        /// <summary>
        /// Cập nhật TotalSpending và recalculate rank.
        /// </summary>
        public async Task UpdateSpendingAsync(Guid customerId, decimal amount, bool isAddition = true)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null) return;

            if (isAddition)
                customer.TotalSpending += amount;
            else
                customer.TotalSpending = Math.Max(0, customer.TotalSpending - amount);

            customer.Rank = CalculateRank(customer.TotalSpending);
            customer.UpdateAt = DateTime.UtcNow;
            
            _unitOfWork.Customers.Update(customer);
            // Note: Not calling CompleteAsync here - let caller manage transaction
        }

        private static CustomerRank CalculateRank(decimal totalSpending)
        {
            if (totalSpending >= RankThresholds[CustomerRank.Diamond])
                return CustomerRank.Diamond;
            if (totalSpending >= RankThresholds[CustomerRank.Platinum])
                return CustomerRank.Platinum;
            if (totalSpending >= RankThresholds[CustomerRank.Gold])
                return CustomerRank.Gold;
            if (totalSpending >= RankThresholds[CustomerRank.Silver])
                return CustomerRank.Silver;
            if (totalSpending >= RankThresholds[CustomerRank.Bronze])
                return CustomerRank.Bronze;
            
            return CustomerRank.Standard;
        }

        #endregion

        #region Private Helpers

        private static CustomerDto MapToDto(Customer c)
        {
            return new CustomerDto
            {
                Id = c.Id,
                Name = c.Name,
                Phone = c.Phone,
                TaxCode = c.TaxCode,
                Rank = c.Rank,
                Type = c.Type,
                TotalSpending = c.TotalSpending,
                Address = c.Address,
                Email = c.Email,
                CreateAt = c.CreateAt
            };
        }

        #endregion
    }
}
