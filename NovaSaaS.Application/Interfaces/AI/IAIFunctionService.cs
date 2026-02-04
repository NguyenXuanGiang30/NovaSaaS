using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace NovaSaaS.Application.Interfaces.AI
{
    /// <summary>
    /// Định nghĩa một function mà AI có thể gọi.
    /// Theo format của Gemini/OpenAI function calling.
    /// </summary>
    public class FunctionDefinition
    {
        /// <summary>
        /// Tên function (dùng để AI gọi).
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả function (giúp AI hiểu khi nào nên dùng).
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Schema tham số (JSON Schema format).
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new();
    }

    /// <summary>
    /// Kết quả của một function call.
    /// </summary>
    public class FunctionCallResult
    {
        /// <summary>
        /// Tên function đã được gọi.
        /// </summary>
        public string FunctionName { get; set; } = string.Empty;

        /// <summary>
        /// Tham số được AI truyền vào.
        /// </summary>
        public Dictionary<string, object?> Arguments { get; set; } = new();

        /// <summary>
        /// Kết quả thực thi (JSON serializable).
        /// </summary>
        public object? Result { get; set; }

        /// <summary>
        /// Thực thi thành công hay không.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Lỗi nếu có.
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// AI response có thể chứa text hoặc function call.
    /// </summary>
    public class AIResponseWithFunctions
    {
        /// <summary>
        /// Text response (nếu AI trả lời trực tiếp).
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Function call request (nếu AI muốn gọi function).
        /// </summary>
        public FunctionCallRequest? FunctionCall { get; set; }

        /// <summary>
        /// AI đã hoàn thành (true) hay cần execute function và tiếp tục (false).
        /// </summary>
        public bool IsComplete { get; set; }
    }

    /// <summary>
    /// Request từ AI để gọi một function.
    /// </summary>
    public class FunctionCallRequest
    {
        public string Name { get; set; } = string.Empty;
        public string ArgumentsJson { get; set; } = "{}";
    }

    /// <summary>
    /// Interface cho AI Function Calling Service.
    /// Cho phép AI thực hiện actions thông qua function calling.
    /// </summary>
    public interface IAIFunctionService
    {
        /// <summary>
        /// Lấy danh sách tất cả functions có sẵn.
        /// </summary>
        IReadOnlyList<FunctionDefinition> GetAvailableFunctions();

        /// <summary>
        /// Thực thi một function call từ AI.
        /// </summary>
        /// <param name="functionName">Tên function</param>
        /// <param name="argumentsJson">Arguments dạng JSON string</param>
        /// <returns>Kết quả thực thi</returns>
        Task<FunctionCallResult> ExecuteFunctionAsync(string functionName, string argumentsJson);

        /// <summary>
        /// Chat với AI có hỗ trợ function calling.
        /// Tự động thực thi functions nếu AI yêu cầu.
        /// </summary>
        /// <param name="userMessage">Tin nhắn từ user</param>
        /// <param name="maxIterations">Số vòng function call tối đa</param>
        /// <returns>Response cuối cùng từ AI</returns>
        Task<string> ChatWithFunctionsAsync(string userMessage, int maxIterations = 5);
    }
}
