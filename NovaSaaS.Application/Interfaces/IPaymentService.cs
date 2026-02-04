using System;
using System.Threading.Tasks;

namespace NovaSaaS.Application.Interfaces
{
    /// <summary>
    /// Kết quả tạo checkout session.
    /// </summary>
    public class CheckoutResult
    {
        /// <summary>
        /// URL để redirect khách hàng đến trang thanh toán.
        /// </summary>
        public string CheckoutUrl { get; set; } = string.Empty;

        /// <summary>
        /// ID của session/transaction (để tracking).
        /// </summary>
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// Có thành công không.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Thông báo lỗi (nếu có).
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Thông tin webhook từ cổng thanh toán.
    /// </summary>
    public class PaymentWebhookEvent
    {
        /// <summary>
        /// Loại sự kiện (checkout.session.completed, payment_intent.succeeded, etc.).
        /// </summary>
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// ID transaction từ gateway.
        /// </summary>
        public string TransactionId { get; set; } = string.Empty;

        /// <summary>
        /// ID session (nếu có).
        /// </summary>
        public string? SessionId { get; set; }

        /// <summary>
        /// Metadata (chứa InvoiceId, TenantId, etc.).
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new();

        /// <summary>
        /// Số tiền thanh toán.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Đơn vị tiền tệ.
        /// </summary>
        public string Currency { get; set; } = string.Empty;
    }

    /// <summary>
    /// IPaymentService - Interface cho Payment Gateway integration.
    /// </summary>
    public interface IPaymentService
    {
        /// <summary>
        /// Tạo checkout session cho Invoice.
        /// </summary>
        /// <param name="invoiceId">ID của Invoice cần thanh toán</param>
        /// <param name="successUrl">URL redirect khi thanh toán thành công</param>
        /// <param name="cancelUrl">URL redirect khi hủy thanh toán</param>
        /// <returns>Checkout result với URL redirect</returns>
        Task<CheckoutResult> CreateCheckoutSessionAsync(Guid invoiceId, string successUrl, string cancelUrl);

        /// <summary>
        /// Xử lý webhook từ cổng thanh toán.
        /// </summary>
        /// <param name="payload">Request body từ webhook</param>
        /// <param name="signature">Signature header để verify</param>
        /// <returns>True nếu xử lý thành công</returns>
        Task<bool> HandleWebhookAsync(string payload, string signature);

        /// <summary>
        /// Lấy thông tin trạng thái thanh toán.
        /// </summary>
        /// <param name="sessionId">ID của checkout session</param>
        Task<PaymentWebhookEvent?> GetPaymentStatusAsync(string sessionId);
    }
}
