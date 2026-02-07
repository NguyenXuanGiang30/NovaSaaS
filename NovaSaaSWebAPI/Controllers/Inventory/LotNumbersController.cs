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
    /// API quản lý số lô hàng.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LotNumbersController : ControllerBase
    {
        private readonly LotNumberService _lotNumberService;

        public LotNumbersController(LotNumberService lotNumberService)
        {
            _lotNumberService = lotNumberService;
        }

        /// <summary>
        /// Lấy danh sách số lô với bộ lọc.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid? productId = null,
            [FromQuery] Guid? warehouseId = null,
            [FromQuery] LotStatus? status = null,
            [FromQuery] bool? isExpired = null,
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var filter = new LotNumberFilterDto
            {
                ProductId = productId,
                WarehouseId = warehouseId,
                Status = status,
                IsExpired = isExpired,
                SearchTerm = search,
                Page = page,
                PageSize = pageSize
            };

            var result = await _lotNumberService.GetAllAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết số lô.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var lot = await _lotNumberService.GetByIdAsync(id);
            if (lot == null)
                return NotFound(new { message = "Số lô không tồn tại" });

            return Ok(lot);
        }

        /// <summary>
        /// Tạo số lô mới.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLotNumberDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var id = await _lotNumberService.CreateAsync(dto);
                var lot = await _lotNumberService.GetByIdAsync(id);
                return CreatedAtAction(nameof(GetById), new { id }, lot);
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
    }
}
