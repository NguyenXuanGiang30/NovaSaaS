using System.Threading.Tasks;

namespace NovaSaaS.Application.Interfaces.AI
{
    /// <summary>
    /// Interface cho Chat Completion Service.
    /// Gọi LLM để generate responses.
    /// </summary>
    public interface IChatCompletionService
    {
        /// <summary>
        /// Generate response từ prompt.
        /// </summary>
        /// <param name="prompt">Prompt đầy đủ (bao gồm context)</param>
        /// <param name="maxTokens">Số tokens tối đa trong response</param>
        /// <returns>Response từ LLM</returns>
        Task<ChatCompletionResult> GenerateAsync(string prompt, int maxTokens = 2048);

        /// <summary>
        /// Generate response với system prompt.
        /// </summary>
        Task<ChatCompletionResult> GenerateAsync(string systemPrompt, string userMessage, int maxTokens = 2048);
    }

    /// <summary>
    /// Kết quả từ Chat Completion.
    /// </summary>
    public class ChatCompletionResult
    {
        /// <summary>
        /// Nội dung response.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Số tokens trong prompt.
        /// </summary>
        public int PromptTokens { get; set; }

        /// <summary>
        /// Số tokens trong response.
        /// </summary>
        public int CompletionTokens { get; set; }

        /// <summary>
        /// Tổng số tokens.
        /// </summary>
        public int TotalTokens => PromptTokens + CompletionTokens;

        /// <summary>
        /// Có thành công không.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Thông báo lỗi (nếu có).
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}
