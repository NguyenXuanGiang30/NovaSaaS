using NovaSaaS.Application.DTOs.Business;
using NovaSaaS.Domain.Entities.Business;
using NovaSaaS.Domain.Enums;
using NovaSaaS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NovaSaaS.Application.Services.Business
{
    public class LeadService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public LeadService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Guid> CreateAsync(CreateLeadDto dto)
        {
            var lead = new Lead
            {
                ContactName = dto.ContactName,
                CompanyName = dto.CompanyName,
                Email = dto.Email,
                Phone = dto.Phone,
                Source = dto.Source,
                Status = LeadStatus.New,
                EstimatedValue = dto.EstimatedValue,
                Notes = dto.Notes,
                AssignedToUserId = dto.AssignedToUserId,
                CreateAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId
            };

            _unitOfWork.Repository<Lead>().Add(lead);
            await _unitOfWork.CompleteAsync();
            return lead.Id;
        }

        public async Task UpdateAsync(Guid id, UpdateLeadDto dto)
        {
            var lead = await _unitOfWork.Repository<Lead>().GetByIdAsync(id);
            if (lead == null) throw new ArgumentException("Lead không tồn tại.");

            lead.ContactName = dto.ContactName;
            lead.CompanyName = dto.CompanyName;
            lead.Email = dto.Email;
            lead.Phone = dto.Phone;
            lead.Source = dto.Source;
            lead.Status = dto.Status;
            lead.EstimatedValue = dto.EstimatedValue;
            lead.Notes = dto.Notes;
            lead.AssignedToUserId = dto.AssignedToUserId;
            lead.UpdateAt = DateTime.UtcNow;
            lead.UpdatedBy = _currentUser.UserId;

            _unitOfWork.Repository<Lead>().Update(lead);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var lead = await _unitOfWork.Repository<Lead>().GetByIdAsync(id);
            if (lead == null) return false;

            _unitOfWork.Repository<Lead>().SoftDelete(lead);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<PaginatedResult<LeadDto>> GetAllAsync(LeadFilterDto? filter = null)
        {
            filter ??= new LeadFilterDto();

            var allLeads = await _unitOfWork.Repository<Lead>().GetAllAsync();
            var query = allLeads.AsQueryable();

            if (filter.Status.HasValue)
                query = query.Where(l => l.Status == filter.Status.Value);
            if (filter.Source.HasValue)
                query = query.Where(l => l.Source == filter.Source.Value);
            if (filter.AssignedToUserId.HasValue)
                query = query.Where(l => l.AssignedToUserId == filter.AssignedToUserId.Value);
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var term = filter.SearchTerm.ToLower();
                query = query.Where(l =>
                    l.ContactName.ToLower().Contains(term) ||
                    (l.CompanyName != null && l.CompanyName.ToLower().Contains(term)) ||
                    (l.Email != null && l.Email.ToLower().Contains(term)));
            }

            var totalCount = query.Count();
            var activities = await _unitOfWork.Repository<Activity>().GetAllAsync();

            var items = query
                .OrderByDescending(l => l.CreateAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(l => new LeadDto
                {
                    Id = l.Id,
                    ContactName = l.ContactName,
                    CompanyName = l.CompanyName,
                    Email = l.Email,
                    Phone = l.Phone,
                    Source = l.Source,
                    Status = l.Status,
                    EstimatedValue = l.EstimatedValue,
                    Notes = l.Notes,
                    AssignedToUserId = l.AssignedToUserId,
                    ConvertedCustomerId = l.ConvertedCustomerId,
                    ConvertedAt = l.ConvertedAt,
                    CreateAt = l.CreateAt,
                    ActivityCount = activities.Count(a => a.LeadId == l.Id)
                }).ToList();

            return new PaginatedResult<LeadDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<LeadDto?> GetByIdAsync(Guid id)
        {
            var l = await _unitOfWork.Repository<Lead>().GetByIdAsync(id);
            if (l == null) return null;

            var activityCount = await _unitOfWork.Repository<Activity>().CountAsync(a => a.LeadId == id);

            return new LeadDto
            {
                Id = l.Id,
                ContactName = l.ContactName,
                CompanyName = l.CompanyName,
                Email = l.Email,
                Phone = l.Phone,
                Source = l.Source,
                Status = l.Status,
                EstimatedValue = l.EstimatedValue,
                Notes = l.Notes,
                AssignedToUserId = l.AssignedToUserId,
                ConvertedCustomerId = l.ConvertedCustomerId,
                ConvertedAt = l.ConvertedAt,
                CreateAt = l.CreateAt,
                ActivityCount = activityCount
            };
        }

        /// <summary>
        /// Chuyển đổi Lead thành Customer.
        /// </summary>
        public async Task<Guid> ConvertToCustomerAsync(Guid leadId, ConvertLeadDto dto)
        {
            var lead = await _unitOfWork.Repository<Lead>().GetByIdAsync(leadId);
            if (lead == null) throw new ArgumentException("Lead không tồn tại.");
            if (lead.Status == LeadStatus.Converted)
                throw new InvalidOperationException("Lead này đã được chuyển đổi.");

            var customer = new Customer
            {
                Name = lead.CompanyName ?? lead.ContactName,
                Phone = lead.Phone ?? "",
                Email = lead.Email,
                Address = dto.Address,
                Type = dto.CustomerType,
                CreateAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId
            };

            _unitOfWork.Customers.Add(customer);

            lead.Status = LeadStatus.Converted;
            lead.ConvertedCustomerId = customer.Id;
            lead.ConvertedAt = DateTime.UtcNow;
            lead.UpdateAt = DateTime.UtcNow;
            lead.UpdatedBy = _currentUser.UserId;
            _unitOfWork.Repository<Lead>().Update(lead);

            if (!string.IsNullOrEmpty(lead.ContactName))
            {
                var contact = new Contact
                {
                    CustomerId = customer.Id,
                    FullName = lead.ContactName,
                    Email = lead.Email,
                    Phone = lead.Phone,
                    IsPrimary = true,
                    CreateAt = DateTime.UtcNow,
                    CreatedBy = _currentUser.UserId
                };
                _unitOfWork.Repository<Contact>().Add(contact);
            }

            await _unitOfWork.CompleteAsync();
            return customer.Id;
        }
    }
}
