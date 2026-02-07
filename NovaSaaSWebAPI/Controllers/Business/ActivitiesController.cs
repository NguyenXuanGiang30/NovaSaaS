using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaSaaS.Application.DTOs.Business;
using NovaSaaS.Application.Services.Business;
using NovaSaaS.Domain.Enums;

namespace NovaSaaSWebAPI.Controllers.Business
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ActivitiesController : ControllerBase
    {
        private readonly ActivityService _activityService;

        public ActivitiesController(ActivityService activityService)
        {
            _activityService = activityService;
        }

        /// <summary>
        /// Lấy danh sách hoạt động với phân trang và lọc.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] ActivityType? type,
            [FromQuery] ActivityStatus? status,
            [FromQuery] Guid? customerId,
            [FromQuery] Guid? leadId,
            [FromQuery] Guid? opportunityId,
            [FromQuery] Guid? assignedToUserId,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var filter = new ActivityFilterDto
            {
                Type = type,
                Status = status,
                CustomerId = customerId,
                LeadId = leadId,
                OpportunityId = opportunityId,
                AssignedToUserId = assignedToUserId,
                FromDate = fromDate,
                ToDate = toDate,
                SearchTerm = search,
                Page = page,
                PageSize = pageSize
            };

            var result = await _activityService.GetAllAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết hoạt động theo ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var activity = await _activityService.GetByIdAsync(id);
            if (activity == null) return NotFound();
            return Ok(activity);
        }

        /// <summary>
        /// Tạo hoạt động mới.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateActivityDto dto)
        {
            try
            {
                var id = await _activityService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id }, new { id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật thông tin hoạt động.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateActivityDto dto)
        {
            try
            {
                await _activityService.UpdateAsync(id, dto);
                var activity = await _activityService.GetByIdAsync(id);
                return Ok(activity);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Xóa hoạt động.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _activityService.DeleteAsync(id);
                if (!result) return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
