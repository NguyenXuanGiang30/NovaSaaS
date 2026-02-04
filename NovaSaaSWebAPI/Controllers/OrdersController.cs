using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaSaaS.Application.DTOs.Business;
using NovaSaaS.Application.Services.Business;
using NovaSaaS.Domain.Enums;

namespace NovaSaaSWebAPI.Controllers.Business
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrdersController(OrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Lấy danh sách đơn hàng với phân trang và lọc.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid? customerId,
            [FromQuery] OrderStatus? status,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var filter = new OrderFilterDto
            {
                CustomerId = customerId,
                Status = status,
                FromDate = fromDate,
                ToDate = toDate,
                Page = page,
                PageSize = pageSize
            };

            var result = await _orderService.GetAllAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Tạo đơn hàng mới (ACID Transaction).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            try
            {
                var order = await _orderService.CreateOrderAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message, error = "BusinessLogicError" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message, error = "ValidationError" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi xử lý đơn hàng: " + ex.Message });
            }
        }

        /// <summary>
        /// Lấy chi tiết đơn hàng theo ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();
            return Ok(order);
        }

        /// <summary>
        /// Cập nhật trạng thái đơn hàng.
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusDto dto)
        {
            try
            {
                var order = await _orderService.UpdateStatusAsync(id, dto);
                return Ok(order);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message, error = "InvalidStatusTransition" });
            }
        }

        /// <summary>
        /// Hủy đơn hàng và hoàn kho (ACID Transaction).
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderRequest? request = null)
        {
            try
            {
                request ??= new CancelOrderRequest { Reason = "Hủy bởi người dùng", RestoreStock = true };
                var order = await _orderService.CancelOrderAsync(id, request);
                return Ok(order);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message, error = "BusinessLogicError" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hủy đơn hàng: " + ex.Message });
            }
        }
    }
}
