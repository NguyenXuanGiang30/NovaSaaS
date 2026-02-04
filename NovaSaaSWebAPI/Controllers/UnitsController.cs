using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaSaaS.Application.DTOs.Inventory;
using NovaSaaS.Application.Services.Inventory;
using System;
using System.Threading.Tasks;

namespace NovaSaaS.WebAPI.Controllers
{
    /// <summary>
    /// API quản lý đơn vị tính.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UnitsController : ControllerBase
    {
        private readonly UnitService _unitService;

        public UnitsController(UnitService unitService)
        {
            _unitService = unitService;
        }

        /// <summary>
        /// Lấy tất cả đơn vị tính.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _unitService.GetAllUnitsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Lấy đơn vị tính theo ID.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var unit = await _unitService.GetByIdAsync(id);
            if (unit == null)
                return NotFound(new { message = "Đơn vị tính không tồn tại" });

            return Ok(unit);
        }

        /// <summary>
        /// Tạo đơn vị tính mới.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUnitDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _unitService.CreateUnitAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật đơn vị tính.
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUnitDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _unitService.UpdateUnitAsync(id, dto);
                if (result == null)
                    return NotFound(new { message = "Đơn vị tính không tồn tại" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Xóa đơn vị tính (soft delete).
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _unitService.DeleteAsync(id);
            if (!success)
                return NotFound(new { message = "Đơn vị tính không tồn tại" });

            return NoContent();
        }
    }
}
