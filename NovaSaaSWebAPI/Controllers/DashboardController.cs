using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaSaaS.Application.Services.Analytics;
using NovaSaaS.WebApi.Authorization;
using NovaSaaS.WebApi.Models;

namespace NovaSaaS.WebApi.Controllers
{
    /// <summary>
    /// Dashboard Controller - API thống kê và phân tích dữ liệu.
    /// Cung cấp metrics cho dashboard frontend.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Tags("Analytics")]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            DashboardService dashboardService,
            ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy thống kê tổng quan.
        /// Bao gồm: Revenue, Orders, Inventory, Customers, Invoices.
        /// </summary>
        [HttpGet("summary")]
        [RequirePermission("reports.view")]
        [ProducesResponseType(typeof(ApiResponse<DashboardSummary>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSummary()
        {
            var summary = await _dashboardService.GetSummaryAsync();
            return Ok(ApiResponse<DashboardSummary>.SuccessResult(summary));
        }

        /// <summary>
        /// Lấy dữ liệu biểu đồ doanh thu.
        /// </summary>
        /// <param name="period">week, month, quarter, year</param>
        [HttpGet("charts/revenue")]
        [RequirePermission("reports.view")]
        [ProducesResponseType(typeof(ApiResponse<List<ChartDataPoint>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRevenueChart([FromQuery] string period = "week")
        {
            var chartPeriod = ParsePeriod(period);
            var data = await _dashboardService.GetRevenueChartAsync(chartPeriod);
            return Ok(ApiResponse<List<ChartDataPoint>>.SuccessResult(data));
        }

        /// <summary>
        /// Lấy dữ liệu biểu đồ đơn hàng theo thời gian.
        /// </summary>
        [HttpGet("charts/orders")]
        [RequirePermission("reports.view")]
        [ProducesResponseType(typeof(ApiResponse<List<ChartDataPoint>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrdersChart([FromQuery] string period = "week")
        {
            var chartPeriod = ParsePeriod(period);
            var data = await _dashboardService.GetOrdersChartAsync(chartPeriod);
            return Ok(ApiResponse<List<ChartDataPoint>>.SuccessResult(data));
        }

        /// <summary>
        /// Lấy dữ liệu đơn hàng theo trạng thái (Pie chart).
        /// </summary>
        [HttpGet("charts/orders-by-status")]
        [RequirePermission("reports.view")]
        [ProducesResponseType(typeof(ApiResponse<List<ChartDataPoint>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrdersByStatus()
        {
            var data = await _dashboardService.GetOrdersByStatusAsync();
            return Ok(ApiResponse<List<ChartDataPoint>>.SuccessResult(data));
        }

        /// <summary>
        /// Lấy danh sách sản phẩm bán chạy nhất.
        /// </summary>
        [HttpGet("top-products")]
        [RequirePermission("reports.view")]
        [ProducesResponseType(typeof(ApiResponse<List<TopProductDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTopProducts([FromQuery] int count = 10)
        {
            var data = await _dashboardService.GetTopProductsAsync(count);
            return Ok(ApiResponse<List<TopProductDto>>.SuccessResult(data));
        }

        /// <summary>
        /// Lấy danh sách khách hàng top.
        /// </summary>
        [HttpGet("top-customers")]
        [RequirePermission("reports.view")]
        [ProducesResponseType(typeof(ApiResponse<List<TopCustomerDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTopCustomers([FromQuery] int count = 10)
        {
            var data = await _dashboardService.GetTopCustomersAsync(count);
            return Ok(ApiResponse<List<TopCustomerDto>>.SuccessResult(data));
        }

        /// <summary>
        /// Lấy thống kê sử dụng AI.
        /// </summary>
        [HttpGet("ai-usage")]
        [RequirePermission("reports.view")]
        [ProducesResponseType(typeof(ApiResponse<AIUsageSummary>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAIUsage()
        {
            var data = await _dashboardService.GetAIUsageAsync();
            return Ok(ApiResponse<AIUsageSummary>.SuccessResult(data));
        }

        /// <summary>
        /// Lấy cảnh báo: sản phẩm tồn kho thấp, hóa đơn quá hạn.
        /// </summary>
        [HttpGet("alerts")]
        [RequirePermission("reports.view")]
        [ProducesResponseType(typeof(ApiResponse<DashboardAlerts>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAlerts()
        {
            var alerts = await _dashboardService.GetAlertsAsync();
            return Ok(ApiResponse<DashboardAlerts>.SuccessResult(alerts));
        }

        /// <summary>
        /// So sánh hiệu suất giữa các khoảng thời gian.
        /// </summary>
        [HttpGet("comparison")]
        [RequirePermission("reports.view")]
        [ProducesResponseType(typeof(ApiResponse<PeriodComparison>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPeriodComparison([FromQuery] string period = "month")
        {
            var chartPeriod = ParsePeriod(period);
            var comparison = await _dashboardService.GetPeriodComparisonAsync(chartPeriod);
            return Ok(ApiResponse<PeriodComparison>.SuccessResult(comparison));
        }

        private ChartPeriod ParsePeriod(string period)
        {
            return period.ToLower() switch
            {
                "week" => ChartPeriod.Week,
                "month" => ChartPeriod.Month,
                "quarter" => ChartPeriod.Quarter,
                "year" => ChartPeriod.Year,
                _ => ChartPeriod.Week
            };
        }
    }
}
