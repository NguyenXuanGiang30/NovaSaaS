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
    public class CustomersController : ControllerBase
    {
        private readonly CustomerService _customerService;

        public CustomersController(CustomerService customerService)
        {
            _customerService = customerService;
        }

        /// <summary>
        /// Lấy danh sách khách hàng với phân trang và lọc.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] CustomerType? type,
            [FromQuery] CustomerRank? rank,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var filter = new CustomerFilterDto
            {
                Type = type,
                Rank = rank,
                SearchTerm = search,
                Page = page,
                PageSize = pageSize
            };

            var result = await _customerService.GetAllAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Tạo khách hàng mới.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
        {
            try
            {
                var id = await _customerService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id }, new { id });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message, error = "DuplicateError" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy chi tiết khách hàng theo ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null) return NotFound();
            return Ok(customer);
        }

        /// <summary>
        /// Cập nhật thông tin khách hàng.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerDto dto)
        {
            try
            {
                await _customerService.UpdateAsync(id, dto);
                var customer = await _customerService.GetByIdAsync(id);
                return Ok(customer);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message, error = "DuplicateError" });
            }
        }

        /// <summary>
        /// Xóa khách hàng.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _customerService.DeleteAsync(id);
                if (!result) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message, error = "HasRelatedData" });
            }
        }

        /// <summary>
        /// Lấy lịch sử mua hàng của khách hàng.
        /// </summary>
        [HttpGet("{id}/history")]
        public async Task<IActionResult> GetHistory(Guid id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null) return NotFound();

            var history = await _customerService.GetHistoryAsync(id);
            return Ok(history);
        }

        /// <summary>
        /// Tính toán lại rank cho khách hàng.
        /// </summary>
        [HttpPost("{id}/recalculate-rank")]
        public async Task<IActionResult> RecalculateRank(Guid id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null) return NotFound();

            await _customerService.RecalculateRankAsync(id);
            var updatedCustomer = await _customerService.GetByIdAsync(id);
            return Ok(updatedCustomer);
        }
    }
}
