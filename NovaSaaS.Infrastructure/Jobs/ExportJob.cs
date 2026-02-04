using NovaSaaS.Application.Interfaces;
using NovaSaaS.Application.Services;
using NovaSaaS.Domain.Entities.Business;
using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using NovaSaaS.Domain.Interfaces;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace NovaSaaS.Infrastructure.Jobs
{
    /// <summary>
    /// ExportJob - Background job xuất dữ liệu ra file CSV.
    /// Xử lý async các báo cáo lớn và thông báo qua SignalR khi hoàn thành.
    /// </summary>
    public class ExportJob
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ExportJob> _logger;

        public ExportJob(
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            ILogger<ExportJob> logger)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Xuất danh sách đơn hàng ra CSV.
        /// </summary>
        [Queue("reports")]
        [AutomaticRetry(Attempts = 2)]
        public async Task ExportOrdersAsync(
            Guid userId,
            Guid tenantId,
            DateTime? fromDate,
            DateTime? toDate,
            string? status)
        {
            try
            {
                _logger.LogInformation("Starting orders export for user {UserId}", userId);

                // Build query
                var orders = await _unitOfWork.Orders.FindAsync(o =>
                    (!fromDate.HasValue || o.CreateAt >= fromDate.Value) &&
                    (!toDate.HasValue || o.CreateAt <= toDate.Value),
                    o => o.Customer, o => o.OrderItems);

                if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, out var orderStatus))
                {
                    orders = orders.Where(o => o.Status == orderStatus);
                }

                var orderList = orders.ToList();

                // Generate CSV
                var fileName = $"orders_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
                var filePath = Path.Combine(Path.GetTempPath(), fileName);

                var lines = new List<string>
                {
                    "OrderNumber,CustomerName,Status,TotalAmount,CreatedAt"
                };

                foreach (var order in orderList)
                {
                    lines.Add($"{order.OrderNumber},{order.Customer?.Name ?? "N/A"}," +
                        $"{order.Status},{order.TotalAmount},{order.CreateAt:yyyy-MM-dd HH:mm}");
                }

                await File.WriteAllLinesAsync(filePath, lines);

                // Notify user via SignalR
                await _notificationService.NotifyUserAsync(
                    userId,
                    "ReportReady",
                    "Xuất báo cáo hoàn tất",
                    $"Đã xuất {orderList.Count} đơn hàng. File: {fileName}");

                _logger.LogInformation("Orders export completed: {Count} records", orderList.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export orders for user {UserId}", userId);

                await _notificationService.NotifyUserAsync(
                    userId,
                    "ReportError",
                    "Xuất báo cáo thất bại",
                    "Có lỗi xảy ra khi xuất báo cáo đơn hàng. Vui lòng thử lại.");

                throw;
            }
        }

        /// <summary>
        /// Xuất lịch sử xuất nhập kho.
        /// </summary>
        [Queue("reports")]
        [AutomaticRetry(Attempts = 2)]
        public async Task ExportStockMovementsAsync(
            Guid userId,
            Guid tenantId,
            DateTime? fromDate,
            DateTime? toDate,
            Guid? warehouseId)
        {
            try
            {
                _logger.LogInformation("Starting stock movements export for user {UserId}", userId);

                var movements = await _unitOfWork.StockMovements.FindAsync(m =>
                    (!fromDate.HasValue || m.CreateAt >= fromDate.Value) &&
                    (!toDate.HasValue || m.CreateAt <= toDate.Value) &&
                    (!warehouseId.HasValue || m.WarehouseId == warehouseId.Value),
                    m => m.Product, m => m.Warehouse);

                var movementList = movements.ToList();

                var fileName = $"stock_movements_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
                var filePath = Path.Combine(Path.GetTempPath(), fileName);

                var lines = new List<string>
                {
                    "ProductSKU,ProductName,Warehouse,Type,Quantity,Reference,CreatedAt"
                };

                foreach (var m in movementList)
                {
                    lines.Add($"{m.Product?.SKU},{m.Product?.Name},{m.Warehouse?.Name}," +
                        $"{m.Type},{m.Quantity},{m.ReferenceCode},{m.CreateAt:yyyy-MM-dd HH:mm}");
                }

                await File.WriteAllLinesAsync(filePath, lines);

                await _notificationService.NotifyUserAsync(
                    userId,
                    "ReportReady",
                    "Xuất báo cáo hoàn tất",
                    $"Đã xuất {movementList.Count} bản ghi xuất nhập kho.");

                _logger.LogInformation("Stock movements export completed: {Count} records", movementList.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export stock movements for user {UserId}", userId);

                await _notificationService.NotifyUserAsync(
                    userId,
                    "ReportError",
                    "Xuất báo cáo thất bại",
                    "Có lỗi xảy ra khi xuất báo cáo xuất nhập kho.");

                throw;
            }
        }

        /// <summary>
        /// Xuất danh sách khách hàng.
        /// </summary>
        [Queue("reports")]
        public async Task ExportCustomersAsync(Guid userId, Guid tenantId)
        {
            try
            {
                var customers = await _unitOfWork.Customers.FindAsync(c => !c.IsDeleted);
                var customerList = customers.ToList();

                var fileName = $"customers_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
                var filePath = Path.Combine(Path.GetTempPath(), fileName);

                var lines = new List<string>
                {
                    "Name,Email,Phone,Type,Rank,TotalSpending,CreatedAt"
                };

                foreach (var c in customerList)
                {
                    lines.Add($"{c.Name},{c.Email},{c.Phone},{c.Type},{c.Rank}," +
                        $"{c.TotalSpending},{c.CreateAt:yyyy-MM-dd}");
                }

                await File.WriteAllLinesAsync(filePath, lines);

                await _notificationService.NotifyUserAsync(
                    userId,
                    "ReportReady",
                    "Xuất danh sách khách hàng hoàn tất",
                    $"Đã xuất {customerList.Count} khách hàng.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export customers");
                throw;
            }
        }
    }
}
