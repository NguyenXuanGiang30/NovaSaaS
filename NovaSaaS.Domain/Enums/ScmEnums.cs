namespace NovaSaaS.Domain.Enums
{
    public enum VendorStatus
    {
        Active,
        Inactive,
        Blacklisted,
        Pending
    }

    public enum VendorRating
    {
        Excellent,
        Good,
        Average,
        Poor,
        Unrated
    }

    public enum PurchaseRequisitionStatus
    {
        Draft,
        Submitted,
        Approved,
        Rejected,
        Converted,
        Cancelled
    }

    public enum PurchaseOrderStatus
    {
        Draft,
        Sent,
        Confirmed,
        PartiallyReceived,
        Received,
        Cancelled,
        Closed
    }

    public enum GoodsReceiptStatus
    {
        Draft,
        Inspecting,
        Accepted,
        PartiallyAccepted,
        Rejected
    }

    public enum PurchaseInvoiceStatus
    {
        Draft,
        Received,
        Verified,
        Approved,
        Paid,
        PartiallyPaid,
        Disputed,
        Cancelled
    }

    public enum VendorPaymentMethod
    {
        BankTransfer,
        Cash,
        Check,
        CreditCard,
        Other
    }

    public enum VendorPaymentStatus
    {
        Pending,
        Processing,
        Completed,
        Failed,
        Cancelled
    }

    public enum ReturnToVendorStatus
    {
        Requested,
        Approved,
        Shipped,
        Received,
        Refunded,
        Rejected,
        Cancelled
    }

    public enum ReturnReason
    {
        Defective,
        WrongItem,
        Damaged,
        QualityIssue,
        ExcessStock,
        Other
    }
}
