namespace NovaSaaS.Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 0,      // Chờ xác nhận
        Confirmed = 1,    // Đã xác nhận (đã trừ kho)
        Shipping = 2,     // Đang giao
        Completed = 3,    // Hoàn thành
        Cancelled = 4     // Hủy (hoàn kho)
    }

    public enum InvoiceStatus
    {
        Draft = 0,        // Nháp
        Unpaid = 1,       // Chưa thanh toán
        Paid = 2,         // Đã thanh toán
        Refunded = 3,     // Hoàn tiền
        Cancelled = 4     // Hủy bỏ
    }
    
    public enum PaymentMethod
    {
        Cash = 0,
        BankTransfer = 1,
        CreditCard = 2,
        COD = 3
    }
}
