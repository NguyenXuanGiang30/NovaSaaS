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
    /// API quản lý số serial.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SerialNumbersController : ControllerBase
    {
        private readonly SerialNumberService _serialNumberService;

        public SerialNumbersController(SerialNumberService serialNumberService)
        {
            _serialNumberService = serialNumberService;
        }

        /// <summary>
        /// Lấy danh sách số serial với bộ lọc.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid? productId = null,
            [FromQuery] Guid? warehouseId = null,
            [FromQuery] SerialNumberStatus? status = null,
            [FromQuery] Guid? lotNumberId = null,
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var filter = new SerialNumberFilterDto
            {
                ProductId = productId,
                WarehouseId = warehouseId,
                Status = status,
                LotNumberId = lotNumberId,
                SearchTerm = search,
                Page = page,
                PageSize = pageSize
            };

            var result = await _serialNumberService.GetAllAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết số serial.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var serial = await _serialNumberService.GetByIdAsync(id);
            if (serial == null)
                return NotFound(new { message = "Số serial không tồn tại" });

            return Ok(serial);
        }

        /// <summary>
        /// Tạo số serial mới.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSerialNumberDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var id = await _serialNumberService.CreateAsync(dto);
                var serial = await _serialNumberService.GetByIdAsync(id);
                return CreatedAtAction(nameof(GetById), new { id }, serial);
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
        /// Tạo nhiều số serial cùng lúc.
        /// </summary>
        [HttpPost("batch")]
        public async Task<IActionResult> CreateBatch([FromBody] CreateSerialNumberBatchDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var ids = await _serialNumberService.CreateBatchAsync(dto);
                return Ok(new { ids, count = ids.Count });
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
