using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaSaaS.Application.DTOs.Business;
using NovaSaaS.Application.DTOs.Inventory;
using NovaSaaS.Application.Services.Inventory;
using NovaSaaS.Domain.Enums;
using System;
using System.Threading.Tasks;

namespace NovaSaaSWebAPI.Controllers.Inventory
{
    /// <summary>
    /// API quản lý kiểm kê tồn kho.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InventoryCountsController : ControllerBase
    {
        private readonly InventoryCountService _inventoryCountService;

        public InventoryCountsController(InventoryCountService inventoryCountService)
        {
            _inventoryCountService = inventoryCountService;
        }

        /// <summary>
        /// Lấy danh sách kiểm kê với bộ lọc.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid? warehouseId = null,
            [FromQuery] InventoryCountStatus? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var filter = new InventoryCountFilterDto
            {
                WarehouseId = warehouseId,
                Status = status,
                FromDate = fromDate,
                ToDate = toDate,
                Page = page,
                PageSize = pageSize
            };

            var result = await _inventoryCountService.GetAllAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết kiểm kê.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var count = await _inventoryCountService.GetByIdAsync(id);
            if (count == null)
                return NotFound(new { message = "Phiếu kiểm kê không tồn tại" });

            return Ok(count);
        }

        /// <summary>
        /// Tạo phiếu kiểm kê mới.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateInventoryCountDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var id = await _inventoryCountService.CreateAsync(dto);
                var count = await _inventoryCountService.GetByIdAsync(id);
                return CreatedAtAction(nameof(GetById), new { id }, count);
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
        /// Hoàn thành phiếu kiểm kê.
        /// </summary>
        [HttpPost("{id}/complete")]
        public async Task<IActionResult> Complete(Guid id)
        {
            try
            {
                await _inventoryCountService.CompleteAsync(id);
                var count = await _inventoryCountService.GetByIdAsync(id);
                return Ok(count);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
