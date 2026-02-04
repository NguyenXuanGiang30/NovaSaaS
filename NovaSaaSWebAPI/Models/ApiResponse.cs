namespace NovaSaaS.WebApi.Models
{
    /// <summary>
    /// Enterprise API Response - Cấu trúc phản hồi chuẩn cho toàn bộ hệ thống.
    /// </summary>
    public class ApiResponse
    {
        /// <summary>
        /// Yêu cầu có thành công không.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Thông điệp cho người dùng.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Data trả về (nếu có).
        /// </summary>
        public object? Data { get; set; }
    }

    /// <summary>
    /// Enterprise API Response với Generic Type.
    /// </summary>
    public class ApiResponse<T> : ApiResponse
    {
        /// <summary>
        /// Data trả về với kiểu cụ thể.
        /// </summary>
        public new T? Data { get; set; }

        public static ApiResponse<T> SuccessResult(T data, string message = "Thành công")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> FailResult(string message)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default
            };
        }
    }

    /// <summary>
    /// Enterprise Error Response - Phản hồi lỗi chuẩn.
    /// </summary>
    public class ApiErrorResponse
    {
        /// <summary>
        /// Luôn là false.
        /// </summary>
        public bool Success { get; set; } = false;

        /// <summary>
        /// Thông điệp thân thiện cho người dùng.
        /// </summary>
        public string Message { get; set; } = "Một lỗi không mong muốn đã xảy ra. Vui lòng thử lại sau.";

        /// <summary>
        /// Mã lỗi để tra cứu (ERR_AUTH_001, ERR_INVENTORY_002, etc.).
        /// </summary>
        public string ErrorCode { get; set; } = "ERR_SYSTEM_000";

        /// <summary>
        /// TraceId để tìm log trong hệ thống.
        /// </summary>
        public string TraceId { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp khi lỗi xảy ra.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Chi tiết lỗi (chỉ hiển thị trong Development).
        /// </summary>
        public string? Details { get; set; }
    }
}
