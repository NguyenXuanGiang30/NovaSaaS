using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaSaaS.Application.Interfaces;

namespace NovaSaaSWebAPI.Controllers
{
    /// <summary>
    /// EmailController - API endpoints để test email service.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailController> _logger;

        public EmailController(
            IEmailService emailService,
            ILogger<EmailController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Test gửi email đơn giản.
        /// </summary>
        [HttpPost("test")]
        [AllowAnonymous]
        public async Task<IActionResult> SendTestEmail([FromBody] TestEmailRequest request)
        {
            if (string.IsNullOrEmpty(request.To))
            {
                return BadRequest(new { error = "Email address is required." });
            }

            var message = new EmailMessage
            {
                To = request.To,
                Subject = request.Subject ?? "🧪 Test Email từ NovaSaaS",
                HtmlBody = $@"
                    <h1>Test Email</h1>
                    <p>Đây là email test từ NovaSaaS.</p>
                    <p><strong>Thời gian gửi:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
                    <p>Chúc bạn có một ngày tốt lành! 🎉</p>
                "
            };

            var result = await _emailService.SendEmailAsync(message);

            if (result.Success)
            {
                return Ok(new
                {
                    success = true,
                    message = "Email sent successfully!",
                    messageId = result.MessageId
                });
            }

            return BadRequest(new
            {
                success = false,
                error = result.ErrorMessage
            });
        }

        /// <summary>
        /// Test gửi email nhắc thanh toán.
        /// </summary>
        [HttpPost("test-invoice-reminder")]
        [AllowAnonymous]
        public async Task<IActionResult> TestInvoiceReminder([FromBody] TestInvoiceReminderRequest request)
        {
            var result = await _emailService.SendInvoiceReminderAsync(
                request.To ?? "test@example.com",
                request.CustomerName ?? "Khách hàng Test",
                request.InvoiceNumber ?? "INV-2024-001",
                request.Amount ?? 1500000,
                request.DueDate ?? DateTime.Now.AddDays(-3),
                request.PaymentLink ?? "https://novasaas.vn/pay/test"
            );

            if (result.Success)
            {
                return Ok(new { success = true, message = "Invoice reminder email sent!" });
            }

            return BadRequest(new { success = false, error = result.ErrorMessage });
        }

        /// <summary>
        /// Test gửi email chào mừng.
        /// </summary>
        [HttpPost("test-welcome")]
        [AllowAnonymous]
        public async Task<IActionResult> TestWelcomeEmail([FromBody] TestWelcomeRequest request)
        {
            var result = await _emailService.SendWelcomeEmailAsync(
                request.To ?? "test@example.com",
                request.TenantName ?? "Công ty Test",
                request.AdminName ?? "Admin",
                request.LoginUrl ?? "https://novasaas.vn/login"
            );

            if (result.Success)
            {
                return Ok(new { success = true, message = "Welcome email sent!" });
            }

            return BadRequest(new { success = false, error = result.ErrorMessage });
        }
    }

    public class TestEmailRequest
    {
        public string To { get; set; } = string.Empty;
        public string? Subject { get; set; }
    }

    public class TestInvoiceReminderRequest
    {
        public string? To { get; set; }
        public string? CustomerName { get; set; }
        public string? InvoiceNumber { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? DueDate { get; set; }
        public string? PaymentLink { get; set; }
    }

    public class TestWelcomeRequest
    {
        public string? To { get; set; }
        public string? TenantName { get; set; }
        public string? AdminName { get; set; }
        public string? LoginUrl { get; set; }
    }
}
