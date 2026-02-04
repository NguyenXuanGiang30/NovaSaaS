using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using NovaSaaS.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NovaSaaS.Infrastructure.Services.Email
{
    /// <summary>
    /// SmtpEmailService - Email service sử dụng SMTP với MailKit.
    /// Hỗ trợ: Gmail, Outlook, SendGrid, Mailgun, AWS SES, etc.
    /// </summary>
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpEmailService> _logger;

        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPassword;
        private readonly string _senderEmail;
        private readonly string _senderName;
        private readonly bool _useSsl;

        public SmtpEmailService(
            IConfiguration configuration,
            ILogger<SmtpEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Load SMTP configuration
            _smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            _smtpPort = _configuration.GetValue<int>("Email:SmtpPort", 587);
            _smtpUser = _configuration["Email:SmtpUser"] ?? "";
            _smtpPassword = _configuration["Email:SmtpPassword"] ?? "";
            _senderEmail = _configuration["Email:SenderEmail"] ?? "noreply@novasaas.vn";
            _senderName = _configuration["Email:SenderName"] ?? "NovaSaaS";
            _useSsl = _configuration.GetValue<bool>("Email:UseSsl", true);
        }

        /// <inheritdoc />
        public async Task<EmailResult> SendEmailAsync(EmailMessage message)
        {
            try
            {
                var mimeMessage = CreateMimeMessage(message);

                using var client = new SmtpClient();
                
                // Connect to SMTP server
                var secureSocketOptions = _useSsl 
                    ? SecureSocketOptions.StartTls 
                    : SecureSocketOptions.Auto;
                    
                await client.ConnectAsync(_smtpHost, _smtpPort, secureSocketOptions);

                // Authenticate if credentials provided
                if (!string.IsNullOrEmpty(_smtpUser) && !string.IsNullOrEmpty(_smtpPassword))
                {
                    await client.AuthenticateAsync(_smtpUser, _smtpPassword);
                }

                // Send email
                var messageId = await client.SendAsync(mimeMessage);
                await client.DisconnectAsync(true);

                _logger.LogInformation("📧 Email sent successfully to {To}: {Subject}", 
                    message.To, message.Subject);

                return new EmailResult
                {
                    Success = true,
                    MessageId = messageId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to send email to {To}: {Subject}", 
                    message.To, message.Subject);

                return new EmailResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <inheritdoc />
        public async Task<List<EmailResult>> SendBulkEmailAsync(List<EmailMessage> messages)
        {
            var results = new List<EmailResult>();

            foreach (var message in messages)
            {
                var result = await SendEmailAsync(message);
                results.Add(result);

                // Add small delay between emails to avoid rate limiting
                await Task.Delay(100);
            }

            return results;
        }

        /// <inheritdoc />
        public async Task<EmailResult> SendTemplatedEmailAsync(
            string templateName,
            string to,
            string subject,
            Dictionary<string, string> templateData)
        {
            var htmlBody = GetEmailTemplate(templateName);

            // Replace placeholders with actual data
            foreach (var kvp in templateData)
            {
                htmlBody = htmlBody.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
            }

            var message = new EmailMessage
            {
                To = to,
                Subject = subject,
                HtmlBody = htmlBody
            };

            return await SendEmailAsync(message);
        }

        /// <inheritdoc />
        public async Task<EmailResult> SendInvoiceReminderAsync(
            string customerEmail,
            string customerName,
            string invoiceNumber,
            decimal amount,
            DateTime dueDate,
            string paymentLink)
        {
            var templateData = new Dictionary<string, string>
            {
                { "CustomerName", customerName },
                { "InvoiceNumber", invoiceNumber },
                { "Amount", amount.ToString("N0") + " VND" },
                { "DueDate", dueDate.ToString("dd/MM/yyyy") },
                { "PaymentLink", paymentLink },
                { "Year", DateTime.Now.Year.ToString() }
            };

            return await SendTemplatedEmailAsync(
                "invoice_reminder",
                customerEmail,
                $"[NovaSaaS] Nhắc thanh toán hóa đơn #{invoiceNumber}",
                templateData);
        }

        /// <inheritdoc />
        public async Task<EmailResult> SendWelcomeEmailAsync(
            string email,
            string tenantName,
            string adminName,
            string loginUrl)
        {
            var templateData = new Dictionary<string, string>
            {
                { "TenantName", tenantName },
                { "AdminName", adminName },
                { "LoginUrl", loginUrl },
                { "Year", DateTime.Now.Year.ToString() }
            };

            return await SendTemplatedEmailAsync(
                "welcome",
                email,
                $"[NovaSaaS] Chào mừng {tenantName} đến với NovaSaaS!",
                templateData);
        }

        /// <inheritdoc />
        public async Task<EmailResult> SendPasswordResetEmailAsync(
            string email,
            string userName,
            string resetLink,
            int expirationMinutes)
        {
            var templateData = new Dictionary<string, string>
            {
                { "UserName", userName },
                { "ResetLink", resetLink },
                { "ExpirationMinutes", expirationMinutes.ToString() },
                { "Year", DateTime.Now.Year.ToString() }
            };

            return await SendTemplatedEmailAsync(
                "password_reset",
                email,
                "[NovaSaaS] Yêu cầu đặt lại mật khẩu",
                templateData);
        }

        /// <inheritdoc />
        public async Task<EmailResult> SendSubscriptionExpiryWarningAsync(
            string email,
            string tenantName,
            DateTime expiryDate,
            int daysRemaining,
            string renewalLink)
        {
            var templateData = new Dictionary<string, string>
            {
                { "TenantName", tenantName },
                { "ExpiryDate", expiryDate.ToString("dd/MM/yyyy") },
                { "DaysRemaining", daysRemaining.ToString() },
                { "RenewalLink", renewalLink },
                { "Year", DateTime.Now.Year.ToString() }
            };

            return await SendTemplatedEmailAsync(
                "subscription_expiry",
                email,
                $"[NovaSaaS] Gói dịch vụ sắp hết hạn trong {daysRemaining} ngày",
                templateData);
        }

        #region Private Helpers

        private MimeMessage CreateMimeMessage(EmailMessage message)
        {
            var mimeMessage = new MimeMessage();

            // From
            mimeMessage.From.Add(new MailboxAddress(_senderName, _senderEmail));

            // To
            if (!string.IsNullOrEmpty(message.ToName))
            {
                mimeMessage.To.Add(new MailboxAddress(message.ToName, message.To));
            }
            else
            {
                mimeMessage.To.Add(MailboxAddress.Parse(message.To));
            }

            // CC
            if (message.Cc?.Count > 0)
            {
                foreach (var cc in message.Cc)
                {
                    mimeMessage.Cc.Add(MailboxAddress.Parse(cc));
                }
            }

            // BCC
            if (message.Bcc?.Count > 0)
            {
                foreach (var bcc in message.Bcc)
                {
                    mimeMessage.Bcc.Add(MailboxAddress.Parse(bcc));
                }
            }

            // Reply-To
            if (!string.IsNullOrEmpty(message.ReplyTo))
            {
                mimeMessage.ReplyTo.Add(MailboxAddress.Parse(message.ReplyTo));
            }

            // Subject
            mimeMessage.Subject = message.Subject;

            // Body
            var builder = new BodyBuilder();
            builder.HtmlBody = message.HtmlBody;

            if (!string.IsNullOrEmpty(message.PlainTextBody))
            {
                builder.TextBody = message.PlainTextBody;
            }

            // Attachments
            if (message.Attachments?.Count > 0)
            {
                foreach (var filePath in message.Attachments)
                {
                    if (File.Exists(filePath))
                    {
                        builder.Attachments.Add(filePath);
                    }
                }
            }

            mimeMessage.Body = builder.ToMessageBody();

            return mimeMessage;
        }

        private string GetEmailTemplate(string templateName)
        {
            // Base templates - trong production nên load từ file hoặc database
            return templateName switch
            {
                "invoice_reminder" => GetInvoiceReminderTemplate(),
                "welcome" => GetWelcomeTemplate(),
                "password_reset" => GetPasswordResetTemplate(),
                "subscription_expiry" => GetSubscriptionExpiryTemplate(),
                _ => GetDefaultTemplate()
            };
        }

        private string GetInvoiceReminderTemplate()
        {
            return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body { font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }
        .content { background: #f8f9fa; padding: 30px; }
        .button { display: inline-block; padding: 12px 30px; background: #667eea; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }
        .footer { text-align: center; padding: 20px; color: #888; font-size: 12px; }
        .highlight { color: #667eea; font-weight: bold; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📋 Nhắc Thanh Toán</h1>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{{CustomerName}}</strong>,</p>
            <p>Đây là thông báo nhắc nhở về hóa đơn chưa thanh toán:</p>
            <table style='width: 100%; border-collapse: collapse; margin: 20px 0;'>
                <tr><td style='padding: 10px; border-bottom: 1px solid #ddd;'>Số hóa đơn:</td><td style='padding: 10px; border-bottom: 1px solid #ddd;' class='highlight'>{{InvoiceNumber}}</td></tr>
                <tr><td style='padding: 10px; border-bottom: 1px solid #ddd;'>Số tiền:</td><td style='padding: 10px; border-bottom: 1px solid #ddd;' class='highlight'>{{Amount}}</td></tr>
                <tr><td style='padding: 10px; border-bottom: 1px solid #ddd;'>Hạn thanh toán:</td><td style='padding: 10px; border-bottom: 1px solid #ddd;' class='highlight'>{{DueDate}}</td></tr>
            </table>
            <p style='text-align: center;'>
                <a href='{{PaymentLink}}' class='button'>💳 Thanh Toán Ngay</a>
            </p>
            <p>Nếu bạn đã thanh toán, vui lòng bỏ qua email này.</p>
        </div>
        <div class='footer'>
            <p>© {{Year}} NovaSaaS. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetWelcomeTemplate()
        {
            return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body { font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }
        .content { background: #f8f9fa; padding: 30px; }
        .button { display: inline-block; padding: 12px 30px; background: #11998e; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }
        .footer { text-align: center; padding: 20px; color: #888; font-size: 12px; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Chào Mừng Đến Với NovaSaaS!</h1>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{{AdminName}}</strong>,</p>
            <p>Chúc mừng bạn đã đăng ký thành công <strong>{{TenantName}}</strong> trên nền tảng NovaSaaS!</p>
            <p>Bây giờ bạn có thể:</p>
            <ul>
                <li>✅ Quản lý sản phẩm và kho hàng</li>
                <li>✅ Tạo đơn hàng và hóa đơn</li>
                <li>✅ Sử dụng AI Assistant thông minh</li>
                <li>✅ Xem báo cáo và phân tích</li>
            </ul>
            <p style='text-align: center;'>
                <a href='{{LoginUrl}}' class='button'>🚀 Đăng Nhập Ngay</a>
            </p>
        </div>
        <div class='footer'>
            <p>© {{Year}} NovaSaaS. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetPasswordResetTemplate()
        {
            return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body { font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }
        .content { background: #f8f9fa; padding: 30px; }
        .button { display: inline-block; padding: 12px 30px; background: #f5576c; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }
        .footer { text-align: center; padding: 20px; color: #888; font-size: 12px; }
        .warning { background: #fff3cd; padding: 15px; border-radius: 5px; border-left: 4px solid #ffc107; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔐 Đặt Lại Mật Khẩu</h1>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{{UserName}}</strong>,</p>
            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
            <p style='text-align: center;'>
                <a href='{{ResetLink}}' class='button'>🔑 Đặt Lại Mật Khẩu</a>
            </p>
            <div class='warning'>
                <strong>⚠️ Lưu ý:</strong> Link này sẽ hết hạn sau <strong>{{ExpirationMinutes}} phút</strong>.
            </div>
            <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
        </div>
        <div class='footer'>
            <p>© {{Year}} NovaSaaS. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetSubscriptionExpiryTemplate()
        {
            return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body { font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: linear-gradient(135deg, #ff9a9e 0%, #fecfef 100%); color: #333; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }
        .content { background: #f8f9fa; padding: 30px; }
        .button { display: inline-block; padding: 12px 30px; background: #ff6b6b; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }
        .footer { text-align: center; padding: 20px; color: #888; font-size: 12px; }
        .alert { background: #f8d7da; padding: 15px; border-radius: 5px; border-left: 4px solid #dc3545; text-align: center; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>⏰ Gói Dịch Vụ Sắp Hết Hạn</h1>
        </div>
        <div class='content'>
            <p>Xin chào,</p>
            <p>Gói dịch vụ của <strong>{{TenantName}}</strong> sẽ hết hạn vào ngày <strong>{{ExpiryDate}}</strong>.</p>
            <div class='alert'>
                <h2>⚠️ Còn {{DaysRemaining}} ngày</h2>
            </div>
            <p>Để tiếp tục sử dụng dịch vụ không gián đoạn, vui lòng gia hạn ngay:</p>
            <p style='text-align: center;'>
                <a href='{{RenewalLink}}' class='button'>🔄 Gia Hạn Ngay</a>
            </p>
        </div>
        <div class='footer'>
            <p>© {{Year}} NovaSaaS. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetDefaultTemplate()
        {
            return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body { font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
    </style>
</head>
<body>
    <div class='container'>
        <p>Email content here</p>
    </div>
</body>
</html>";
        }

        #endregion
    }
}
