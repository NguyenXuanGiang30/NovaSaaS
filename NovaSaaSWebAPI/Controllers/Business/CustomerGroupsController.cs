using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaSaaS.Application.DTOs.Business;
using NovaSaaS.Application.Services.Business;

namespace NovaSaaSWebAPI.Controllers.Business
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomerGroupsController : ControllerBase
    {
        private readonly CustomerGroupService _customerGroupService;

        public CustomerGroupsController(CustomerGroupService customerGroupService)
        {
            _customerGroupService = customerGroupService;
        }

        /// <summary>
        /// Lấy danh sách tất cả nhóm khách hàng.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _customerGroupService.GetAllAsync();
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết nhóm khách hàng theo ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var group = await _customerGroupService.GetByIdAsync(id);
            if (group == null) return NotFound();
            return Ok(group);
        }

        /// <summary>
        /// Tạo nhóm khách hàng mới.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerGroupDto dto)
        {
            try
            {
                var id = await _customerGroupService.CreateAsync(dto);
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
        /// Cập nhật thông tin nhóm khách hàng.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerGroupDto dto)
        {
            try
            {
                await _customerGroupService.UpdateAsync(id, dto);
                var group = await _customerGroupService.GetByIdAsync(id);
                return Ok(group);
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
        /// Xóa nhóm khách hàng.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _customerGroupService.DeleteAsync(id);
                if (!result) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message, error = "HasRelatedData" });
            }
        }
    }
}
