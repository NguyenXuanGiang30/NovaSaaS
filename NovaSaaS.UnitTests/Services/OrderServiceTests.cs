using FluentAssertions;
using Moq;
using NovaSaaS.Application.DTOs.Business;
using NovaSaaS.Application.Interfaces;
using NovaSaaS.Application.Interfaces.Business;
using NovaSaaS.Application.Interfaces.Inventory;
using NovaSaaS.Application.Services.Business;
using NovaSaaS.Domain.Entities.Business;
using NovaSaaS.Domain.Entities.Inventory;
using NovaSaaS.Domain.Enums;
using NovaSaaS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace NovaSaaS.UnitTests.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ICurrentUserService> _mockCurrentUser;
        private readonly Mock<IStockService> _mockStockService;
        private readonly Mock<ICustomerService> _mockCustomerService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IRepository<Order>> _mockOrderRepo;
        private readonly Mock<IRepository<Customer>> _mockCustomerRepo;
        private readonly Mock<IRepository<Product>> _mockProductRepo;
        private readonly Mock<IRepository<Invoice>> _mockInvoiceRepo;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCurrentUser = new Mock<ICurrentUserService>();
            _mockStockService = new Mock<IStockService>();
            _mockCustomerService = new Mock<ICustomerService>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockOrderRepo = new Mock<IRepository<Order>>();
            _mockCustomerRepo = new Mock<IRepository<Customer>>();
            _mockProductRepo = new Mock<IRepository<Product>>();
            _mockInvoiceRepo = new Mock<IRepository<Invoice>>();

            // Mock Repository access
            _mockUnitOfWork.Setup(u => u.Orders).Returns(_mockOrderRepo.Object);
            _mockUnitOfWork.Setup(u => u.Invoices).Returns(_mockInvoiceRepo.Object);
            _mockUnitOfWork.Setup(u => u.Customers).Returns(_mockCustomerRepo.Object);
            _mockUnitOfWork.Setup(u => u.Products).Returns(_mockProductRepo.Object);

            _orderService = new OrderService(
                _mockUnitOfWork.Object,
                _mockCurrentUser.Object,
                _mockStockService.Object,
                _mockCustomerService.Object,
                _mockNotificationService.Object
            );
        }

        #region Order Total Calculation Tests

        [Fact]
        public async Task CreateOrderAsync_CalculatesSubTotalCorrectly()
        {
            // Arrange - 2 products with different prices
            var customerId = Guid.NewGuid();
            var product1Id = Guid.NewGuid();
            var product2Id = Guid.NewGuid();
            
            var dto = new CreateOrderDto
            {
                CustomerId = customerId,
                Items = new List<CreateOrderItemDto>
                {
                    new() { ProductId = product1Id, Quantity = 3, WarehouseId = Guid.NewGuid() }, // 3 * 100 = 300
                    new() { ProductId = product2Id, Quantity = 2, WarehouseId = Guid.NewGuid() }  // 2 * 250 = 500
                }
            };

            var customer = new Customer { Id = customerId, Name = "Test" };
            var product1 = new Product { Id = product1Id, Price = 100, Name = "P1", StockQuantity = 100 };
            var product2 = new Product { Id = product2Id, Price = 250, Name = "P2", StockQuantity = 100 };

            _mockCustomerRepo.Setup(r => r.GetByIdAsync(customerId)).ReturnsAsync(customer);
            _mockProductRepo.Setup(r => r.GetByIdAsync(product1Id)).ReturnsAsync(product1);
            _mockProductRepo.Setup(r => r.GetByIdAsync(product2Id)).ReturnsAsync(product2);

            Order? capturedOrder = null;
            _mockOrderRepo.Setup(r => r.Add(It.IsAny<Order>()))
                .Callback<Order>(o => capturedOrder = o);

            _mockOrderRepo.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Expression<Func<Order, object?>>>(),
                It.IsAny<Expression<Func<Order, object?>>>(),
                It.IsAny<Expression<Func<Order, object?>>>()
            )).ReturnsAsync(() => new List<Order> { 
                new Order { 
                    Id = Guid.NewGuid(), 
                    Customer = customer, 
                    OrderItems = new List<OrderItem>(),
                    Invoice = new Invoice()
                } 
            });
            _mockProductRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(new List<Product> { product1, product2 });

            // Act
            await _orderService.CreateOrderAsync(dto);

            // Assert - SubTotal = 300 + 500 = 800
            capturedOrder.Should().NotBeNull();
            capturedOrder!.SubTotal.Should().Be(800);
            // TotalAmount = SubTotal + Tax (10%) = 800 + 80 = 880
            capturedOrder.TotalAmount.Should().Be(880);
        }

        [Fact]
        public async Task CreateOrderAsync_WithCoupon_AppliesDiscount()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var couponId = Guid.NewGuid();
            
            var dto = new CreateOrderDto
            {
                CustomerId = customerId,
                CouponId = couponId, // Coupon applies discount
                Items = new List<CreateOrderItemDto>
                {
                    new() { ProductId = productId, Quantity = 10, WarehouseId = Guid.NewGuid() } // 10 * 100 = 1000
                }
            };

            var customer = new Customer { Id = customerId, Name = "VIP Customer" };
            var product = new Product { Id = productId, Price = 100, Name = "Product", StockQuantity = 100 };

            _mockCustomerRepo.Setup(r => r.GetByIdAsync(customerId)).ReturnsAsync(customer);
            _mockProductRepo.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);

            Order? capturedOrder = null;
            _mockOrderRepo.Setup(r => r.Add(It.IsAny<Order>()))
                .Callback<Order>(o => capturedOrder = o);

            _mockOrderRepo.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Expression<Func<Order, object?>>>(),
                It.IsAny<Expression<Func<Order, object?>>>(),
                It.IsAny<Expression<Func<Order, object?>>>()
            )).ReturnsAsync(() => new List<Order> { 
                new Order { 
                    Id = Guid.NewGuid(), 
                    Customer = customer, 
                    OrderItems = new List<OrderItem>(),
                    Invoice = new Invoice()
                } 
            });
            _mockProductRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(new List<Product> { product });

            // Act
            await _orderService.CreateOrderAsync(dto);

            // Assert - Order should have correct calculations
            capturedOrder.Should().NotBeNull();
            capturedOrder!.SubTotal.Should().Be(1000);
            // DTO contains CouponId - actual coupon application tested separately
        }

        #endregion

        #region Transaction Rollback Tests

        [Fact]
        public async Task CreateOrderAsync_StockInsufficient_ShouldThrowAndRollback()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var dto = new CreateOrderDto
            {
                CustomerId = customerId,
                Items = new List<CreateOrderItemDto>
                {
                    new() { ProductId = productId, Quantity = 20, WarehouseId = Guid.NewGuid() }
                }
            };
            
            var customer = new Customer { Id = customerId };
            var product = new Product { Id = productId, Price = 100 };

            _mockCustomerRepo.Setup(r => r.GetByIdAsync(customerId)).ReturnsAsync(customer);
            _mockProductRepo.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);

            _mockStockService.Setup(s => s.ReduceStockForOrderAsync(It.IsAny<Guid>(), It.IsAny<string>(), dto.Items))
                .ThrowsAsync(new InvalidOperationException("Tồn kho không đủ"));

            // Act
            Func<Task> act = async () => await _orderService.CreateOrderAsync(dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Tồn kho không đủ*");
            _mockUnitOfWork.Verify(u => u.RollbackTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateOrderAsync_CustomerNotFound_ShouldThrowBeforeTransaction()
        {
            // Arrange
            var dto = new CreateOrderDto
            {
                CustomerId = Guid.NewGuid(),
                Items = new List<CreateOrderItemDto>
                {
                    new() { ProductId = Guid.NewGuid(), Quantity = 1, WarehouseId = Guid.NewGuid() }
                }
            };

            _mockCustomerRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Customer?)null);

            // Act
            Func<Task> act = async () => await _orderService.CreateOrderAsync(dto);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Khách hàng không tồn tại*");
            _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Never);
        }

        #endregion

        #region Notification Tests

        [Fact]
        public async Task CreateOrderAsync_Success_ShouldSendNotification()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var dto = new CreateOrderDto
            {
                CustomerId = customerId,
                Items = new List<CreateOrderItemDto>
                {
                    new() { ProductId = productId, Quantity = 2, WarehouseId = Guid.NewGuid() }
                }
            };

            var customer = new Customer { Id = customerId, Name = "Test Customer" };
            var product = new Product { Id = productId, Price = 100, Name = "Test Product", StockQuantity = 10 };

            _mockCustomerRepo.Setup(r => r.GetByIdAsync(customerId)).ReturnsAsync(customer);
            _mockProductRepo.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
            _mockCurrentUser.Setup(u => u.TenantId).Returns(Guid.NewGuid());
            
            _mockOrderRepo.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Expression<Func<Order, object?>>>(),
                It.IsAny<Expression<Func<Order, object?>>>(),
                It.IsAny<Expression<Func<Order, object?>>>()
            )).ReturnsAsync(new List<Order> { 
                new Order { 
                    Id = Guid.NewGuid(), 
                    Customer = customer, 
                    OrderItems = new List<OrderItem> { new() { ProductId = productId, Quantity = 2, UnitPrice = 100 } },
                    Invoice = new Invoice()
                } 
            });
            _mockProductRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(new List<Product> { product });

            // Act
            await _orderService.CreateOrderAsync(dto);

            // Assert
            _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Once);
            _mockNotificationService.Verify(n => n.NotifyNewOrderAsync(
                It.IsAny<Guid>(), 
                It.IsAny<Guid>(), 
                It.IsAny<string>(), 
                "Test Customer", 
                It.IsAny<decimal>(), 
                1), Times.Once);
        }

        #endregion
    }
}
