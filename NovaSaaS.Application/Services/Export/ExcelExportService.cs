using ClosedXML.Excel;
using NovaSaaS.Application.Interfaces;
using NovaSaaS.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace NovaSaaS.Application.Services.Export
{
    /// <summary>
    /// ExcelExportService - Service tạo báo cáo Excel.
    /// Sử dụng ClosedXML để tạo file Excel với styling.
    /// </summary>
    public class ExcelExportService : IExportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ExcelExportService> _logger;

        public ExcelExportService(
            IUnitOfWork unitOfWork,
            ILogger<ExcelExportService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Xuất báo cáo đơn hàng ra file Excel.
        /// </summary>
        public async Task<byte[]> ExportOrdersReportAsync(DateTime startDate, DateTime endDate)
        {
            var orders = await _unitOfWork.Orders.FindAsync(
                o => o.CreateAt >= startDate && o.CreateAt <= endDate && !o.IsDeleted,
                o => o.Customer,
                o => o.OrderItems);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Orders");

            // Header style
            var headerRow = 1;
            worksheet.Cell(headerRow, 1).Value = "Mã đơn hàng";
            worksheet.Cell(headerRow, 2).Value = "Khách hàng";
            worksheet.Cell(headerRow, 3).Value = "Ngày tạo";
            worksheet.Cell(headerRow, 4).Value = "Trạng thái";
            worksheet.Cell(headerRow, 5).Value = "Tổng tiền";
            worksheet.Cell(headerRow, 6).Value = "Số sản phẩm";

            var headerRange = worksheet.Range(headerRow, 1, headerRow, 6);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // Data rows
            var row = 2;
            foreach (var order in orders)
            {
                worksheet.Cell(row, 1).Value = order.OrderNumber;
                worksheet.Cell(row, 2).Value = order.Customer?.Name ?? "N/A";
                worksheet.Cell(row, 3).Value = order.CreateAt.ToString("dd/MM/yyyy HH:mm");
                worksheet.Cell(row, 4).Value = order.Status.ToString();
                worksheet.Cell(row, 5).Value = order.TotalAmount;
                worksheet.Cell(row, 6).Value = order.OrderItems?.Count ?? 0;
                row++;
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            // Currency format for total
            worksheet.Column(5).Style.NumberFormat.Format = "#,##0 ₫";

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        /// <summary>
        /// Xuất báo cáo sản phẩm ra file Excel.
        /// </summary>
        public async Task<byte[]> ExportProductsReportAsync()
        {
            var products = await _unitOfWork.Products.FindAsync(
                p => !p.IsDeleted,
                p => p.Category);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Products");

            // Header
            var headerRow = 1;
            worksheet.Cell(headerRow, 1).Value = "SKU";
            worksheet.Cell(headerRow, 2).Value = "Tên sản phẩm";
            worksheet.Cell(headerRow, 3).Value = "Danh mục";
            worksheet.Cell(headerRow, 4).Value = "Giá bán";
            worksheet.Cell(headerRow, 5).Value = "Tồn kho";
            worksheet.Cell(headerRow, 6).Value = "Tồn kho tối thiểu";
            worksheet.Cell(headerRow, 7).Value = "Trạng thái";

            var headerRange = worksheet.Range(headerRow, 1, headerRow, 7);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGreen;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // Data
            var row = 2;
            foreach (var product in products)
            {
                worksheet.Cell(row, 1).Value = product.SKU;
                worksheet.Cell(row, 2).Value = product.Name;
                worksheet.Cell(row, 3).Value = product.Category?.Name ?? "N/A";
                worksheet.Cell(row, 4).Value = product.Price;
                worksheet.Cell(row, 5).Value = product.StockQuantity;
                worksheet.Cell(row, 6).Value = product.MinStockLevel;
                worksheet.Cell(row, 7).Value = product.IsActive ? "Hoạt động" : "Ngừng bán";

                // Highlight low stock
                if (product.StockQuantity <= product.MinStockLevel)
                {
                    worksheet.Range(row, 5, row, 5).Style.Fill.BackgroundColor = XLColor.LightSalmon;
                }

                row++;
            }

            worksheet.Columns().AdjustToContents();
            worksheet.Column(4).Style.NumberFormat.Format = "#,##0 ₫";

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        /// <summary>
        /// Xuất báo cáo khách hàng ra file Excel.
        /// </summary>
        public async Task<byte[]> ExportCustomersReportAsync()
        {
            var customers = await _unitOfWork.Customers.FindAsync(c => !c.IsDeleted);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Customers");

            // Header
            worksheet.Cell(1, 1).Value = "Tên khách hàng";
            worksheet.Cell(1, 2).Value = "Email";
            worksheet.Cell(1, 3).Value = "Số điện thoại";
            worksheet.Cell(1, 4).Value = "Tổng chi tiêu";
            worksheet.Cell(1, 5).Value = "Ngày tạo";

            var headerRange = worksheet.Range(1, 1, 1, 5);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightYellow;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            var row = 2;
            foreach (var customer in customers.OrderByDescending(c => c.TotalSpending))
            {
                worksheet.Cell(row, 1).Value = customer.Name;
                worksheet.Cell(row, 2).Value = customer.Email ?? "";
                worksheet.Cell(row, 3).Value = customer.Phone ?? "";
                worksheet.Cell(row, 4).Value = customer.TotalSpending;
                worksheet.Cell(row, 5).Value = customer.CreateAt.ToString("dd/MM/yyyy");
                row++;
            }

            worksheet.Columns().AdjustToContents();
            worksheet.Column(4).Style.NumberFormat.Format = "#,##0 ₫";

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        /// <summary>
        /// Xuất báo cáo hóa đơn ra file Excel.
        /// </summary>
        public async Task<byte[]> ExportInvoicesReportAsync(DateTime startDate, DateTime endDate)
        {
            var invoices = await _unitOfWork.Invoices.FindAsync(
                i => i.CreateAt >= startDate && i.CreateAt <= endDate,
                i => i.Order.Customer);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Invoices");

            // Header
            worksheet.Cell(1, 1).Value = "Số hóa đơn";
            worksheet.Cell(1, 2).Value = "Khách hàng";
            worksheet.Cell(1, 3).Value = "Ngày phát hành";
            worksheet.Cell(1, 4).Value = "Hạn thanh toán";
            worksheet.Cell(1, 5).Value = "Trạng thái";
            worksheet.Cell(1, 6).Value = "Tổng tiền";

            var headerRange = worksheet.Range(1, 1, 1, 6);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightCoral;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            var row = 2;
            foreach (var invoice in invoices)
            {
                worksheet.Cell(row, 1).Value = invoice.InvoiceNumber;
                worksheet.Cell(row, 2).Value = invoice.Order?.Customer?.Name ?? "N/A";
                worksheet.Cell(row, 3).Value = invoice.CreateAt.ToString("dd/MM/yyyy");
                worksheet.Cell(row, 4).Value = invoice.DueDate?.ToString("dd/MM/yyyy") ?? "N/A";
                worksheet.Cell(row, 5).Value = invoice.Status.ToString();
                worksheet.Cell(row, 6).Value = invoice.TotalAmount;

                // Highlight overdue
                if (invoice.DueDate.HasValue && invoice.DueDate.Value < DateTime.UtcNow && 
                    invoice.Status == Domain.Enums.InvoiceStatus.Unpaid)
                {
                    worksheet.Range(row, 1, row, 6).Style.Fill.BackgroundColor = XLColor.LightPink;
                }

                row++;
            }

            worksheet.Columns().AdjustToContents();
            worksheet.Column(6).Style.NumberFormat.Format = "#,##0 ₫";

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    /// <summary>
    /// Interface cho export service.
    /// </summary>
    public interface IExportService
    {
        Task<byte[]> ExportOrdersReportAsync(DateTime startDate, DateTime endDate);
        Task<byte[]> ExportProductsReportAsync();
        Task<byte[]> ExportCustomersReportAsync();
        Task<byte[]> ExportInvoicesReportAsync(DateTime startDate, DateTime endDate);
    }
}
