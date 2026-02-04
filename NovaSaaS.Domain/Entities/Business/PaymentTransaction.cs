using NovaSaaS.Domain.Entities.Common;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Business
{
    /// <summary>
    /// Trạng thái giao dịch thanh toán.
    /// </summary>
    public enum PaymentTransactionStatus
    {
        /// <summary>
        /// Đang chờ xử lý.
        /// </summary>
        Pending,

        /// <summary>
        /// Đã hoàn thành.
        /// </summary>
        Completed,

        /// <summary>
        /// Thất bại.
        /// </summary>
        Failed,

        /// <summary>
        /// Đã hoàn tiền.
        /// </summary>
        Refunded,

        /// <summary>
        /// Đã hủy.
        /// </summary>
        Cancelled
    }

    /// <summary>
    /// Cổng thanh toán được sử dụng.
    /// </summary>
    public enum PaymentGateway
    {
        Stripe,
        VNPay,
        Manual
    }

    /// <summary>
    /// PaymentTransaction - Ghi log giao dịch thanh toán.
    /// Mỗi lần thanh toán Invoice sẽ tạo một transaction.
    /// </summary>
    public class PaymentTransaction : BaseEntity
    {
        /// <summary>
        /// ID của Invoice liên quan.
        /// </summary>
        public Guid InvoiceId { get; set; }

        /// <summary>
        /// Mã giao dịch từ cổng thanh toán (PaymentIntent ID, TransactionId, etc.).
        /// </summary>
        [MaxLength(255)]
        public string? GatewayTransactionId { get; set; }

        /// <summary>
        /// Cổng thanh toán được sử dụng.
        /// </summary>
        public PaymentGateway Gateway { get; set; }

        /// <summary>
        /// Số tiền thanh toán.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Đơn vị tiền tệ (VND, USD, etc.).
        /// </summary>
        [MaxLength(10)]
        public string Currency { get; set; } = "VND";

        /// <summary>
        /// Trạng thái giao dịch.
        /// </summary>
        public PaymentTransactionStatus Status { get; set; } = PaymentTransactionStatus.Pending;

        /// <summary>
        /// Thông tin lỗi (nếu có).
        /// </summary>
        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Metadata từ cổng thanh toán (JSON).
        /// </summary>
        public string? GatewayMetadata { get; set; }

        /// <summary>
        /// Thời điểm hoàn thành giao dịch.
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        // Navigation
        public virtual Invoice? Invoice { get; set; }
    }
}
