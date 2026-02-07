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
    public class OpportunityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public OpportunityService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Guid> CreateAsync(CreateOpportunityDto dto)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(dto.CustomerId);
            if (customer == null) throw new ArgumentException("Khách hàng không tồn tại.");

            var opp = new Opportunity
            {
                Name = dto.Name,
                CustomerId = dto.CustomerId,
                Stage = OpportunityStage.Qualification,
                Value = dto.Value,
                Probability = dto.Probability,
                ExpectedCloseDate = dto.ExpectedCloseDate,
                AssignedToUserId = dto.AssignedToUserId,
                Notes = dto.Notes,
                CreateAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId
            };

            _unitOfWork.Repository<Opportunity>().Add(opp);
            await _unitOfWork.CompleteAsync();
            return opp.Id;
        }

        public async Task UpdateAsync(Guid id, UpdateOpportunityDto dto)
        {
            var opp = await _unitOfWork.Repository<Opportunity>().GetByIdAsync(id);
            if (opp == null) throw new ArgumentException("Cơ hội không tồn tại.");

            opp.Name = dto.Name;
            opp.Stage = dto.Stage;
            opp.Value = dto.Value;
            opp.Probability = dto.Probability;
            opp.ExpectedCloseDate = dto.ExpectedCloseDate;
            opp.AssignedToUserId = dto.AssignedToUserId;
            opp.LostReason = dto.LostReason;
            opp.Notes = dto.Notes;
            opp.UpdateAt = DateTime.UtcNow;
            opp.UpdatedBy = _currentUser.UserId;

            if (dto.Stage == OpportunityStage.ClosedWon || dto.Stage == OpportunityStage.ClosedLost)
            {
                opp.ActualCloseDate = DateTime.UtcNow;
            }

            _unitOfWork.Repository<Opportunity>().Update(opp);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var opp = await _unitOfWork.Repository<Opportunity>().GetByIdAsync(id);
            if (opp == null) return false;

            _unitOfWork.Repository<Opportunity>().SoftDelete(opp);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<PaginatedResult<OpportunityDto>> GetAllAsync(OpportunityFilterDto? filter = null)
        {
            filter ??= new OpportunityFilterDto();

            var allOpps = await _unitOfWork.Repository<Opportunity>().GetAllAsync();
            var query = allOpps.AsQueryable();

            if (filter.Stage.HasValue)
                query = query.Where(o => o.Stage == filter.Stage.Value);
            if (filter.CustomerId.HasValue)
                query = query.Where(o => o.CustomerId == filter.CustomerId.Value);
            if (filter.AssignedToUserId.HasValue)
                query = query.Where(o => o.AssignedToUserId == filter.AssignedToUserId.Value);
            if (filter.FromDate.HasValue)
                query = query.Where(o => o.CreateAt >= filter.FromDate.Value);
            if (filter.ToDate.HasValue)
                query = query.Where(o => o.CreateAt <= filter.ToDate.Value);
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var term = filter.SearchTerm.ToLower();
                query = query.Where(o => o.Name.ToLower().Contains(term));
            }

            var totalCount = query.Count();
            var customers = await _unitOfWork.Customers.GetAllAsync();
            var activities = await _unitOfWork.Repository<Activity>().GetAllAsync();
            var quotations = await _unitOfWork.Repository<Quotation>().GetAllAsync();

            var customerDict = customers.ToDictionary(c => c.Id, c => c.Name);

            var items = query
                .OrderByDescending(o => o.CreateAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(o => new OpportunityDto
                {
                    Id = o.Id,
                    Name = o.Name,
                    CustomerId = o.CustomerId,
                    CustomerName = customerDict.ContainsKey(o.CustomerId) ? customerDict[o.CustomerId] : "",
                    Stage = o.Stage,
                    Value = o.Value,
                    Probability = o.Probability,
                    ExpectedCloseDate = o.ExpectedCloseDate,
                    ActualCloseDate = o.ActualCloseDate,
                    AssignedToUserId = o.AssignedToUserId,
                    LostReason = o.LostReason,
                    Notes = o.Notes,
                    ConvertedOrderId = o.ConvertedOrderId,
                    CreateAt = o.CreateAt,
                    ActivityCount = activities.Count(a => a.OpportunityId == o.Id),
                    QuotationCount = quotations.Count(q => q.OpportunityId == o.Id)
                }).ToList();

            return new PaginatedResult<OpportunityDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<OpportunityDto?> GetByIdAsync(Guid id)
        {
            var o = await _unitOfWork.Repository<Opportunity>().GetByIdAsync(id);
            if (o == null) return null;

            var customer = await _unitOfWork.Customers.GetByIdAsync(o.CustomerId);
            var activityCount = await _unitOfWork.Repository<Activity>().CountAsync(a => a.OpportunityId == id);
            var quotationCount = await _unitOfWork.Repository<Quotation>().CountAsync(q => q.OpportunityId == id);

            return new OpportunityDto
            {
                Id = o.Id,
                Name = o.Name,
                CustomerId = o.CustomerId,
                CustomerName = customer?.Name ?? "",
                Stage = o.Stage,
                Value = o.Value,
                Probability = o.Probability,
                ExpectedCloseDate = o.ExpectedCloseDate,
                ActualCloseDate = o.ActualCloseDate,
                AssignedToUserId = o.AssignedToUserId,
                LostReason = o.LostReason,
                Notes = o.Notes,
                ConvertedOrderId = o.ConvertedOrderId,
                CreateAt = o.CreateAt,
                ActivityCount = activityCount,
                QuotationCount = quotationCount
            };
        }

        /// <summary>
        /// Lấy thống kê pipeline.
        /// </summary>
        public async Task<Dictionary<string, object>> GetPipelineSummaryAsync()
        {
            var allOpps = await _unitOfWork.Repository<Opportunity>().GetAllAsync();

            var byStage = allOpps.GroupBy(o => o.Stage)
                .Select(g => new
                {
                    Stage = g.Key.ToString(),
                    Count = g.Count(),
                    TotalValue = g.Sum(o => o.Value),
                    WeightedValue = g.Sum(o => o.Value * o.Probability / 100)
                }).ToList();

            return new Dictionary<string, object>
            {
                ["totalOpportunities"] = allOpps.Count(),
                ["totalValue"] = allOpps.Sum(o => o.Value),
                ["weightedValue"] = allOpps.Sum(o => o.Value * o.Probability / 100),
                ["byStage"] = byStage
            };
        }
    }
}
