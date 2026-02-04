using NovaSaaS.Domain.Enums;
using System;

namespace NovaSaaS.Application.DTOs.Business
{
    public class InvoiceDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public InvoiceStatus Status { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public DateTime CreateAt { get; set; }
        public string StatusName => Status.ToString();
    }

    public class InvoiceFilterDto
    {
        public InvoiceStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class MarkAsPaidRequest
    {
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.BankTransfer;
        public string? TransactionReference { get; set; }
    }

    public class RefundRequest
    {
        public string Reason { get; set; } = string.Empty;
        public bool RestoreStock { get; set; } = true;
    }
}
