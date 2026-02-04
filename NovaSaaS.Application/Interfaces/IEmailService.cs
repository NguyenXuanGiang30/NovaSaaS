using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NovaSaaS.Application.Interfaces
{
    /// <summary>
    /// Email message model.
    /// </summary>
    public class EmailMessage
    {
        /// <summary>
        /// Địa chỉ email người nhận.
        /// </summary>
        public string To { get; set; } = string.Empty;

        /// <summary>
        /// Tên người nhận (optional).
        /// </summary>
        public string? ToName { get; set; }

        /// <summary>
        /// Tiêu đề email.
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Nội dung HTML.
        /// </summary>
        public string HtmlBody { get; set; } = string.Empty;

        /// <summary>
        /// Nội dung text thuần (fallback).
        /// </summary>
        public string? PlainTextBody { get; set; }

        /// <summary>
        /// CC email addresses.
        /// </summary>
        public List<string>? Cc { get; set; }

        /// <summary>
        /// BCC email addresses.
        /// </summary>
        public List<string>? Bcc { get; set; }

        /// <summary>
        /// Custom reply-to address.
        /// </summary>
        public string? ReplyTo { get; set; }

        /// <summary>
        /// Attachments (file paths).
        /// </summary>
        public List<string>? Attachments { get; set; }
    }

    /// <summary>
    /// Kết quả gửi email.
    /// </summary>
    public class EmailResult
    {
        public bool Success { get; set; }
        public string? MessageId { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// IEmailService - Interface cho Email Service.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Gửi email đơn.
        /// </summary>
        Task<EmailResult> SendEmailAsync(EmailMessage message);

        /// <summary>
        /// Gửi email hàng loạt.
        /// </summary>
        Task<List<EmailResult>> SendBulkEmailAsync(List<EmailMessage> messages);

        /// <summary>
        /// Gửi email sử dụng template.
        /// </summary>
        /// <param name="templateName">Tên template (welcome, invoice_reminder, password_reset, etc.)</param>
        /// <param name="to">Email người nhận</param>
        /// <param name="subject">Tiêu đề</param>
        /// <param name="templateData">Data để render template</param>
        Task<EmailResult> SendTemplatedEmailAsync(
            string templateName, 
            string to, 
            string subject, 
            Dictionary<string, string> templateData);

        /// <summary>
        /// Gửi email nhắc thanh toán Invoice.
        /// </summary>
        Task<EmailResult> SendInvoiceReminderAsync(
            string customerEmail,
            string customerName,
            string invoiceNumber,
            decimal amount,
            DateTime dueDate,
            string paymentLink);

        /// <summary>
        /// Gửi email chào mừng tenant mới.
        /// </summary>
        Task<EmailResult> SendWelcomeEmailAsync(
            string email,
            string tenantName,
            string adminName,
            string loginUrl);

        /// <summary>
        /// Gửi email reset password.
        /// </summary>
        Task<EmailResult> SendPasswordResetEmailAsync(
            string email,
            string userName,
            string resetLink,
            int expirationMinutes);

        /// <summary>
        /// Gửi email cảnh báo subscription sắp hết hạn.
        /// </summary>
        Task<EmailResult> SendSubscriptionExpiryWarningAsync(
            string email,
            string tenantName,
            DateTime expiryDate,
            int daysRemaining,
            string renewalLink);
    }
}
