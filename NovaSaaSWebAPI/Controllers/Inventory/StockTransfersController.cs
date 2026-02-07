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
    /// API quản lý chuyển kho.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StockTransfersController : ControllerBase
    {
        private readonly StockTransferService _stockTransferService;

        public StockTransfersController(StockTransferService stockTransferService)
        {
            _stockTransferService = stockTransferService;
        }

        /// <summary>
        /// Lấy danh sách chuyển kho với bộ lọc.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid? fromWarehouseId = null,
            [FromQuery] Guid? toWarehouseId = null,
            [FromQuery] StockTransferStatus? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var filter = new StockTransferFilterDto
            {
                FromWarehouseId = fromWarehouseId,
                ToWarehouseId = toWarehouseId,
                Status = status,
                FromDate = fromDate,
                ToDate = toDate,
                Page = page,
                PageSize = pageSize
            };

            var result = await _stockTransferService.GetAllAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết chuyển kho.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var transfer = await _stockTransferService.GetByIdAsync(id);
            if (transfer == null)
                return NotFound(new { message = "Phiếu chuyển kho không tồn tại" });

            return Ok(transfer);
        }

        /// <summary>
        /// Tạo phiếu chuyển kho mới.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStockTransferDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var id = await _stockTransferService.CreateAsync(dto);
                var transfer = await _stockTransferService.GetByIdAsync(id);
                return CreatedAtAction(nameof(GetById), new { id }, transfer);
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
        /// Duyệt phiếu chuyển kho.
        /// </summary>
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            try
            {
                await _stockTransferService.ApproveAsync(id);
                var transfer = await _stockTransferService.GetByIdAsync(id);
                return Ok(transfer);
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
        /// Hoàn thành phiếu chuyển kho.
        /// </summary>
        [HttpPost("{id}/complete")]
        public async Task<IActionResult> Complete(Guid id)
        {
            try
            {
                await _stockTransferService.CompleteAsync(id);
                var transfer = await _stockTransferService.GetByIdAsync(id);
                return Ok(transfer);
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
