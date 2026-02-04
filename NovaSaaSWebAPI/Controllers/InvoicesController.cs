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
    public class InvoicesController : ControllerBase
    {
        private readonly InvoiceService _invoiceService;

        public InvoicesController(InvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        /// <summary>
        /// Lấy danh sách hóa đơn với phân trang và lọc.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] InvoiceStatus? status,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var filter = new InvoiceFilterDto
            {
                Status = status,
                FromDate = fromDate,
                ToDate = toDate,
                Page = page,
                PageSize = pageSize
            };

            var result = await _invoiceService.GetAllAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết hóa đơn theo ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var invoice = await _invoiceService.GetByIdAsync(id);
            if (invoice == null) return NotFound();
            return Ok(invoice);
        }

        /// <summary>
        /// Lấy hóa đơn theo Order ID.
        /// </summary>
        [HttpGet("by-order/{orderId}")]
        public async Task<IActionResult> GetByOrderId(Guid orderId)
        {
            var invoice = await _invoiceService.GetByOrderIdAsync(orderId);
            if (invoice == null) return NotFound();
            return Ok(invoice);
        }

        /// <summary>
        /// Đánh dấu hóa đơn đã thanh toán.
        /// </summary>
        [HttpPut("{id}/pay")]
        public async Task<IActionResult> MarkAsPaid(Guid id, [FromBody] MarkAsPaidRequest request)
        {
            try
            {
                var invoice = await _invoiceService.MarkAsPaidAsync(id, request);
                return Ok(invoice);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message, error = "BusinessLogicError" });
            }
        }

        /// <summary>
        /// Hoàn tiền hóa đơn.
        /// </summary>
        [HttpPut("{id}/refund")]
        public async Task<IActionResult> Refund(Guid id, [FromBody] RefundRequest request)
        {
            try
            {
                var invoice = await _invoiceService.RefundAsync(id, request);
                return Ok(invoice);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message, error = "BusinessLogicError" });
            }
        }

        /// <summary>
        /// Hủy hóa đơn.
        /// </summary>
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelRequest request)
        {
            try
            {
                var invoice = await _invoiceService.CancelAsync(id, request.Reason);
                return Ok(invoice);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message, error = "BusinessLogicError" });
            }
        }

        /// <summary>
        /// Lấy thống kê doanh thu.
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var stats = await _invoiceService.GetStatisticsAsync(fromDate, toDate);
            return Ok(stats);
        }
    }

    public class CancelRequest
    {
        public string Reason { get; set; } = string.Empty;
    }
}
