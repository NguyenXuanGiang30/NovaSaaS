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
    public class CampaignService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public CampaignService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Guid> CreateAsync(CreateCampaignDto dto)
        {
            var campaign = new Campaign
            {
                Name = dto.Name,
                Description = dto.Description,
                Type = dto.Type,
                Status = CampaignStatus.Draft,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Budget = dto.Budget,
                CouponId = dto.CouponId,
                TargetCustomerGroupId = dto.TargetCustomerGroupId,
                InternalNotes = dto.InternalNotes,
                CreateAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId
            };

            _unitOfWork.Repository<Campaign>().Add(campaign);
            await _unitOfWork.CompleteAsync();
            return campaign.Id;
        }

        public async Task UpdateAsync(Guid id, UpdateCampaignDto dto)
        {
            var campaign = await _unitOfWork.Repository<Campaign>().GetByIdAsync(id);
            if (campaign == null) throw new ArgumentException("Chiến dịch không tồn tại.");

            campaign.Name = dto.Name;
            campaign.Description = dto.Description;
            campaign.Type = dto.Type;
            campaign.Status = dto.Status;
            campaign.StartDate = dto.StartDate;
            campaign.EndDate = dto.EndDate;
            campaign.Budget = dto.Budget;
            campaign.ActualCost = dto.ActualCost;
            campaign.Revenue = dto.Revenue;
            campaign.CouponId = dto.CouponId;
            campaign.TargetCustomerGroupId = dto.TargetCustomerGroupId;
            campaign.InternalNotes = dto.InternalNotes;
            campaign.UpdateAt = DateTime.UtcNow;
            campaign.UpdatedBy = _currentUser.UserId;

            _unitOfWork.Repository<Campaign>().Update(campaign);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var campaign = await _unitOfWork.Repository<Campaign>().GetByIdAsync(id);
            if (campaign == null) return false;

            _unitOfWork.Repository<Campaign>().SoftDelete(campaign);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<PaginatedResult<CampaignDto>> GetAllAsync(CampaignFilterDto? filter = null)
        {
            filter ??= new CampaignFilterDto();

            var allCampaigns = await _unitOfWork.Repository<Campaign>().GetAllAsync();
            var query = allCampaigns.AsQueryable();

            if (filter.Status.HasValue)
                query = query.Where(c => c.Status == filter.Status.Value);
            if (filter.Type.HasValue)
                query = query.Where(c => c.Type == filter.Type.Value);
            if (filter.FromDate.HasValue)
                query = query.Where(c => c.StartDate >= filter.FromDate.Value);
            if (filter.ToDate.HasValue)
                query = query.Where(c => c.StartDate <= filter.ToDate.Value);
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var term = filter.SearchTerm.ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(term));
            }

            var totalCount = query.Count();
            var groups = await _unitOfWork.Repository<CustomerGroup>().GetAllAsync();
            var groupDict = groups.ToDictionary(g => g.Id, g => g.Name);

            var items = query
                .OrderByDescending(c => c.CreateAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(c => new CampaignDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Type = c.Type,
                    Status = c.Status,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Budget = c.Budget,
                    ActualCost = c.ActualCost,
                    Revenue = c.Revenue,
                    LeadsGenerated = c.LeadsGenerated,
                    OrdersGenerated = c.OrdersGenerated,
                    CouponId = c.CouponId,
                    TargetCustomerGroupId = c.TargetCustomerGroupId,
                    TargetCustomerGroupName = c.TargetCustomerGroupId.HasValue && groupDict.ContainsKey(c.TargetCustomerGroupId.Value)
                        ? groupDict[c.TargetCustomerGroupId.Value] : null,
                    InternalNotes = c.InternalNotes,
                    CreateAt = c.CreateAt
                }).ToList();

            return new PaginatedResult<CampaignDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<CampaignDto?> GetByIdAsync(Guid id)
        {
            var c = await _unitOfWork.Repository<Campaign>().GetByIdAsync(id);
            if (c == null) return null;

            string? groupName = null;
            if (c.TargetCustomerGroupId.HasValue)
            {
                var group = await _unitOfWork.Repository<CustomerGroup>().GetByIdAsync(c.TargetCustomerGroupId.Value);
                groupName = group?.Name;
            }

            return new CampaignDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Type = c.Type,
                Status = c.Status,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Budget = c.Budget,
                ActualCost = c.ActualCost,
                Revenue = c.Revenue,
                LeadsGenerated = c.LeadsGenerated,
                OrdersGenerated = c.OrdersGenerated,
                CouponId = c.CouponId,
                TargetCustomerGroupId = c.TargetCustomerGroupId,
                TargetCustomerGroupName = groupName,
                InternalNotes = c.InternalNotes,
                CreateAt = c.CreateAt
            };
        }
    }
}
