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
    public class ActivityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public ActivityService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Guid> CreateAsync(CreateActivityDto dto)
        {
            var activity = new Activity
            {
                Title = dto.Title,
                Type = dto.Type,
                Status = ActivityStatus.Planned,
                Description = dto.Description,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                ReminderAt = dto.ReminderAt,
                AssignedToUserId = dto.AssignedToUserId ?? Guid.Parse(_currentUser.UserId ?? Guid.Empty.ToString()),
                CustomerId = dto.CustomerId,
                LeadId = dto.LeadId,
                OpportunityId = dto.OpportunityId,
                CreateAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId
            };

            _unitOfWork.Repository<Activity>().Add(activity);
            await _unitOfWork.CompleteAsync();
            return activity.Id;
        }

        public async Task UpdateAsync(Guid id, UpdateActivityDto dto)
        {
            var activity = await _unitOfWork.Repository<Activity>().GetByIdAsync(id);
            if (activity == null) throw new ArgumentException("Hoạt động không tồn tại.");

            activity.Title = dto.Title;
            activity.Type = dto.Type;
            activity.Status = dto.Status;
            activity.Description = dto.Description;
            activity.StartTime = dto.StartTime;
            activity.EndTime = dto.EndTime;
            activity.ReminderAt = dto.ReminderAt;
            activity.AssignedToUserId = dto.AssignedToUserId;
            activity.Outcome = dto.Outcome;
            activity.UpdateAt = DateTime.UtcNow;
            activity.UpdatedBy = _currentUser.UserId;

            _unitOfWork.Repository<Activity>().Update(activity);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var activity = await _unitOfWork.Repository<Activity>().GetByIdAsync(id);
            if (activity == null) return false;

            _unitOfWork.Repository<Activity>().Remove(activity);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<PaginatedResult<ActivityDto>> GetAllAsync(ActivityFilterDto? filter = null)
        {
            filter ??= new ActivityFilterDto();

            var allActivities = await _unitOfWork.Repository<Activity>().GetAllAsync();
            var query = allActivities.AsQueryable();

            if (filter.Type.HasValue)
                query = query.Where(a => a.Type == filter.Type.Value);
            if (filter.Status.HasValue)
                query = query.Where(a => a.Status == filter.Status.Value);
            if (filter.CustomerId.HasValue)
                query = query.Where(a => a.CustomerId == filter.CustomerId.Value);
            if (filter.LeadId.HasValue)
                query = query.Where(a => a.LeadId == filter.LeadId.Value);
            if (filter.OpportunityId.HasValue)
                query = query.Where(a => a.OpportunityId == filter.OpportunityId.Value);
            if (filter.AssignedToUserId.HasValue)
                query = query.Where(a => a.AssignedToUserId == filter.AssignedToUserId.Value);
            if (filter.FromDate.HasValue)
                query = query.Where(a => a.StartTime >= filter.FromDate.Value);
            if (filter.ToDate.HasValue)
                query = query.Where(a => a.StartTime <= filter.ToDate.Value);
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var term = filter.SearchTerm.ToLower();
                query = query.Where(a => a.Title.ToLower().Contains(term));
            }

            var totalCount = query.Count();

            var customers = await _unitOfWork.Customers.GetAllAsync();
            var leads = await _unitOfWork.Repository<Lead>().GetAllAsync();
            var opps = await _unitOfWork.Repository<Opportunity>().GetAllAsync();

            var customerDict = customers.ToDictionary(c => c.Id, c => c.Name);
            var leadDict = leads.ToDictionary(l => l.Id, l => l.ContactName);
            var oppDict = opps.ToDictionary(o => o.Id, o => o.Name);

            var items = query
                .OrderByDescending(a => a.StartTime ?? a.CreateAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(a => new ActivityDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Type = a.Type,
                    Status = a.Status,
                    Description = a.Description,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    ReminderAt = a.ReminderAt,
                    AssignedToUserId = a.AssignedToUserId,
                    CustomerId = a.CustomerId,
                    CustomerName = a.CustomerId.HasValue && customerDict.ContainsKey(a.CustomerId.Value) ? customerDict[a.CustomerId.Value] : null,
                    LeadId = a.LeadId,
                    LeadContactName = a.LeadId.HasValue && leadDict.ContainsKey(a.LeadId.Value) ? leadDict[a.LeadId.Value] : null,
                    OpportunityId = a.OpportunityId,
                    OpportunityName = a.OpportunityId.HasValue && oppDict.ContainsKey(a.OpportunityId.Value) ? oppDict[a.OpportunityId.Value] : null,
                    Outcome = a.Outcome,
                    CreateAt = a.CreateAt
                }).ToList();

            return new PaginatedResult<ActivityDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<ActivityDto?> GetByIdAsync(Guid id)
        {
            var a = await _unitOfWork.Repository<Activity>().GetByIdAsync(id);
            if (a == null) return null;

            string? customerName = null, leadName = null, oppName = null;

            if (a.CustomerId.HasValue)
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(a.CustomerId.Value);
                customerName = customer?.Name;
            }
            if (a.LeadId.HasValue)
            {
                var lead = await _unitOfWork.Repository<Lead>().GetByIdAsync(a.LeadId.Value);
                leadName = lead?.ContactName;
            }
            if (a.OpportunityId.HasValue)
            {
                var opp = await _unitOfWork.Repository<Opportunity>().GetByIdAsync(a.OpportunityId.Value);
                oppName = opp?.Name;
            }

            return new ActivityDto
            {
                Id = a.Id,
                Title = a.Title,
                Type = a.Type,
                Status = a.Status,
                Description = a.Description,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                ReminderAt = a.ReminderAt,
                AssignedToUserId = a.AssignedToUserId,
                CustomerId = a.CustomerId,
                CustomerName = customerName,
                LeadId = a.LeadId,
                LeadContactName = leadName,
                OpportunityId = a.OpportunityId,
                OpportunityName = oppName,
                Outcome = a.Outcome,
                CreateAt = a.CreateAt
            };
        }
    }
}
