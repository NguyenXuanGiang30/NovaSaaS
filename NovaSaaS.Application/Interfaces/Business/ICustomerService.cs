using NovaSaaS.Application.DTOs.Business;
using NovaSaaS.Domain.Entities.Business;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NovaSaaS.Application.Interfaces.Business
{
    public interface ICustomerService
    {
        Task<Guid> CreateAsync(CreateCustomerDto dto);
        Task UpdateAsync(Guid id, UpdateCustomerDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<PaginatedResult<CustomerDto>> GetAllAsync(CustomerFilterDto? filter = null);
        Task<CustomerDto?> GetByIdAsync(Guid id);
        Task<List<OrderDto>> GetHistoryAsync(Guid customerId);
        Task RecalculateRankAsync(Guid customerId);
        Task UpdateSpendingAsync(Guid customerId, decimal amount, bool isAddition = true);
    }
}
