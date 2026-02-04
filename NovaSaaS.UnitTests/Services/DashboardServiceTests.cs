using FluentAssertions;
using Moq;
using NovaSaaS.Application.Interfaces;
using NovaSaaS.Application.Services.Analytics;
using NovaSaaS.Domain.Entities.Business;
using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Entities.Inventory;
using NovaSaaS.Domain.Entities.AI;
using NovaSaaS.Domain.Enums;
using NovaSaaS.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace NovaSaaS.UnitTests.Services
{
    /// <summary>
    /// Unit Tests for DashboardService.
    /// Tests analytics calculations, chart data, and alert generation.
    /// </summary>
    public class DashboardServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ICurrentUserService> _mockCurrentUser;
        private readonly Mock<ILogger<DashboardService>> _mockLogger;
        private readonly DashboardService _dashboardService;

        public DashboardServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCurrentUser = new Mock<ICurrentUserService>();
            _mockLogger = new Mock<ILogger<DashboardService>>();

            _dashboardService = new DashboardService(
                _mockUnitOfWork.Object,
                _mockCurrentUser.Object,
                _mockLogger.Object);
        }

        #region GetSummaryAsync Tests

        [Fact]
        public async Task GetSummaryAsync_WithOrders_CalculatesRevenueCorrectly()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;
            var orders = new List<Order>
            {
                new Order { Id = Guid.NewGuid(), TotalAmount = 1000m, CreateAt = today, Status = OrderStatus.Completed },
                new Order { Id = Guid.NewGuid(), TotalAmount = 2000m, CreateAt = today, Status = OrderStatus.Completed },
                new Order { Id = Guid.NewGuid(), TotalAmount = 500m, CreateAt = today.AddDays(-3), Status = OrderStatus.Completed },
            };

            SetupOrdersRepository(orders);
            SetupProductsRepository(new List<Product>());
            SetupCustomersRepository(new List<Customer>());
            SetupInvoicesRepository(new List<Invoice>());

            // Act
            var result = await _dashboardService.GetSummaryAsync();

            // Assert
            result.Revenue.Today.Should().Be(3000m);
            result.Orders.TodayCount.Should().Be(2);
        }

        [Fact]
        public async Task GetSummaryAsync_WithNoOrders_ReturnsZeroValues()
        {
            // Arrange
            SetupOrdersRepository(new List<Order>());
            SetupProductsRepository(new List<Product>());
            SetupCustomersRepository(new List<Customer>());
            SetupInvoicesRepository(new List<Invoice>());

            // Act
            var result = await _dashboardService.GetSummaryAsync();

            // Assert
            result.Revenue.Today.Should().Be(0);
            result.Revenue.ThisWeek.Should().Be(0);
            result.Revenue.ThisMonth.Should().Be(0);
            result.Orders.TodayCount.Should().Be(0);
        }

        [Fact]
        public async Task GetSummaryAsync_ExcludesCancelledOrders()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;
            var orders = new List<Order>
            {
                new Order { Id = Guid.NewGuid(), TotalAmount = 1000m, CreateAt = today, Status = OrderStatus.Completed },
                new Order { Id = Guid.NewGuid(), TotalAmount = 5000m, CreateAt = today, Status = OrderStatus.Cancelled },
            };

            SetupOrdersRepository(orders);
            SetupProductsRepository(new List<Product>());
            SetupCustomersRepository(new List<Customer>());
            SetupInvoicesRepository(new List<Invoice>());

            // Act
            var result = await _dashboardService.GetSummaryAsync();

            // Assert
            result.Revenue.Today.Should().Be(1000m); // Cancelled order excluded
            result.Orders.TodayCount.Should().Be(1);
        }

        [Fact]
        public async Task GetSummaryAsync_CalculatesGrowthPercentage()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var lastMonth = startOfMonth.AddMonths(-1);

            var orders = new List<Order>
            {
                // This month: 2000
                new Order { Id = Guid.NewGuid(), TotalAmount = 2000m, CreateAt = startOfMonth.AddDays(1), Status = OrderStatus.Completed },
                // Last month: 1000
                new Order { Id = Guid.NewGuid(), TotalAmount = 1000m, CreateAt = lastMonth.AddDays(5), Status = OrderStatus.Completed },
            };

            SetupOrdersRepository(orders);
            SetupProductsRepository(new List<Product>());
            SetupCustomersRepository(new List<Customer>());
            SetupInvoicesRepository(new List<Invoice>());

            // Act
            var result = await _dashboardService.GetSummaryAsync();

            // Assert
            // Growth = ((2000 - 1000) / 1000) * 100 = 100%
            result.Revenue.GrowthPercentage.Should().Be(100m);
        }

        #endregion

        #region GetAlertsAsync Tests

        [Fact]
        public async Task GetAlertsAsync_WithLowStockProducts_ReturnsAlerts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = Guid.NewGuid(), Name = "Product A", SKU = "SKU001", StockQuantity = 5, MinStockLevel = 10, IsActive = true, IsDeleted = false },
                new Product { Id = Guid.NewGuid(), Name = "Product B", SKU = "SKU002", StockQuantity = 0, MinStockLevel = 5, IsActive = true, IsDeleted = false },
                new Product { Id = Guid.NewGuid(), Name = "Product C", SKU = "SKU003", StockQuantity = 20, MinStockLevel = 10, IsActive = true, IsDeleted = false }, // Not low stock
            };

            SetupProductsRepositoryForAlerts(products);
            SetupInvoicesRepositoryForAlerts(new List<Invoice>());

            // Act
            var result = await _dashboardService.GetAlertsAsync();

            // Assert
            result.LowStockAlerts.Should().HaveCount(2);
            result.LowStockAlerts.Should().Contain(a => a.SKU == "SKU001" && a.Severity == "warning");
            result.LowStockAlerts.Should().Contain(a => a.SKU == "SKU002" && a.Severity == "critical");
        }

        [Fact]
        public async Task GetAlertsAsync_WithOverdueInvoices_ReturnsAlerts()
        {
            // Arrange
            var invoices = new List<Invoice>
            {
                new Invoice { Id = Guid.NewGuid(), InvoiceNumber = "INV001", DueDate = DateTime.UtcNow.AddDays(-5), Status = InvoiceStatus.Unpaid, TotalAmount = 1000m },
                new Invoice { Id = Guid.NewGuid(), InvoiceNumber = "INV002", DueDate = DateTime.UtcNow.AddDays(-10), Status = InvoiceStatus.Unpaid, TotalAmount = 2000m },
                new Invoice { Id = Guid.NewGuid(), InvoiceNumber = "INV003", DueDate = DateTime.UtcNow.AddDays(5), Status = InvoiceStatus.Unpaid, TotalAmount = 500m }, // Not overdue
            };

            SetupProductsRepositoryForAlerts(new List<Product>());
            SetupInvoicesRepositoryForAlerts(invoices);

            // Act
            var result = await _dashboardService.GetAlertsAsync();

            // Assert
            result.OverdueInvoiceAlerts.Should().HaveCount(2);
            result.TotalOverdueCount.Should().Be(2);
        }

        [Fact]
        public async Task GetAlertsAsync_WithNoIssues_ReturnsEmptyAlerts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = Guid.NewGuid(), Name = "Product A", StockQuantity = 50, MinStockLevel = 10, IsActive = true, IsDeleted = false },
            };

            var invoices = new List<Invoice>
            {
                new Invoice { Id = Guid.NewGuid(), InvoiceNumber = "INV001", DueDate = DateTime.UtcNow.AddDays(5), Status = InvoiceStatus.Unpaid, TotalAmount = 1000m },
            };

            SetupProductsRepositoryForAlerts(products);
            SetupInvoicesRepositoryForAlerts(invoices);

            // Act
            var result = await _dashboardService.GetAlertsAsync();

            // Assert
            result.LowStockAlerts.Should().BeEmpty();
            result.OverdueInvoiceAlerts.Should().BeEmpty();
        }

        #endregion

        #region GetPeriodComparisonAsync Tests

        [Fact]
        public async Task GetPeriodComparisonAsync_WeekPeriod_CalculatesCorrectly()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;
            var orders = new List<Order>
            {
                // Current week: 1500
                new Order { Id = Guid.NewGuid(), TotalAmount = 1000m, CreateAt = today.AddDays(-3), Status = OrderStatus.Completed },
                new Order { Id = Guid.NewGuid(), TotalAmount = 500m, CreateAt = today.AddDays(-5), Status = OrderStatus.Completed },
                // Previous week: 1000
                new Order { Id = Guid.NewGuid(), TotalAmount = 1000m, CreateAt = today.AddDays(-10), Status = OrderStatus.Completed },
            };

            SetupOrdersRepository(orders);
            SetupCustomersRepository(new List<Customer>());

            // Act
            var result = await _dashboardService.GetPeriodComparisonAsync(ChartPeriod.Week);

            // Assert
            result.Period.Should().Be("Week");
            result.Revenue.Current.Should().Be(1500m);
            result.Revenue.Previous.Should().Be(1000m);
            result.Revenue.ChangePercentage.Should().Be(50m); // ((1500 - 1000) / 1000) * 100
            result.Revenue.Trend.Should().Be("up");
        }

        [Fact]
        public async Task GetPeriodComparisonAsync_NoPreviousData_Returns100Percent()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;
            var orders = new List<Order>
            {
                new Order { Id = Guid.NewGuid(), TotalAmount = 1000m, CreateAt = today.AddDays(-3), Status = OrderStatus.Completed },
            };

            SetupOrdersRepository(orders);
            SetupCustomersRepository(new List<Customer>());

            // Act
            var result = await _dashboardService.GetPeriodComparisonAsync(ChartPeriod.Week);

            // Assert
            result.Revenue.ChangePercentage.Should().Be(100m); // No previous data = 100% growth
        }

        [Fact]
        public async Task GetPeriodComparisonAsync_Decline_ShowsDownTrend()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;
            var orders = new List<Order>
            {
                // Current week: 500
                new Order { Id = Guid.NewGuid(), TotalAmount = 500m, CreateAt = today.AddDays(-3), Status = OrderStatus.Completed },
                // Previous week: 1000
                new Order { Id = Guid.NewGuid(), TotalAmount = 1000m, CreateAt = today.AddDays(-10), Status = OrderStatus.Completed },
            };

            SetupOrdersRepository(orders);
            SetupCustomersRepository(new List<Customer>());

            // Act
            var result = await _dashboardService.GetPeriodComparisonAsync(ChartPeriod.Week);

            // Assert
            result.Revenue.ChangePercentage.Should().Be(-50m);
            result.Revenue.Trend.Should().Be("down");
        }

        #endregion

        #region GetTopProductsAsync Tests

        [Fact]
        public async Task GetTopProductsAsync_ReturnsOrderedByRevenue()
        {
            // Arrange
            var product1 = new Product { Id = Guid.NewGuid(), Name = "Product A", SKU = "SKU001" };
            var product2 = new Product { Id = Guid.NewGuid(), Name = "Product B", SKU = "SKU002" };

            var orderItems = new List<OrderItem>
            {
                new OrderItem { ProductId = product1.Id, Product = product1, Quantity = 10, UnitPrice = 100m, Order = new Order { Status = OrderStatus.Completed, IsDeleted = false } },
                new OrderItem { ProductId = product2.Id, Product = product2, Quantity = 5, UnitPrice = 500m, Order = new Order { Status = OrderStatus.Completed, IsDeleted = false } },
            };

            _mockUnitOfWork.Setup(u => u.OrderItems.FindAsync(
                It.IsAny<Expression<Func<OrderItem, bool>>>(),
                It.IsAny<Expression<Func<OrderItem, object>>[]>()))
                .ReturnsAsync(orderItems);

            // Act
            var result = await _dashboardService.GetTopProductsAsync(10);

            // Assert
            result.Should().HaveCount(2);
            result[0].ProductName.Should().Be("Product B"); // Higher revenue: 5 * 500 = 2500
            result[1].ProductName.Should().Be("Product A"); // Lower revenue: 10 * 100 = 1000
        }

        #endregion

        #region GetAIUsageAsync Tests

        [Fact]
        public async Task GetAIUsageAsync_CountsCorrectly()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            var documents = new List<KnowledgeDocument>
            {
                new KnowledgeDocument { Id = Guid.NewGuid() },
                new KnowledgeDocument { Id = Guid.NewGuid() },
            };

            var segments = new List<DocumentSegment>
            {
                new DocumentSegment { Id = Guid.NewGuid() },
                new DocumentSegment { Id = Guid.NewGuid() },
                new DocumentSegment { Id = Guid.NewGuid() },
            };

            var chatHistories = new List<ChatHistory>
            {
                new ChatHistory { Id = Guid.NewGuid(), CreateAt = today },
                new ChatHistory { Id = Guid.NewGuid(), CreateAt = today.AddDays(-2) },
                new ChatHistory { Id = Guid.NewGuid(), CreateAt = today.AddMonths(-2) },
            };

            _mockUnitOfWork.Setup(u => u.KnowledgeDocuments.GetAllAsync())
                .ReturnsAsync(documents);
            _mockUnitOfWork.Setup(u => u.DocumentSegments.GetAllAsync())
                .ReturnsAsync(segments);
            _mockUnitOfWork.Setup(u => u.ChatHistories.GetAllAsync())
                .ReturnsAsync(chatHistories);

            // Act
            var result = await _dashboardService.GetAIUsageAsync();

            // Assert
            result.TotalDocuments.Should().Be(2);
            result.TotalChunks.Should().Be(3);
            result.TotalChats.Should().Be(3);
            result.ChatsToday.Should().Be(1);
            result.ChatsThisMonth.Should().Be(2);
        }

        #endregion

        #region Helper Methods

        private void SetupOrdersRepository(List<Order> orders)
        {
            _mockUnitOfWork.Setup(u => u.Orders.FindAsync(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Expression<Func<Order, object>>[]>()))
                .ReturnsAsync(orders);
        }

        private void SetupProductsRepository(List<Product> products)
        {
            _mockUnitOfWork.Setup(u => u.Products.FindAsync(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<Expression<Func<Product, object>>[]>()))
                .ReturnsAsync(products);

            _mockUnitOfWork.Setup(u => u.Products.CountAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(products.Count);
        }

        private void SetupProductsRepositoryForAlerts(List<Product> products)
        {
            _mockUnitOfWork.Setup(u => u.Products.FindAsync(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<Expression<Func<Product, object>>[]>()))
                .ReturnsAsync(products);
        }

        private void SetupCustomersRepository(List<Customer> customers)
        {
            _mockUnitOfWork.Setup(u => u.Customers.FindAsync(
                It.IsAny<Expression<Func<Customer, bool>>>(),
                It.IsAny<Expression<Func<Customer, object>>[]>()))
                .ReturnsAsync(customers);

            _mockUnitOfWork.Setup(u => u.Customers.CountAsync(It.IsAny<Expression<Func<Customer, bool>>>()))
                .ReturnsAsync(customers.Count);
        }

        private void SetupInvoicesRepository(List<Invoice> invoices)
        {
            _mockUnitOfWork.Setup(u => u.Invoices.GetAllAsync())
                .ReturnsAsync(invoices);
        }

        private void SetupInvoicesRepositoryForAlerts(List<Invoice> invoices)
        {
            _mockUnitOfWork.Setup(u => u.Invoices.FindAsync(
                It.IsAny<Expression<Func<Invoice, bool>>>(),
                It.IsAny<Expression<Func<Invoice, object>>[]>()))
                .ReturnsAsync(invoices);
        }

        #endregion
    }
}
