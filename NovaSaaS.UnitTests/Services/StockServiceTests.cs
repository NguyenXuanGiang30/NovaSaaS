using FluentAssertions;
using Moq;
using NovaSaaS.Application.DTOs.Inventory;
using NovaSaaS.Application.Interfaces;
using NovaSaaS.Domain.Entities.Inventory;
using NovaSaaS.Domain.Interfaces;
using NovaSaaS.Application.Services.Inventory;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace NovaSaaS.UnitTests.Services
{
    public class StockServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ICurrentUserService> _mockCurrentUser;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IRepository<Stock>> _mockStockRepo;
        private readonly Mock<IRepository<StockMovement>> _mockMovementRepo;
        private readonly Mock<IRepository<Product>> _mockProductRepo;
        private readonly Mock<IRepository<Warehouse>> _mockWarehouseRepo;
        private readonly StockService _stockService;

        public StockServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCurrentUser = new Mock<ICurrentUserService>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockStockRepo = new Mock<IRepository<Stock>>();
            _mockMovementRepo = new Mock<IRepository<StockMovement>>();
            _mockProductRepo = new Mock<IRepository<Product>>();
            _mockWarehouseRepo = new Mock<IRepository<Warehouse>>();
            
            // Mock UnitOfWork properties
            _mockUnitOfWork.Setup(u => u.Repository<Stock>()).Returns(_mockStockRepo.Object);
            _mockUnitOfWork.Setup(u => u.StockMovements).Returns(_mockMovementRepo.Object);
            _mockUnitOfWork.Setup(u => u.Products).Returns(_mockProductRepo.Object);
            _mockUnitOfWork.Setup(u => u.Warehouses).Returns(_mockWarehouseRepo.Object);

            _stockService = new StockService(
                _mockUnitOfWork.Object,
                _mockCurrentUser.Object,
                _mockNotificationService.Object
            );
        }

        #region Stock Validation Tests

        [Fact]
        public async Task AdjustStockAsync_Outbound_InsufficientStock_ShouldThrow()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var warehouseId = Guid.NewGuid();
            var dto = new StockAdjustmentDto
            {
                ProductId = productId,
                WarehouseId = warehouseId,
                Quantity = 10,
                IsInbound = false // Xuất kho
            };

            var stock = new Stock { ProductId = productId, WarehouseId = warehouseId, Quantity = 5 }; // Chỉ có 5
            var product = new Product { Id = productId, Name = "Test Product" };
            var warehouse = new Warehouse { Id = warehouseId, Name = "Test Warehouse" };

            _mockProductRepo.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
            _mockWarehouseRepo.Setup(r => r.GetByIdAsync(warehouseId)).ReturnsAsync(warehouse);
            _mockStockRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Stock, bool>>>()))
                .ReturnsAsync(stock);

            // Act
            Func<Task> act = async () => await _stockService.AdjustStockAsync(dto);

            // Assert - ERR_BIZ_500: Tồn kho không đủ
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Tồn kho không đủ*");
        }

        [Fact]
        public async Task AdjustStockAsync_Outbound_NoExistingStock_ShouldThrow()
        {
            // Arrange
            var dto = new StockAdjustmentDto
            {
                ProductId = Guid.NewGuid(),
                WarehouseId = Guid.NewGuid(),
                Quantity = 5,
                IsInbound = false
            };

            var product = new Product { Id = dto.ProductId, Name = "Test" };
            var warehouse = new Warehouse { Id = dto.WarehouseId, Name = "WH" };

            _mockProductRepo.Setup(r => r.GetByIdAsync(dto.ProductId)).ReturnsAsync(product);
            _mockWarehouseRepo.Setup(r => r.GetByIdAsync(dto.WarehouseId)).ReturnsAsync(warehouse);
            _mockStockRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Stock, bool>>>()))
                .ReturnsAsync((Stock?)null);

            // Act
            Func<Task> act = async () => await _stockService.AdjustStockAsync(dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Không thể xuất kho khi chưa có tồn*");
        }

        #endregion

        #region Stock Update and Notification Tests

        [Fact]
        public async Task AdjustStockAsync_Inbound_ShouldUpdateAndNotify()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var warehouseId = Guid.NewGuid();
            var dto = new StockAdjustmentDto
            {
                ProductId = productId,
                WarehouseId = warehouseId,
                Quantity = 20,
                IsInbound = true // Nhập kho
            };

            var stock = new Stock { ProductId = productId, WarehouseId = warehouseId, Quantity = 10 };
            var product = new Product { Id = productId, Name = "Product", StockQuantity = 100, SKU = "SKU001" };
            var warehouse = new Warehouse { Id = warehouseId, Name = "Warehouse A" };

            _mockProductRepo.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
            _mockWarehouseRepo.Setup(r => r.GetByIdAsync(warehouseId)).ReturnsAsync(warehouse);
            _mockStockRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Stock, bool>>>()))
                .ReturnsAsync(stock);
            _mockCurrentUser.Setup(u => u.TenantId).Returns(Guid.NewGuid());

            // Act
            var result = await _stockService.AdjustStockAsync(dto);

            // Assert
            stock.Quantity.Should().Be(30); // 10 + 20
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
            _mockNotificationService.Verify(n => n.NotifyStockUpdatedAsync(
                It.IsAny<Guid>(), productId, "Product", "SKU001",
                warehouseId, "Warehouse A", 10, 30, It.IsAny<decimal>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task AdjustStockAsync_Outbound_SufficientStock_ShouldUpdateAndNotify()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var warehouseId = Guid.NewGuid();
            var dto = new StockAdjustmentDto
            {
                ProductId = productId,
                WarehouseId = warehouseId,
                Quantity = 5,
                IsInbound = false
            };

            var stock = new Stock { ProductId = productId, WarehouseId = warehouseId, Quantity = 10 };
            var product = new Product { Id = productId, Name = "Test Product", StockQuantity = 100 };
            var warehouse = new Warehouse { Id = warehouseId, Name = "Test Warehouse" };

            _mockProductRepo.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
            _mockWarehouseRepo.Setup(r => r.GetByIdAsync(warehouseId)).ReturnsAsync(warehouse);
            _mockStockRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Stock, bool>>>()))
                .ReturnsAsync(stock);

            // Act
            await _stockService.AdjustStockAsync(dto);

            // Assert
            stock.Quantity.Should().Be(5); // 10 - 5
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
            _mockNotificationService.Verify(n => n.NotifyStockUpdatedAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<decimal>(), 
                It.IsAny<decimal>(), It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region Transfer Stock Tests

        [Fact]
        public async Task TransferStockAsync_SameWarehouse_ShouldThrow()
        {
            // Arrange
            var warehouseId = Guid.NewGuid();
            var dto = new StockTransferDto
            {
                ProductId = Guid.NewGuid(),
                FromWarehouseId = warehouseId,
                ToWarehouseId = warehouseId, // Same warehouse
                Quantity = 10
            };

            // Act
            Func<Task> act = async () => await _stockService.TransferStockAsync(dto);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Kho nguồn và kho đích không được trùng nhau*");
        }

        [Fact]
        public async Task TransferStockAsync_InsufficientStock_ShouldRollback()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var fromWarehouseId = Guid.NewGuid();
            var toWarehouseId = Guid.NewGuid();
            
            var dto = new StockTransferDto
            {
                ProductId = productId,
                FromWarehouseId = fromWarehouseId,
                ToWarehouseId = toWarehouseId,
                Quantity = 100 // Request 100
            };

            var fromStock = new Stock { ProductId = productId, WarehouseId = fromWarehouseId, Quantity = 50 }; // Only 50
            var product = new Product { Id = productId, Name = "Product" };
            var fromWarehouse = new Warehouse { Id = fromWarehouseId, Name = "From WH" };
            var toWarehouse = new Warehouse { Id = toWarehouseId, Name = "To WH" };

            _mockProductRepo.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
            _mockWarehouseRepo.Setup(r => r.GetByIdAsync(fromWarehouseId)).ReturnsAsync(fromWarehouse);
            _mockWarehouseRepo.Setup(r => r.GetByIdAsync(toWarehouseId)).ReturnsAsync(toWarehouse);
            _mockStockRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Stock, bool>>>()))
                .ReturnsAsync(fromStock);

            // Act
            Func<Task> act = async () => await _stockService.TransferStockAsync(dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Tồn kho không đủ*");
            _mockUnitOfWork.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        }

        #endregion
    }
}
