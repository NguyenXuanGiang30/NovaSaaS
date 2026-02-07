namespace NovaSaaS.Domain.Enums
{
    /// <summary>
    /// Trạng thái Lead.
    /// </summary>
    public enum LeadStatus
    {
        New = 0,            // Mới tạo
        Contacted = 1,      // Đã liên hệ
        Qualified = 2,      // Đủ điều kiện
        Unqualified = 3,    // Không đủ điều kiện
        Converted = 4,      // Đã chuyển đổi thành Customer
        Lost = 5            // Mất
    }

    /// <summary>
    /// Nguồn Lead.
    /// </summary>
    public enum LeadSource
    {
        Website = 0,
        Referral = 1,
        SocialMedia = 2,
        Email = 3,
        Phone = 4,
        Advertisement = 5,
        Event = 6,
        Other = 7
    }

    /// <summary>
    /// Trạng thái Opportunity (cơ hội bán hàng).
    /// </summary>
    public enum OpportunityStage
    {
        Qualification = 0,      // Đánh giá
        NeedsAnalysis = 1,      // Phân tích nhu cầu
        Proposal = 2,           // Đề xuất
        Negotiation = 3,        // Đàm phán
        ClosedWon = 4,          // Thắng
        ClosedLost = 5          // Thua
    }

    /// <summary>
    /// Trạng thái Quotation (báo giá).
    /// </summary>
    public enum QuotationStatus
    {
        Draft = 0,          // Nháp
        Sent = 1,           // Đã gửi
        Accepted = 2,       // Chấp nhận
        Rejected = 3,       // Từ chối
        Expired = 4,        // Hết hạn
        Converted = 5       // Đã chuyển thành Order
    }

    /// <summary>
    /// Loại hoạt động CRM.
    /// </summary>
    public enum ActivityType
    {
        Call = 0,           // Cuộc gọi
        Email = 1,          // Email
        Meeting = 2,        // Cuộc họp
        Note = 3,           // Ghi chú
        Task = 4,           // Công việc
        Visit = 5           // Ghé thăm
    }

    /// <summary>
    /// Trạng thái Activity.
    /// </summary>
    public enum ActivityStatus
    {
        Planned = 0,        // Lên kế hoạch
        InProgress = 1,     // Đang thực hiện
        Completed = 2,      // Hoàn thành
        Cancelled = 3       // Hủy
    }

    /// <summary>
    /// Trạng thái Campaign.
    /// </summary>
    public enum CampaignStatus
    {
        Draft = 0,          // Nháp
        Active = 1,         // Đang chạy
        Paused = 2,         // Tạm dừng
        Completed = 3,      // Hoàn thành
        Cancelled = 4       // Hủy
    }

    /// <summary>
    /// Loại Campaign.
    /// </summary>
    public enum CampaignType
    {
        Email = 0,
        SMS = 1,
        Social = 2,
        Event = 3,
        Promotion = 4,
        Other = 5
    }

    /// <summary>
    /// Loại giao dịch Loyalty.
    /// </summary>
    public enum LoyaltyTransactionType
    {
        Earn = 0,           // Tích điểm
        Redeem = 1,         // Đổi điểm
        Expire = 2,         // Hết hạn
        Adjust = 3          // Điều chỉnh
    }
}
