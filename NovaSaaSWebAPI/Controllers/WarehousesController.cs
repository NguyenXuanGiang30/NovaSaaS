using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaSaaS.Application.DTOs.Inventory;
using NovaSaaS.Application.Services.Inventory;
using System;
using System.Threading.Tasks;

namespace NovaSaaS.WebAPI.Controllers
{
    /// <summary>
    /// API quản lý kho hàng.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WarehousesController : ControllerBase
    {
        private readonly WarehouseService _warehouseService;

        public WarehousesController(WarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        /// <summary>
        /// Lấy danh sách kho hàng.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _warehouseService.GetAllWarehousesAsync();
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết kho hàng.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var warehouse = await _warehouseService.GetWarehouseByIdAsync(id);
            if (warehouse == null)
                return NotFound(new { message = "Kho hàng không tồn tại" });

            return Ok(warehouse);
        }

        /// <summary>
        /// Tạo kho hàng mới.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateWarehouseDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _warehouseService.CreateWarehouseAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật kho hàng.
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWarehouseDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _warehouseService.UpdateWarehouseAsync(id, dto);
                if (result == null)
                    return NotFound(new { message = "Kho hàng không tồn tại" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Xóa kho hàng (soft delete).
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _warehouseService.DeleteAsync(id);
            if (!success)
                return NotFound(new { message = "Kho hàng không tồn tại" });

            return NoContent();
        }
    }
}
