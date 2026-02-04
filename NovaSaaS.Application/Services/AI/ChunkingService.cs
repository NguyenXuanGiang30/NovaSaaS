using NovaSaaS.Application.Interfaces.AI;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NovaSaaS.Application.Services.AI
{
    /// <summary>
    /// ChunkingService - Chia nhỏ văn bản thành các chunks.
    /// 
    /// Sử dụng kỹ thuật:
    /// - Overlap (gối đầu) để không mất ngữ cảnh
    /// - Ưu tiên cắt tại ranh giới câu/đoạn
    /// </summary>
    public class ChunkingService : IChunkingService
    {
        private const int DefaultChunkSize = 800;
        private const int DefaultOverlap = 150;

        /// <summary>
        /// Chia văn bản thành các chunks với overlap.
        /// </summary>
        public List<TextChunk> ChunkText(string text, int chunkSize = DefaultChunkSize, int overlap = DefaultOverlap)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<TextChunk>();

            // Normalize whitespace
            text = NormalizeText(text);

            var chunks = new List<TextChunk>();
            int startPos = 0;
            int index = 0;

            while (startPos < text.Length)
            {
                // Tính vị trí kết thúc
                int endPos = Math.Min(startPos + chunkSize, text.Length);

                // Nếu không phải chunk cuối, tìm ranh giới tốt hơn
                if (endPos < text.Length)
                {
                    endPos = FindBestBreakPoint(text, startPos, endPos);
                }

                // Tạo chunk
                var content = text.Substring(startPos, endPos - startPos).Trim();

                if (!string.IsNullOrWhiteSpace(content))
                {
                    chunks.Add(new TextChunk
                    {
                        Index = index,
                        Content = content,
                        StartPosition = startPos,
                        EndPosition = endPos
                    });
                    index++;
                }

                // Di chuyển với overlap
                if (endPos >= text.Length)
                    break;

                startPos = endPos - overlap;
                if (startPos <= chunks[^1].StartPosition)
                    startPos = endPos; // Tránh loop vô hạn
            }

            return chunks;
        }

        /// <summary>
        /// Ước tính số tokens (1 token ≈ 4 ký tự cho tiếng Anh, 2-3 cho tiếng Việt).
        /// </summary>
        public int EstimateTokenCount(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            // Đếm số ký tự không phải whitespace
            int charCount = 0;
            foreach (char c in text)
            {
                if (!char.IsWhiteSpace(c))
                    charCount++;
            }

            // Tiếng Việt có nhiều ký tự hơn per token
            // Ước tính: 1 token ≈ 3 ký tự
            return (int)Math.Ceiling(charCount / 3.0);
        }

        /// <summary>
        /// Tìm điểm cắt tốt nhất (ưu tiên: đoạn > câu > từ).
        /// </summary>
        private int FindBestBreakPoint(string text, int startPos, int targetEndPos)
        {
            // Tìm trong khoảng từ targetEndPos lùi về 100 ký tự
            int searchStart = Math.Max(targetEndPos - 100, startPos + 50);

            // Ưu tiên 1: Tìm cuối đoạn (double newline)
            int paragraphBreak = text.LastIndexOf("\n\n", targetEndPos, targetEndPos - searchStart);
            if (paragraphBreak > searchStart)
                return paragraphBreak + 2;

            // Ưu tiên 2: Tìm cuối câu
            for (int i = targetEndPos; i >= searchStart; i--)
            {
                char c = text[i];
                if (c == '.' || c == '!' || c == '?' || c == '。')
                {
                    // Kiểm tra không phải viết tắt
                    if (i + 1 < text.Length && (char.IsWhiteSpace(text[i + 1]) || text[i + 1] == '\n'))
                        return i + 1;
                }
            }

            // Ưu tiên 3: Tìm dấu phẩy hoặc chấm phẩy
            for (int i = targetEndPos; i >= searchStart; i--)
            {
                char c = text[i];
                if (c == ',' || c == ';' || c == '、')
                    return i + 1;
            }

            // Ưu tiên 4: Tìm khoảng trắng
            for (int i = targetEndPos; i >= searchStart; i--)
            {
                if (char.IsWhiteSpace(text[i]))
                    return i;
            }

            // Không tìm thấy, cắt tại target
            return targetEndPos;
        }

        /// <summary>
        /// Normalize văn bản: loại bỏ whitespace thừa.
        /// </summary>
        private string NormalizeText(string text)
        {
            // Thay nhiều newlines liên tiếp bằng 2
            text = Regex.Replace(text, @"\n{3,}", "\n\n");

            // Thay nhiều spaces liên tiếp bằng 1
            text = Regex.Replace(text, @"[ \t]+", " ");

            // Xóa space đầu/cuối dòng
            text = Regex.Replace(text, @"[ \t]+\n", "\n");
            text = Regex.Replace(text, @"\n[ \t]+", "\n");

            return text.Trim();
        }
    }
}
