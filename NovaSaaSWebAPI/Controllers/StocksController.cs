using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaSaaS.Application.DTOs.Inventory;
using NovaSaaS.Application.Services.Inventory;
using System;
using System.Threading.Tasks;

namespace NovaSaaS.WebAPI.Controllers
{
    /// <summary>
    /// API quản lý tồn kho và biến động kho.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StocksController : ControllerBase
    {
        private readonly StockService _stockService;

        public StocksController(StockService stockService)
        {
            _stockService = stockService;
        }

        /// <summary>
        /// Lấy tồn kho của sản phẩm tại tất cả các kho.
        /// </summary>
        [HttpGet("product/{productId:guid}")]
        public async Task<IActionResult> GetStockByProduct(Guid productId)
        {
            var result = await _stockService.GetStockByProductAsync(productId);
            return Ok(result);
        }

        /// <summary>
        /// Lấy số lượng tồn tại một kho cụ thể.
        /// </summary>
        [HttpGet("product/{productId:guid}/warehouse/{warehouseId:guid}")]
        public async Task<IActionResult> GetStockQuantity(Guid productId, Guid warehouseId)
        {
            var quantity = await _stockService.GetStockQuantityAsync(productId, warehouseId);
            return Ok(new { productId, warehouseId, quantity });
        }

        /// <summary>
        /// Nhập/Xuất kho.
        /// </summary>
        [HttpPost("adjust")]
        public async Task<IActionResult> AdjustStock([FromBody] QuickStockAdjustmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _stockService.AdjustStockAsync(dto);
                return Ok(result);
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
        /// Chuyển kho (Transfer).
        /// Sử dụng Transaction đảm bảo atomic.
        /// </summary>
        [HttpPost("transfer")]
        public async Task<IActionResult> TransferStock([FromBody] QuickStockTransferDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var (outMovement, inMovement) = await _stockService.TransferStockAsync(dto);
                return Ok(new
                {
                    message = "Chuyển kho thành công",
                    outMovement,
                    inMovement
                });
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
        /// Lấy lịch sử biến động kho.
        /// </summary>
        [HttpGet("movements")]
        public async Task<IActionResult> GetMovementHistory(
            [FromQuery] Guid? productId = null,
            [FromQuery] Guid? warehouseId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int take = 50)
        {
            var result = await _stockService.GetMovementHistoryAsync(
                productId, warehouseId, fromDate, toDate, take);
            return Ok(result);
        }
    }
}
