using System.Collections.Generic;

namespace NovaSaaS.Application.Interfaces.AI
{
    /// <summary>
    /// Một chunk văn bản sau khi chia nhỏ.
    /// </summary>
    public class TextChunk
    {
        /// <summary>
        /// Thứ tự chunk (0-based).
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Nội dung text của chunk.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Vị trí bắt đầu trong văn bản gốc.
        /// </summary>
        public int StartPosition { get; set; }

        /// <summary>
        /// Vị trí kết thúc trong văn bản gốc.
        /// </summary>
        public int EndPosition { get; set; }

        /// <summary>
        /// Số ký tự trong chunk.
        /// </summary>
        public int CharacterCount => Content.Length;
    }

    /// <summary>
    /// Interface cho Chunking Service.
    /// Chia nhỏ văn bản thành các đoạn phù hợp cho embedding.
    /// </summary>
    public interface IChunkingService
    {
        /// <summary>
        /// Chia văn bản thành các chunks.
        /// </summary>
        /// <param name="text">Văn bản gốc</param>
        /// <param name="chunkSize">Kích thước mỗi chunk (ký tự)</param>
        /// <param name="overlap">Số ký tự gối đầu giữa các chunks</param>
        /// <returns>Danh sách chunks</returns>
        List<TextChunk> ChunkText(string text, int chunkSize = 800, int overlap = 150);

        /// <summary>
        /// Ước tính số tokens từ số ký tự.
        /// </summary>
        int EstimateTokenCount(string text);
    }
}
