using System.Collections.Generic;
using System.Threading.Tasks;

namespace NovaSaaS.Application.Interfaces.AI
{
    /// <summary>
    /// Interface cho Embedding Service.
    /// Chuyển đổi text thành vector để tìm kiếm ngữ nghĩa.
    /// </summary>
    public interface IEmbeddingService
    {
        /// <summary>
        /// Tạo embedding vector cho một đoạn text.
        /// </summary>
        /// <param name="text">Văn bản cần embed</param>
        /// <returns>Vector embedding (768 dimensions cho Gemini)</returns>
        Task<float[]> GenerateEmbeddingAsync(string text);

        /// <summary>
        /// Tạo embedding vectors cho nhiều đoạn text (batch).
        /// </summary>
        /// <param name="texts">Danh sách văn bản</param>
        /// <returns>Danh sách vectors</returns>
        Task<List<float[]>> GenerateEmbeddingsAsync(List<string> texts);
    }
}
