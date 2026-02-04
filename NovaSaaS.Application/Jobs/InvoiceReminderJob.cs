using NovaSaaS.Application.Interfaces;
using NovaSaaS.Domain.Enums;
using NovaSaaS.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace NovaSaaS.Application.Jobs
{
    /// <summary>
    /// InvoiceReminderJob - Job gửi email nhắc thanh toán cho invoice quá hạn.
    /// Chạy hàng ngày vào 09:00 UTC.
    /// </summary>
    public class InvoiceReminderJob
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<InvoiceReminderJob> _logger;

        public InvoiceReminderJob(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<InvoiceReminderJob> logger)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Thực thi job nhắc thanh toán.
        /// </summary>
        public async Task ExecuteAsync()
        {
            _logger.LogInformation("📧 Starting InvoiceReminderJob...");

            var now = DateTime.UtcNow;
            var remindersSent = 0;
            var errors = 0;

            try
            {
                // Lấy danh sách Invoice chưa thanh toán và đã quá hạn
                var overdueInvoices = await _unitOfWork.Invoices.GetAllAsync(
                    i => i.Status != InvoiceStatus.Paid &&
                         i.Status != InvoiceStatus.Cancelled &&
                         i.DueDate.HasValue &&
                         i.DueDate.Value < now);

                foreach (var invoice in overdueInvoices)
                {
                    try
                    {
                        // Lấy thông tin Order và Customer
                        var order = await _unitOfWork.Orders.GetByIdAsync(invoice.OrderId);
                        if (order == null) continue;

                        var customer = await _unitOfWork.Customers.GetByIdAsync(order.CustomerId);
                        if (customer == null || string.IsNullOrEmpty(customer.Email)) continue;

                        // Tạo payment link
                        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://novasaas.vn";
                        var paymentLink = $"{baseUrl}/payments/checkout?invoiceId={invoice.Id}";

                        // Gửi email nhắc nhở
                        var result = await _emailService.SendInvoiceReminderAsync(
                            customer.Email,
                            customer.Name,
                            invoice.InvoiceNumber,
                            invoice.TotalAmount,
                            invoice.DueDate ?? now,
                            paymentLink);

                        if (result.Success)
                        {
                            remindersSent++;
                            _logger.LogInformation(
                                "📧 Invoice reminder sent to {Email} for invoice #{InvoiceNumber}",
                                customer.Email, invoice.InvoiceNumber);
                        }
                        else
                        {
                            errors++;
                            _logger.LogWarning(
                                "⚠️ Failed to send reminder to {Email}: {Error}",
                                customer.Email, result.ErrorMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        errors++;
                        _logger.LogError(ex, "Error processing invoice reminder for {InvoiceId}", invoice.Id);
                    }
                }

                _logger.LogInformation(
                    "✅ InvoiceReminderJob completed. Sent: {Sent}, Errors: {Errors}",
                    remindersSent, errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ InvoiceReminderJob failed");
                throw;
            }
        }
    }
}
