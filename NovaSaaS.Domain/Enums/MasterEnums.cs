namespace NovaSaaS.Domain.Enums
{
    /// <summary>
    /// Trạng thái của Tenant trong hệ thống.
    /// </summary>
    public enum TenantStatus
    {
        /// <summary>
        /// Đang khởi tạo hạ tầng (schema, tables).
        /// </summary>
        Provisioning = 0,

        /// <summary>
        /// Hoạt động bình thường.
        /// </summary>
        Active = 1,

        /// <summary>
        /// Tạm khóa do quá hạn thanh toán hoặc vi phạm.
        /// </summary>
        Suspended = 2,

        /// <summary>
        /// Đã hủy hợp đồng, không thể phục hồi.
        /// </summary>
        Terminated = 3
    }

    /// <summary>
    /// Loại Usage Log để tracking resource consumption.
    /// </summary>
    public enum UsageType
    {
        /// <summary>
        /// Chat với AI (Gemini Chat Completion).
        /// </summary>
        AIChat = 0,

        /// <summary>
        /// Tạo embeddings cho RAG.
        /// </summary>
        AIEmbedding = 1,

        /// <summary>
        /// Upload tài liệu.
        /// </summary>
        DocumentUpload = 2,

        /// <summary>
        /// Storage usage (tính theo MB).
        /// </summary>
        DocumentStorage = 3
    }

    /// <summary>
    /// Mức độ nghiêm trọng của System Log.
    /// </summary>
    public enum SystemLogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        Critical = 4
    }
}
