using DocumentFormat.OpenXml.Packaging;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using NovaSaaS.Application.Interfaces.AI;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NovaSaaS.Infrastructure.AI
{
    /// <summary>
    /// TextExtractionService - Trích xuất văn bản từ PDF, DOCX, TXT.
    /// 
    /// Sử dụng:
    /// - iText7 cho PDF
    /// - DocumentFormat.OpenXml cho DOCX
    /// </summary>
    public class TextExtractionService : ITextExtractionService
    {
        /// <summary>
        /// Trích xuất text tự động dựa vào extension.
        /// </summary>
        public async Task<string> ExtractTextAsync(Stream fileStream, string fileExtension)
        {
            var ext = fileExtension.ToLowerInvariant().TrimStart('.');

            return ext switch
            {
                "pdf" => await ExtractFromPdfAsync(fileStream),
                "docx" => await ExtractFromDocxAsync(fileStream),
                "txt" => await ExtractFromTxtAsync(fileStream),
                _ => throw new NotSupportedException($"File type '{ext}' is not supported")
            };
        }

        /// <summary>
        /// Trích xuất text từ PDF sử dụng iText7.
        /// </summary>
        public Task<string> ExtractFromPdfAsync(Stream fileStream)
        {
            return Task.Run(() =>
            {
                var text = new StringBuilder();

                using var memoryStream = new MemoryStream();
                fileStream.CopyTo(memoryStream);
                memoryStream.Position = 0;

                using var pdfReader = new PdfReader(memoryStream);
                using var pdfDoc = new PdfDocument(pdfReader);

                for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
                {
                    var pageObj = pdfDoc.GetPage(page);
                    var strategy = new SimpleTextExtractionStrategy();
                    var pageText = PdfTextExtractor.GetTextFromPage(pageObj, strategy);

                    if (!string.IsNullOrWhiteSpace(pageText))
                    {
                        text.AppendLine(pageText);
                        text.AppendLine(); // Phân cách giữa các trang
                    }
                }

                return text.ToString().Trim();
            });
        }

        /// <summary>
        /// Trích xuất text từ DOCX sử dụng OpenXml.
        /// </summary>
        public Task<string> ExtractFromDocxAsync(Stream fileStream)
        {
            return Task.Run(() =>
            {
                var text = new StringBuilder();

                using var memoryStream = new MemoryStream();
                fileStream.CopyTo(memoryStream);
                memoryStream.Position = 0;

                using var wordDoc = WordprocessingDocument.Open(memoryStream, false);
                var body = wordDoc.MainDocumentPart?.Document?.Body;

                if (body != null)
                {
                    foreach (var paragraph in body.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
                    {
                        var paragraphText = paragraph.InnerText;
                        if (!string.IsNullOrWhiteSpace(paragraphText))
                        {
                            text.AppendLine(paragraphText);
                        }
                    }
                }

                return text.ToString().Trim();
            });
        }

        /// <summary>
        /// Đọc text từ TXT file.
        /// </summary>
        public async Task<string> ExtractFromTxtAsync(Stream fileStream)
        {
            using var reader = new StreamReader(fileStream, Encoding.UTF8);
            return await reader.ReadToEndAsync();
        }
    }
}
