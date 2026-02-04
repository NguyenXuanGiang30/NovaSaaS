using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaSaaS.Application.Interfaces;

namespace NovaSaaSWebAPI.Controllers
{
    /// <summary>
    /// PaymentsController - API endpoints cho Payment Gateway.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IPaymentService paymentService,
            ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        /// <summary>
        /// Tạo checkout session để thanh toán Invoice.
        /// </summary>
        [HttpPost("checkout")]
        [Authorize]
        public async Task<IActionResult> CreateCheckout([FromBody] CreateCheckoutRequest request)
        {
            if (request.InvoiceId == Guid.Empty)
            {
                return BadRequest(new { error = "InvoiceId is required." });
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var successUrl = request.SuccessUrl ?? $"{baseUrl}/payment/success";
            var cancelUrl = request.CancelUrl ?? $"{baseUrl}/payment/cancel";

            var result = await _paymentService.CreateCheckoutSessionAsync(
                request.InvoiceId, 
                successUrl, 
                cancelUrl);

            if (!result.Success)
            {
                return BadRequest(new { error = result.ErrorMessage });
            }

            return Ok(new
            {
                checkoutUrl = result.CheckoutUrl,
                sessionId = result.SessionId
            });
        }

        /// <summary>
        /// Webhook endpoint cho Stripe.
        /// Stripe sẽ gọi endpoint này khi có sự kiện thanh toán.
        /// </summary>
        [HttpPost("webhook/stripe")]
        [AllowAnonymous]
        public async Task<IActionResult> StripeWebhook()
        {
            try
            {
                // Đọc request body
                using var reader = new StreamReader(Request.Body);
                var payload = await reader.ReadToEndAsync();

                // Lấy Stripe signature header
                var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();
                if (string.IsNullOrEmpty(signature))
                {
                    _logger.LogWarning("Stripe webhook missing signature header");
                    return BadRequest("Missing signature header.");
                }

                var success = await _paymentService.HandleWebhookAsync(payload, signature);

                if (!success)
                {
                    return BadRequest("Webhook processing failed.");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Stripe webhook");
                return StatusCode(500, "Webhook error.");
            }
        }

        /// <summary>
        /// Kiểm tra trạng thái thanh toán.
        /// </summary>
        [HttpGet("status/{sessionId}")]
        [Authorize]
        public async Task<IActionResult> GetPaymentStatus(string sessionId)
        {
            var status = await _paymentService.GetPaymentStatusAsync(sessionId);

            if (status == null)
            {
                return NotFound(new { error = "Session not found." });
            }

            return Ok(new
            {
                status = status.EventType,
                transactionId = status.TransactionId,
                amount = status.Amount,
                currency = status.Currency
            });
        }

        /// <summary>
        /// Trang thành công sau thanh toán (cho testing).
        /// </summary>
        [HttpGet("success")]
        [AllowAnonymous]
        public IActionResult PaymentSuccess([FromQuery] string session_id)
        {
            return Ok(new
            {
                message = "Payment successful!",
                sessionId = session_id
            });
        }

        /// <summary>
        /// Trang hủy thanh toán (cho testing).
        /// </summary>
        [HttpGet("cancel")]
        [AllowAnonymous]
        public IActionResult PaymentCancel()
        {
            return Ok(new { message = "Payment cancelled." });
        }
    }

    /// <summary>
    /// Request model cho tạo checkout.
    /// </summary>
    public class CreateCheckoutRequest
    {
        /// <summary>
        /// ID của Invoice cần thanh toán.
        /// </summary>
        public Guid InvoiceId { get; set; }

        /// <summary>
        /// URL redirect khi thanh toán thành công (optional).
        /// </summary>
        public string? SuccessUrl { get; set; }

        /// <summary>
        /// URL redirect khi hủy thanh toán (optional).
        /// </summary>
        public string? CancelUrl { get; set; }
    }
}
