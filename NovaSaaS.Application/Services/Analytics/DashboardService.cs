using NovaSaaS.Application.Interfaces;
using NovaSaaS.Domain.Entities.Business;
using NovaSaaS.Domain.Enums;
using NovaSaaS.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace NovaSaaS.Application.Services.Analytics
{
    /// <summary>
    /// DashboardService - Thống kê và phân tích dữ liệu kinh doanh.
    /// Cung cấp metrics cho dashboard: revenue, orders, stock alerts, etc.
    /// </summary>
    public class DashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<DashboardService> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        #region Summary Statistics

        /// <summary>
        /// Lấy thống kê tổng quan cho dashboard.
        /// </summary>
        public async Task<DashboardSummary> GetSummaryAsync()
        {
            var today = DateTime.UtcNow.Date;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var startOfLastMonth = startOfMonth.AddMonths(-1);

            // Revenue calculations
            var orders = await _unitOfWork.Orders.FindAsync(o => 
                o.Status != OrderStatus.Cancelled);

            var todayRevenue = orders
                .Where(o => o.CreateAt >= today)
                .Sum(o => o.TotalAmount);

            var weekRevenue = orders
                .Where(o => o.CreateAt >= startOfWeek)
                .Sum(o => o.TotalAmount);

            var monthRevenue = orders
                .Where(o => o.CreateAt >= startOfMonth)
                .Sum(o => o.TotalAmount);

            var lastMonthRevenue = orders
                .Where(o => o.CreateAt >= startOfLastMonth && o.CreateAt < startOfMonth)
                .Sum(o => o.TotalAmount);

            // Order counts
            var todayOrders = orders.Count(o => o.CreateAt >= today);
            var pendingOrders = orders.Count(o => 
                o.Status == OrderStatus.Pending || o.Status == OrderStatus.Confirmed);

            // Stock alerts - check products below MinStockLevel
            var products = await _unitOfWork.Products.FindAsync(p => 
                p.StockQuantity <= p.MinStockLevel && p.IsActive);
            var lowStockCount = products.Count();

            // Customer count
            var customerCount = await _unitOfWork.Customers.CountAsync(c => !c.IsDeleted);

            // Invoice stats
            var invoices = await _unitOfWork.Invoices.GetAllAsync();
            var overdueInvoices = invoices.Count(i => 
                i.Status == InvoiceStatus.Unpaid && i.DueDate < DateTime.UtcNow);

            // Growth calculation
            var growthPercentage = lastMonthRevenue > 0 
                ? ((monthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100 
                : 100;

            return new DashboardSummary
            {
                Revenue = new RevenueSummary
                {
                    Today = todayRevenue,
                    ThisWeek = weekRevenue,
                    ThisMonth = monthRevenue,
                    LastMonth = lastMonthRevenue,
                    GrowthPercentage = Math.Round(growthPercentage, 2)
                },
                Orders = new OrderSummary
                {
                    TodayCount = todayOrders,
                    PendingCount = pendingOrders,
                    TotalThisMonth = orders.Count(o => o.CreateAt >= startOfMonth)
                },
                Inventory = new InventorySummary
                {
                    LowStockCount = lowStockCount,
                    TotalProducts = await _unitOfWork.Products.CountAsync(p => !p.IsDeleted)
                },
                Customers = new CustomerSummary
                {
                    TotalCount = customerCount,
                    NewThisMonth = await _unitOfWork.Customers.CountAsync(c => 
                        c.CreateAt >= startOfMonth && !c.IsDeleted)
                },
                Invoices = new InvoiceSummary
                {
                    OverdueCount = overdueInvoices,
                    TotalUnpaid = invoices
                        .Where(i => i.Status != InvoiceStatus.Paid)
                        .Sum(i => i.TotalAmount)
                },
                GeneratedAt = DateTime.UtcNow
            };
        }

        #endregion

        #region Chart Data

        /// <summary>
        /// Lấy dữ liệu biểu đồ doanh thu theo thời gian.
        /// </summary>
        public async Task<List<ChartDataPoint>> GetRevenueChartAsync(ChartPeriod period)
        {
            var (startDate, groupBy) = GetDateRange(period);

            var orders = await _unitOfWork.Orders.FindAsync(o => 
                o.CreateAt >= startDate && o.Status != OrderStatus.Cancelled);

            var grouped = orders
                .GroupBy(o => groupBy(o.CreateAt))
                .Select(g => new ChartDataPoint
                {
                    Label = g.Key,
                    Value = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(p => p.Label)
                .ToList();

            return grouped;
        }

        /// <summary>
        /// Lấy dữ liệu biểu đồ đơn hàng theo thời gian.
        /// </summary>
        public async Task<List<ChartDataPoint>> GetOrdersChartAsync(ChartPeriod period)
        {
            var (startDate, groupBy) = GetDateRange(period);

            var orders = await _unitOfWork.Orders.FindAsync(o => o.CreateAt >= startDate);

            var grouped = orders
                .GroupBy(o => groupBy(o.CreateAt))
                .Select(g => new ChartDataPoint
                {
                    Label = g.Key,
                    Value = g.Count()
                })
                .OrderBy(p => p.Label)
                .ToList();

            return grouped;
        }

        /// <summary>
        /// Lấy dữ liệu biểu đồ đơn hàng theo trạng thái (Pie chart).
        /// </summary>
        public async Task<List<ChartDataPoint>> GetOrdersByStatusAsync()
        {
            var orders = await _unitOfWork.Orders.GetAllAsync();

            return orders
                .GroupBy(o => o.Status)
                .Select(g => new ChartDataPoint
                {
                    Label = g.Key.ToString(),
                    Value = g.Count()
                })
                .ToList();
        }

        /// <summary>
        /// Lấy dữ liệu top sản phẩm bán chạy.
        /// </summary>
        public async Task<List<TopProductDto>> GetTopProductsAsync(int count = 10)
        {
            var orderItems = await _unitOfWork.OrderItems.FindAsync(
                oi => !oi.Order.IsDeleted,
                o => o.Order, o => o.Product);

            return orderItems
                .Where(oi => oi.Order.Status != OrderStatus.Cancelled)
                .GroupBy(oi => oi.ProductId)
                .Select(g => new TopProductDto
                {
                    ProductId = g.Key,
                    ProductName = g.First().Product?.Name ?? "Unknown",
                    SKU = g.First().Product?.SKU ?? "",
                    TotalQuantity = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .OrderByDescending(p => p.TotalRevenue)
                .Take(count)
                .ToList();
        }

        /// <summary>
        /// Lấy dữ liệu top khách hàng.
        /// </summary>
        public async Task<List<TopCustomerDto>> GetTopCustomersAsync(int count = 10)
        {
            var customers = await _unitOfWork.Customers.FindAsync(c => !c.IsDeleted);

            return customers
                .Select(c => new TopCustomerDto
                {
                    CustomerId = c.Id,
                    CustomerName = c.Name,
                    Email = c.Email,
                    TotalSpent = c.TotalSpending
                })
                .OrderByDescending(c => c.TotalSpent)
                .Take(count)
                .ToList();
        }

        #endregion

        #region AI Usage

        /// <summary>
        /// Lấy thống kê sử dụng AI.
        /// </summary>
        public async Task<AIUsageSummary> GetAIUsageAsync()
        {
            var documents = await _unitOfWork.KnowledgeDocuments.GetAllAsync();
            var segments = await _unitOfWork.DocumentSegments.GetAllAsync();
            var chatHistories = await _unitOfWork.ChatHistories.GetAllAsync();

            var today = DateTime.UtcNow.Date;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            return new AIUsageSummary
            {
                TotalDocuments = documents.Count(),
                TotalChunks = segments.Count(),
                TotalChats = chatHistories.Count(),
                ChatsToday = chatHistories.Count(c => c.CreateAt >= today),
                ChatsThisMonth = chatHistories.Count(c => c.CreateAt >= startOfMonth)
            };
        }

        #endregion

        #region Alerts

        /// <summary>
        /// Lấy cảnh báo: sản phẩm tồn kho thấp, hóa đơn quá hạn.
        /// </summary>
        public async Task<DashboardAlerts> GetAlertsAsync()
        {
            // Low stock products
            var lowStockProducts = await _unitOfWork.Products.FindAsync(p => 
                p.StockQuantity <= p.MinStockLevel && p.IsActive && !p.IsDeleted);

            var lowStockAlerts = lowStockProducts.Select(p => new LowStockAlert
            {
                ProductId = p.Id,
                ProductName = p.Name,
                SKU = p.SKU,
                CurrentStock = p.StockQuantity,
                MinStockLevel = p.MinStockLevel,
                Severity = p.StockQuantity == 0 ? "critical" : "warning"
            }).OrderBy(a => a.CurrentStock).Take(20).ToList();

            // Overdue invoices
            var overdueInvoices = await _unitOfWork.Invoices.FindAsync(i => 
                i.Status == InvoiceStatus.Unpaid && i.DueDate.HasValue && i.DueDate.Value < DateTime.UtcNow);

            var overdueAlerts = overdueInvoices
                .Where(i => i.DueDate.HasValue)
                .OrderBy(i => i.DueDate)
                .Take(20)
                .Select(i => new OverdueInvoiceAlert
                {
                    InvoiceId = i.Id,
                    InvoiceNumber = i.InvoiceNumber,
                    DueDate = i.DueDate!.Value,
                    DaysOverdue = (int)(DateTime.UtcNow - i.DueDate!.Value).TotalDays,
                    Amount = i.TotalAmount
                }).ToList();

            return new DashboardAlerts
            {
                LowStockAlerts = lowStockAlerts,
                OverdueInvoiceAlerts = overdueAlerts,
                TotalLowStockCount = lowStockProducts.Count(),
                TotalOverdueCount = overdueInvoices.Count(),
                GeneratedAt = DateTime.UtcNow
            };
        }

        #endregion

        #region Period Comparison

        /// <summary>
        /// So sánh hiệu suất giữa các khoảng thời gian.
        /// </summary>
        public async Task<PeriodComparison> GetPeriodComparisonAsync(ChartPeriod period)
        {
            var today = DateTime.UtcNow.Date;
            DateTime currentStart, previousStart, previousEnd;

            switch (period)
            {
                case ChartPeriod.Week:
                    currentStart = today.AddDays(-7);
                    previousEnd = currentStart;
                    previousStart = previousEnd.AddDays(-7);
                    break;
                case ChartPeriod.Month:
                    currentStart = today.AddMonths(-1);
                    previousEnd = currentStart;
                    previousStart = previousEnd.AddMonths(-1);
                    break;
                case ChartPeriod.Quarter:
                    currentStart = today.AddMonths(-3);
                    previousEnd = currentStart;
                    previousStart = previousEnd.AddMonths(-3);
                    break;
                case ChartPeriod.Year:
                default:
                    currentStart = today.AddYears(-1);
                    previousEnd = currentStart;
                    previousStart = previousEnd.AddYears(-1);
                    break;
            }

            var orders = await _unitOfWork.Orders.FindAsync(o => 
                o.CreateAt >= previousStart && o.Status != OrderStatus.Cancelled);

            var currentPeriodOrders = orders.Where(o => o.CreateAt >= currentStart).ToList();
            var previousPeriodOrders = orders.Where(o => o.CreateAt >= previousStart && o.CreateAt < previousEnd).ToList();

            var currentRevenue = currentPeriodOrders.Sum(o => o.TotalAmount);
            var previousRevenue = previousPeriodOrders.Sum(o => o.TotalAmount);
            var revenueChange = previousRevenue > 0 
                ? ((currentRevenue - previousRevenue) / previousRevenue) * 100 
                : 100;

            var currentOrderCount = currentPeriodOrders.Count;
            var previousOrderCount = previousPeriodOrders.Count;
            var orderChange = previousOrderCount > 0 
                ? ((decimal)(currentOrderCount - previousOrderCount) / previousOrderCount) * 100 
                : 100;

            var customers = await _unitOfWork.Customers.FindAsync(c => 
                c.CreateAt >= previousStart && !c.IsDeleted);
            var currentCustomers = customers.Count(c => c.CreateAt >= currentStart);
            var previousCustomers = customers.Count(c => c.CreateAt >= previousStart && c.CreateAt < previousEnd);
            var customerChange = previousCustomers > 0 
                ? ((decimal)(currentCustomers - previousCustomers) / previousCustomers) * 100 
                : 100;

            return new PeriodComparison
            {
                Period = period.ToString(),
                CurrentPeriodStart = currentStart,
                PreviousPeriodStart = previousStart,
                Revenue = new MetricComparison
                {
                    Current = currentRevenue,
                    Previous = previousRevenue,
                    ChangePercentage = Math.Round(revenueChange, 2),
                    Trend = currentRevenue >= previousRevenue ? "up" : "down"
                },
                Orders = new MetricComparison
                {
                    Current = currentOrderCount,
                    Previous = previousOrderCount,
                    ChangePercentage = Math.Round(orderChange, 2),
                    Trend = currentOrderCount >= previousOrderCount ? "up" : "down"
                },
                NewCustomers = new MetricComparison
                {
                    Current = currentCustomers,
                    Previous = previousCustomers,
                    ChangePercentage = Math.Round(customerChange, 2),
                    Trend = currentCustomers >= previousCustomers ? "up" : "down"
                }
            };
        }

        #endregion

        #region Private Helpers

        private (DateTime startDate, Func<DateTime, string> groupBy) GetDateRange(ChartPeriod period)
        {
            var today = DateTime.UtcNow.Date;

            return period switch
            {
                ChartPeriod.Week => (today.AddDays(-7), d => d.ToString("yyyy-MM-dd")),
                ChartPeriod.Month => (today.AddMonths(-1), d => d.ToString("yyyy-MM-dd")),
                ChartPeriod.Quarter => (today.AddMonths(-3), d => $"W{GetWeekOfYear(d)}"),
                ChartPeriod.Year => (today.AddYears(-1), d => d.ToString("yyyy-MM")),
                _ => (today.AddDays(-7), d => d.ToString("yyyy-MM-dd"))
            };
        }

        private int GetWeekOfYear(DateTime date)
        {
            return System.Globalization.CultureInfo.CurrentCulture.Calendar
                .GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek,
                    DayOfWeek.Monday);
        }

        #endregion
    }

    #region DTOs

    public class DashboardSummary
    {
        public RevenueSummary Revenue { get; set; } = new();
        public OrderSummary Orders { get; set; } = new();
        public InventorySummary Inventory { get; set; } = new();
        public CustomerSummary Customers { get; set; } = new();
        public InvoiceSummary Invoices { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }

    public class RevenueSummary
    {
        public decimal Today { get; set; }
        public decimal ThisWeek { get; set; }
        public decimal ThisMonth { get; set; }
        public decimal LastMonth { get; set; }
        public decimal GrowthPercentage { get; set; }
    }

    public class OrderSummary
    {
        public int TodayCount { get; set; }
        public int PendingCount { get; set; }
        public int TotalThisMonth { get; set; }
    }

    public class InventorySummary
    {
        public int LowStockCount { get; set; }
        public int TotalProducts { get; set; }
    }

    public class CustomerSummary
    {
        public int TotalCount { get; set; }
        public int NewThisMonth { get; set; }
    }

    public class InvoiceSummary
    {
        public int OverdueCount { get; set; }
        public decimal TotalUnpaid { get; set; }
    }

    public class ChartDataPoint
    {
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }

    public enum ChartPeriod
    {
        Week,
        Month,
        Quarter,
        Year
    }

    public class TopProductDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class TopCustomerDto
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class AIUsageSummary
    {
        public int TotalDocuments { get; set; }
        public int TotalChunks { get; set; }
        public int TotalChats { get; set; }
        public int ChatsToday { get; set; }
        public int ChatsThisMonth { get; set; }
    }

    public class DashboardAlerts
    {
        public List<LowStockAlert> LowStockAlerts { get; set; } = new();
        public List<OverdueInvoiceAlert> OverdueInvoiceAlerts { get; set; } = new();
        public int TotalLowStockCount { get; set; }
        public int TotalOverdueCount { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class LowStockAlert
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int MinStockLevel { get; set; }
        public string Severity { get; set; } = "warning";
    }

    public class OverdueInvoiceAlert
    {
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int DaysOverdue { get; set; }
        public decimal Amount { get; set; }
    }

    public class PeriodComparison
    {
        public string Period { get; set; } = string.Empty;
        public DateTime CurrentPeriodStart { get; set; }
        public DateTime PreviousPeriodStart { get; set; }
        public MetricComparison Revenue { get; set; } = new();
        public MetricComparison Orders { get; set; } = new();
        public MetricComparison NewCustomers { get; set; } = new();
    }

    public class MetricComparison
    {
        public decimal Current { get; set; }
        public decimal Previous { get; set; }
        public decimal ChangePercentage { get; set; }
        public string Trend { get; set; } = "up";
    }

    #endregion
}
