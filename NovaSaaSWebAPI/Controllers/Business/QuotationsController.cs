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
    public class QuotationsController : ControllerBase
    {
        private readonly QuotationService _quotationService;

        public QuotationsController(QuotationService quotationService)
        {
            _quotationService = quotationService;
        }

        /// <summary>
        /// Lấy danh sách báo giá với phân trang và lọc.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] QuotationStatus? status,
            [FromQuery] Guid? customerId,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var filter = new QuotationFilterDto
            {
                Status = status,
                CustomerId = customerId,
                FromDate = fromDate,
                ToDate = toDate,
                SearchTerm = search,
                Page = page,
                PageSize = pageSize
            };

            var result = await _quotationService.GetAllAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết báo giá theo ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var quotation = await _quotationService.GetByIdAsync(id);
            if (quotation == null) return NotFound();
            return Ok(quotation);
        }

        /// <summary>
        /// Tạo báo giá mới.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateQuotationDto dto)
        {
            try
            {
                var id = await _quotationService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id }, new { id });
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

        /// <summary>
        /// Xóa báo giá.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _quotationService.DeleteAsync(id);
                if (!result) return NotFound();
                return NoContent();
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

        /// <summary>
        /// Cập nhật trạng thái báo giá.
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateQuotationStatusDto dto)
        {
            try
            {
                await _quotationService.UpdateStatusAsync(id, dto);
                var quotation = await _quotationService.GetByIdAsync(id);
                return Ok(quotation);
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
    }
}
