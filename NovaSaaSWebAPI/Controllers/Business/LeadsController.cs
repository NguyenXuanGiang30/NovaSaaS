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
    public class LeadsController : ControllerBase
    {
        private readonly LeadService _leadService;

        public LeadsController(LeadService leadService)
        {
            _leadService = leadService;
        }

        /// <summary>
        /// Lấy danh sách lead với phân trang và lọc.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] LeadStatus? status,
            [FromQuery] LeadSource? source,
            [FromQuery] Guid? assignedToUserId,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var filter = new LeadFilterDto
            {
                Status = status,
                Source = source,
                AssignedToUserId = assignedToUserId,
                SearchTerm = search,
                Page = page,
                PageSize = pageSize
            };

            var result = await _leadService.GetAllAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết lead theo ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var lead = await _leadService.GetByIdAsync(id);
            if (lead == null) return NotFound();
            return Ok(lead);
        }

        /// <summary>
        /// Tạo lead mới.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLeadDto dto)
        {
            try
            {
                var id = await _leadService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id }, new { id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật thông tin lead.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeadDto dto)
        {
            try
            {
                await _leadService.UpdateAsync(id, dto);
                var lead = await _leadService.GetByIdAsync(id);
                return Ok(lead);
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
        /// Xóa lead.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _leadService.DeleteAsync(id);
                if (!result) return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Chuyển đổi lead thành khách hàng.
        /// </summary>
        [HttpPost("{id}/convert")]
        public async Task<IActionResult> ConvertToCustomer(Guid id, [FromBody] ConvertLeadDto dto)
        {
            try
            {
                var customerId = await _leadService.ConvertToCustomerAsync(id, dto);
                return Ok(new { customerId });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
