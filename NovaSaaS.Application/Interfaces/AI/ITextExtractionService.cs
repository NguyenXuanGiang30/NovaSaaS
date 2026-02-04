using System.IO;
using System.Threading.Tasks;

namespace NovaSaaS.Application.Interfaces.AI
{
    /// <summary>
    /// Interface cho Text Extraction Service.
    /// Trích xuất văn bản từ các loại file khác nhau.
    /// </summary>
    public interface ITextExtractionService
    {
        /// <summary>
        /// Trích xuất text từ file PDF.
        /// </summary>
        Task<string> ExtractFromPdfAsync(Stream fileStream);

        /// <summary>
        /// Trích xuất text từ file DOCX.
        /// </summary>
        Task<string> ExtractFromDocxAsync(Stream fileStream);

        /// <summary>
        /// Trích xuất text từ file TXT.
        /// </summary>
        Task<string> ExtractFromTxtAsync(Stream fileStream);

        /// <summary>
        /// Trích xuất text tự động dựa vào extension.
        /// </summary>
        /// <param name="fileStream">Stream của file</param>
        /// <param name="fileExtension">Extension (.pdf, .docx, .txt)</param>
        Task<string> ExtractTextAsync(Stream fileStream, string fileExtension);
    }
}
