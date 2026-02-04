using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaSaaS.Application.Interfaces;
using NovaSaaS.Application.Services.Export;
using NovaSaaS.Domain.Interfaces;
using NovaSaaS.Infrastructure.Jobs;
using NovaSaaS.WebApi.Authorization;
using NovaSaaS.WebApi.Models;
using Hangfire;

namespace NovaSaaS.WebApi.Controllers
{
    /// <summary>
    /// Reports Controller - API xuất báo cáo.
    /// Hỗ trợ cả export trực tiếp (nhỏ) và background jobs (lớn).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Tags("Analytics")]
    public class ReportsController : ControllerBase
    {
        private readonly IBackgroundJobClient _backgroundJobs;
        private readonly ICurrentUserService _currentUserService;
        private readonly ITenantService _tenantService;
        private readonly IExportService _exportService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(
            IBackgroundJobClient backgroundJobs,
            ICurrentUserService currentUserService,
            ITenantService tenantService,
            IExportService exportService,
            ILogger<ReportsController> logger)
        {
            _backgroundJobs = backgroundJobs;
            _currentUserService = currentUserService;
            _tenantService = tenantService;
            _exportService = exportService;
            _logger = logger;
        }

        private Guid GetCurrentUserId()
        {
            if (!string.IsNullOrEmpty(_currentUserService.UserId) &&
                Guid.TryParse(_currentUserService.UserId, out var userId))
            {
                return userId;
            }
            return Guid.Empty;
        }

        private Guid GetTenantId()
        {
            return _tenantService.TenantId ?? Guid.Empty;
        }

        #region Immediate Download (Excel)

        /// <summary>
        /// Download báo cáo đơn hàng Excel (trực tiếp).
        /// </summary>
        [HttpGet("download/orders.xlsx")]
        [RequirePermission("reports.export")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> DownloadOrdersExcel(
            [FromQuery] DateTime? fromDate, 
            [FromQuery] DateTime? toDate)
        {
            var start = fromDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = toDate ?? DateTime.UtcNow;

            var bytes = await _exportService.ExportOrdersReportAsync(start, end);
            return File(bytes, 
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"orders_{start:yyyyMMdd}_{end:yyyyMMdd}.xlsx");
        }

        /// <summary>
        /// Download báo cáo sản phẩm Excel (trực tiếp).
        /// </summary>
        [HttpGet("download/products.xlsx")]
        [RequirePermission("reports.export")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> DownloadProductsExcel()
        {
            var bytes = await _exportService.ExportProductsReportAsync();
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"products_{DateTime.UtcNow:yyyyMMdd}.xlsx");
        }

        /// <summary>
        /// Download báo cáo khách hàng Excel (trực tiếp).
        /// </summary>
        [HttpGet("download/customers.xlsx")]
        [RequirePermission("reports.export")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> DownloadCustomersExcel()
        {
            var bytes = await _exportService.ExportCustomersReportAsync();
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"customers_{DateTime.UtcNow:yyyyMMdd}.xlsx");
        }

        /// <summary>
        /// Download báo cáo hóa đơn Excel (trực tiếp).
        /// </summary>
        [HttpGet("download/invoices.xlsx")]
        [RequirePermission("reports.export")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> DownloadInvoicesExcel(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var start = fromDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = toDate ?? DateTime.UtcNow;

            var bytes = await _exportService.ExportInvoicesReportAsync(start, end);
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"invoices_{start:yyyyMMdd}_{end:yyyyMMdd}.xlsx");
        }

        #endregion

        #region Background Export Jobs (Large datasets)

        /// <summary>
        /// Yêu cầu xuất báo cáo đơn hàng (background job cho dataset lớn).
        /// </summary>
        [HttpPost("export/orders")]
        [RequirePermission("reports.export")]
        [ProducesResponseType(typeof(ApiResponse<ExportJobResponse>), StatusCodes.Status202Accepted)]
        public IActionResult ExportOrders([FromBody] ExportOrdersRequest request)
        {
            var userId = GetCurrentUserId();
            var tenantId = GetTenantId();

            var jobId = _backgroundJobs.Enqueue<ExportJob>(job =>
                job.ExportOrdersAsync(userId, tenantId, request.FromDate, request.ToDate, request.Status));

            _logger.LogInformation("Export orders job queued: {JobId} by user {UserId}", jobId, userId);

            return Accepted(ApiResponse<ExportJobResponse>.SuccessResult(new ExportJobResponse
            {
                JobId = jobId,
                Message = "Yêu cầu xuất báo cáo đã được ghi nhận. Bạn sẽ nhận thông báo khi hoàn tất."
            }));
        }

        /// <summary>
        /// Yêu cầu xuất báo cáo xuất nhập kho.
        /// </summary>
        [HttpPost("export/stock-movements")]
        [RequirePermission("reports.export")]
        [ProducesResponseType(typeof(ApiResponse<ExportJobResponse>), StatusCodes.Status202Accepted)]
        public IActionResult ExportStockMovements([FromBody] ExportStockMovementsRequest request)
        {
            var userId = GetCurrentUserId();
            var tenantId = GetTenantId();

            var jobId = _backgroundJobs.Enqueue<ExportJob>(job =>
                job.ExportStockMovementsAsync(userId, tenantId, request.FromDate, request.ToDate, request.WarehouseId));

            _logger.LogInformation("Export stock movements job queued: {JobId}", jobId);

            return Accepted(ApiResponse<ExportJobResponse>.SuccessResult(new ExportJobResponse
            {
                JobId = jobId,
                Message = "Yêu cầu xuất báo cáo xuất nhập kho đã được ghi nhận."
            }));
        }

        /// <summary>
        /// Yêu cầu xuất danh sách khách hàng.
        /// </summary>
        [HttpPost("export/customers")]
        [RequirePermission("reports.export")]
        [ProducesResponseType(typeof(ApiResponse<ExportJobResponse>), StatusCodes.Status202Accepted)]
        public IActionResult ExportCustomers()
        {
            var userId = GetCurrentUserId();
            var tenantId = GetTenantId();

            var jobId = _backgroundJobs.Enqueue<ExportJob>(job =>
                job.ExportCustomersAsync(userId, tenantId));

            return Accepted(ApiResponse<ExportJobResponse>.SuccessResult(new ExportJobResponse
            {
                JobId = jobId,
                Message = "Yêu cầu xuất danh sách khách hàng đã được ghi nhận."
            }));
        }

        #endregion

        /// <summary>
        /// Download file báo cáo đã xuất (từ background job).
        /// </summary>
        [HttpGet("download/{fileName}")]
        [RequirePermission("reports.view")]
        public IActionResult DownloadReport(string fileName)
        {
            // Validate filename
            if (string.IsNullOrEmpty(fileName) || 
                fileName.Contains("..") || 
                Path.GetInvalidFileNameChars().Any(c => fileName.Contains(c)))
            {
                return BadRequest(ApiResponse<object>.FailResult("Tên file không hợp lệ."));
            }

            var filePath = Path.Combine(Path.GetTempPath(), fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(ApiResponse<object>.FailResult("File không tồn tại hoặc đã hết hạn."));
            }

            var contentType = fileName.EndsWith(".csv") 
                ? "text/csv" 
                : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return PhysicalFile(filePath, contentType, fileName);
        }
    }

    #region DTOs

    public class ExportOrdersRequest
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Status { get; set; }
    }

    public class ExportStockMovementsRequest
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? WarehouseId { get; set; }
    }

    public class ExportJobResponse
    {
        public string JobId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    #endregion
}
