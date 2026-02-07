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
    /// API quản lý điều chỉnh tồn kho.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StockAdjustmentsController : ControllerBase
    {
        private readonly StockAdjustmentService _stockAdjustmentService;

        public StockAdjustmentsController(StockAdjustmentService stockAdjustmentService)
        {
            _stockAdjustmentService = stockAdjustmentService;
        }

        /// <summary>
        /// Lấy danh sách điều chỉnh tồn kho với bộ lọc.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid? productId = null,
            [FromQuery] Guid? warehouseId = null,
            [FromQuery] StockAdjustmentType? type = null,
            [FromQuery] StockAdjustmentStatus? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var filter = new StockAdjustmentFilterDto
            {
                ProductId = productId,
                WarehouseId = warehouseId,
                Type = type,
                Status = status,
                FromDate = fromDate,
                ToDate = toDate,
                Page = page,
                PageSize = pageSize
            };

            var result = await _stockAdjustmentService.GetAllAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết điều chỉnh tồn kho.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var adjustment = await _stockAdjustmentService.GetByIdAsync(id);
            if (adjustment == null)
                return NotFound(new { message = "Phiếu điều chỉnh không tồn tại" });

            return Ok(adjustment);
        }

        /// <summary>
        /// Tạo phiếu điều chỉnh tồn kho mới.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStockAdjustmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var id = await _stockAdjustmentService.CreateAsync(dto);
                var adjustment = await _stockAdjustmentService.GetByIdAsync(id);
                return CreatedAtAction(nameof(GetById), new { id }, adjustment);
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
        /// Duyệt phiếu điều chỉnh tồn kho.
        /// </summary>
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            try
            {
                await _stockAdjustmentService.ApproveAsync(id);
                var adjustment = await _stockAdjustmentService.GetByIdAsync(id);
                return Ok(adjustment);
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

        /// <summary>
        /// Từ chối phiếu điều chỉnh tồn kho.
        /// </summary>
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] string? reason = null)
        {
            try
            {
                await _stockAdjustmentService.RejectAsync(id, reason);
                var adjustment = await _stockAdjustmentService.GetByIdAsync(id);
                return Ok(adjustment);
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
