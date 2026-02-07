using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaSaaS.Application.DTOs.Inventory;
using NovaSaaS.Application.Services.Inventory;
using System;
using System.Threading.Tasks;

namespace NovaSaaSWebAPI.Controllers.Inventory
{
    /// <summary>
    /// API quản lý vị trí trong kho.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LocationsController : ControllerBase
    {
        private readonly LocationService _locationService;

        public LocationsController(LocationService locationService)
        {
            _locationService = locationService;
        }

        /// <summary>
        /// Lấy danh sách vị trí theo kho.
        /// </summary>
        [HttpGet("warehouse/{warehouseId}")]
        public async Task<IActionResult> GetByWarehouseId(Guid warehouseId)
        {
            var result = await _locationService.GetByWarehouseIdAsync(warehouseId);
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết vị trí.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var location = await _locationService.GetByIdAsync(id);
            if (location == null)
                return NotFound(new { message = "Vị trí không tồn tại" });

            return Ok(location);
        }

        /// <summary>
        /// Tạo vị trí mới.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLocationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var id = await _locationService.CreateAsync(dto);
                var location = await _locationService.GetByIdAsync(id);
                return CreatedAtAction(nameof(GetById), new { id }, location);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật vị trí.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLocationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _locationService.UpdateAsync(id, dto);
                var location = await _locationService.GetByIdAsync(id);
                if (location == null)
                    return NotFound(new { message = "Vị trí không tồn tại" });

                return Ok(location);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Xóa vị trí.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _locationService.DeleteAsync(id);
            if (!success)
                return NotFound(new { message = "Vị trí không tồn tại" });

            return NoContent();
        }
    }
}
