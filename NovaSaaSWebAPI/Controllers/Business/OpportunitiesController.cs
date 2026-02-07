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
    public class OpportunitiesController : ControllerBase
    {
        private readonly OpportunityService _opportunityService;

        public OpportunitiesController(OpportunityService opportunityService)
        {
            _opportunityService = opportunityService;
        }

        /// <summary>
        /// Lấy danh sách cơ hội với phân trang và lọc.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] OpportunityStage? stage,
            [FromQuery] Guid? customerId,
            [FromQuery] Guid? assignedToUserId,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var filter = new OpportunityFilterDto
            {
                Stage = stage,
                CustomerId = customerId,
                AssignedToUserId = assignedToUserId,
                FromDate = fromDate,
                ToDate = toDate,
                SearchTerm = search,
                Page = page,
                PageSize = pageSize
            };

            var result = await _opportunityService.GetAllAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết cơ hội theo ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var opportunity = await _opportunityService.GetByIdAsync(id);
            if (opportunity == null) return NotFound();
            return Ok(opportunity);
        }

        /// <summary>
        /// Tạo cơ hội mới.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOpportunityDto dto)
        {
            try
            {
                var id = await _opportunityService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id }, new { id });
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
        /// Cập nhật thông tin cơ hội.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOpportunityDto dto)
        {
            try
            {
                await _opportunityService.UpdateAsync(id, dto);
                var opportunity = await _opportunityService.GetByIdAsync(id);
                return Ok(opportunity);
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
        /// Xóa cơ hội.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _opportunityService.DeleteAsync(id);
                if (!result) return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy thống kê pipeline.
        /// </summary>
        [HttpGet("pipeline/summary")]
        public async Task<IActionResult> GetPipelineSummary()
        {
            var result = await _opportunityService.GetPipelineSummaryAsync();
            return Ok(result);
        }
    }
}
