using NovaSaaS.Application.DTOs.Business;
using NovaSaaS.Domain.Entities.Business;
using NovaSaaS.Domain.Entities.Inventory;
using NovaSaaS.Domain.Enums;
using NovaSaaS.Domain.Interfaces;
using NovaSaaS.Application.Services.Inventory;
using NovaSaaS.Application.Interfaces.Inventory; // IStockService
using NovaSaaS.Application.Interfaces.Business; // ICustomerService
using NovaSaaS.Application.Interfaces; // Added namespace
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NovaSaaS.Application.Services.Business
{
    public class OrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IStockService _stockService;
        private readonly ICustomerService _customerService;
        private readonly INotificationService _notificationService;

        public OrderService(
            IUnitOfWork unitOfWork, 
            ICurrentUserService currentUser, 
            IStockService stockService,
            ICustomerService customerService,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _stockService = stockService;
            _customerService = customerService;
            _notificationService = notificationService;
        }

        #region Order Creation (ACID Transaction)

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
        {
            // 1. Basic Validation
            if (dto.Items == null || !dto.Items.Any())
                throw new ArgumentException("Đơn hàng phải có ít nhất một sản phẩm.");

            var customer = await _unitOfWork.Customers.GetByIdAsync(dto.CustomerId);
            if (customer == null)
                throw new ArgumentException("Khách hàng không tồn tại.");

            // 2. Begin ACID Transaction
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 3. Generate Order Number (Simple logic: ORD-YYYYMMDD-Random)
                var orderNumber = $"ORD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

                var order = new Order
                {
                    OrderNumber = orderNumber,
                    CustomerId = dto.CustomerId,
                    Note = dto.Note,
                    Status = OrderStatus.Confirmed,
                    CreateAt = DateTime.UtcNow,
                    CreatedBy = _currentUser.UserId
                };

                // 4. Process Items & Calculate Totals
                decimal subTotal = 0;
                var orderItems = new List<OrderItem>();

                foreach (var itemDto in dto.Items)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId);
                    if (product == null)
                        throw new ArgumentException($"Sản phẩm ID {itemDto.ProductId} không tồn tại.");

                    var item = new OrderItem
                    {
                        ProductId = itemDto.ProductId,
                        Quantity = itemDto.Quantity,
                        UnitPrice = product.Price,
                        CreateAt = DateTime.UtcNow,
                        CreatedBy = _currentUser.UserId
                    };
                    orderItems.Add(item);
                    subTotal += item.Quantity * item.UnitPrice;
                }

                order.OrderItems = orderItems;
                order.SubTotal = subTotal;
                order.TaxAmount = subTotal * 0.1m; // 10% VAT
                order.DiscountAmount = 0; // TODO: Apply coupon logic
                order.TotalAmount = order.SubTotal + order.TaxAmount - order.DiscountAmount;

                // Save Order
                _unitOfWork.Orders.Add(order);
                await _unitOfWork.CompleteAsync();

                // 5. Stock Deduction & Movement Logging
                await _stockService.ReduceStockForOrderAsync(order.Id, order.OrderNumber, dto.Items);

                // 6. Update Customer Spending
                await _customerService.UpdateSpendingAsync(dto.CustomerId, order.TotalAmount, isAddition: true);

                // 7. Auto-Generate Invoice
                var invoice = new Invoice
                {
                    OrderId = order.Id,
                    InvoiceNumber = $"INV-{order.OrderNumber}",
                    TotalAmount = order.TotalAmount,
                    Status = InvoiceStatus.Unpaid,
                    DueDate = DateTime.UtcNow.AddDays(7),
                    CreateAt = DateTime.UtcNow,
                    CreatedBy = _currentUser.UserId
                };
                _unitOfWork.Invoices.Add(invoice);

                // 8. Commit Transaction
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                // 9. Send Real-time Notification (Fire-and-forget)
                var tenantId = _currentUser.TenantId ?? Guid.Empty;
                _ = _notificationService.NotifyNewOrderAsync(
                    tenantId,
                    order.Id,
                    order.OrderNumber,
                    customer.Name,
                    order.TotalAmount,
                    order.OrderItems.Count
                );

                return (await GetOrderByIdAsync(order.Id))!;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        #endregion

        #region Query Operations

        public async Task<PaginatedResult<OrderDto>> GetAllAsync(OrderFilterDto? filter = null)
        {
            filter ??= new OrderFilterDto();

            var allOrders = await _unitOfWork.Orders.FindAsync(
                _ => true,
                o => o.Customer,
                o => o.Invoice);

            var query = allOrders.AsQueryable();

            // Apply filters
            if (filter.CustomerId.HasValue)
                query = query.Where(o => o.CustomerId == filter.CustomerId.Value);

            if (filter.Status.HasValue)
                query = query.Where(o => o.Status == filter.Status.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(o => o.CreateAt >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(o => o.CreateAt <= filter.ToDate.Value);

            var totalCount = query.Count();

            var items = query
                .OrderByDescending(o => o.CreateAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer.Name,
                    SubTotal = o.SubTotal,
                    TaxAmount = o.TaxAmount,
                    DiscountAmount = o.DiscountAmount,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    CreateAt = o.CreateAt,
                    Note = o.Note,
                    Invoice = o.Invoice != null ? new InvoiceDto
                    {
                        Id = o.Invoice.Id,
                        InvoiceNumber = o.Invoice.InvoiceNumber,
                        Status = o.Invoice.Status
                    } : null
                })
                .ToList();

            return new PaginatedResult<OrderDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<OrderDto?> GetOrderByIdAsync(Guid id)
        {
            var orders = await _unitOfWork.Orders.FindAsync(
                o => o.Id == id,
                o => o.Customer,
                o => o.OrderItems,
                o => o.Invoice
            );

            var order = orders.FirstOrDefault();
            if (order == null) return null;

            // Load product names for items
            var productIds = order.OrderItems.Select(i => i.ProductId).ToList();
            var products = await _unitOfWork.Products.FindAsync(p => productIds.Contains(p.Id));
            var productDict = products.ToDictionary(p => p.Id, p => p.Name);

            return new OrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer.Name,
                SubTotal = order.SubTotal,
                TaxAmount = order.TaxAmount,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                CreateAt = order.CreateAt,
                Note = order.Note,
                Items = order.OrderItems.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = productDict.GetValueOrDefault(i.ProductId, "Unknown"),
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList(),
                Invoice = order.Invoice != null ? new InvoiceDto
                {
                    Id = order.Invoice.Id,
                    InvoiceNumber = order.Invoice.InvoiceNumber,
                    TotalAmount = order.Invoice.TotalAmount,
                    Status = order.Invoice.Status,
                    DueDate = order.Invoice.DueDate,
                    PaidDate = order.Invoice.PaidDate,
                    PaymentMethod = order.Invoice.PaymentMethod
                } : null
            };
        }

        #endregion

        #region Order Status Management

        public async Task<OrderDto> UpdateStatusAsync(Guid id, UpdateOrderStatusDto dto)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
                throw new ArgumentException("Đơn hàng không tồn tại.");

            // Validate status transition
            ValidateStatusTransition(order.Status, dto.NewStatus);

            order.Status = dto.NewStatus;
            order.Note = string.IsNullOrEmpty(dto.Note) ? order.Note : $"{order.Note}\n[{DateTime.Now:dd/MM HH:mm}] {dto.Note}";
            order.UpdateAt = DateTime.UtcNow;
            order.UpdatedBy = _currentUser.UserId;

            _unitOfWork.Orders.Update(order);
            await _unitOfWork.CompleteAsync();

            // Real-time notification for status change
            var tenantId = _currentUser.TenantId ?? Guid.Empty;
            _ = _notificationService.NotifyOrderStatusChangedAsync(
                tenantId,
                order.Id,
                order.OrderNumber,
                order.Status.ToString(),
                dto.NewStatus.ToString()
            );

            return (await GetOrderByIdAsync(id))!;
        }

        public async Task<OrderDto> CancelOrderAsync(Guid id, CancelOrderRequest request)
        {
            var orders = await _unitOfWork.Orders.FindAsync(
                o => o.Id == id,
                o => o.OrderItems,
                o => o.Invoice);

            var order = orders.FirstOrDefault();
            if (order == null)
                throw new ArgumentException("Đơn hàng không tồn tại.");

            // Can only cancel Pending or Confirmed orders
            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
                throw new InvalidOperationException($"Không thể hủy đơn hàng ở trạng thái '{order.Status}'.");

            // Begin Transaction
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 1. Restore Stock if requested
                if (request.RestoreStock && order.Status == OrderStatus.Confirmed)
                {
                    await RestoreStockForOrderAsync(order);
                }

                // 2. Update Order Status
                order.Status = OrderStatus.Cancelled;
                order.Note = $"{order.Note}\n[{DateTime.Now:dd/MM HH:mm}] Hủy đơn: {request.Reason}";
                order.UpdateAt = DateTime.UtcNow;
                order.UpdatedBy = _currentUser.UserId;
                _unitOfWork.Orders.Update(order);

                // 3. Cancel Invoice if exists
                if (order.Invoice != null)
                {
                    order.Invoice.Status = InvoiceStatus.Cancelled;
                    order.Invoice.UpdateAt = DateTime.UtcNow;
                    _unitOfWork.Invoices.Update(order.Invoice);
                }

                // 4. Reduce Customer Spending
                await _customerService.UpdateSpendingAsync(order.CustomerId, order.TotalAmount, isAddition: false);

                // 5. Commit
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                return (await GetOrderByIdAsync(id))!;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        #endregion

        #region Private Helpers

        private async Task RestoreStockForOrderAsync(Order order)
        {
            foreach (var item in order.OrderItems)
            {
                // Find the original stock movement to get warehouse
                var movements = await _unitOfWork.StockMovements.FindAsync(
                    m => m.ReferenceId == order.Id && m.ProductId == item.ProductId && m.Type == StockMovementType.Sale);
                
                var originalMovement = movements.FirstOrDefault();
                if (originalMovement == null) continue;

                var warehouseId = originalMovement.WarehouseId;

                // Get current stock
                var stock = await _unitOfWork.Repository<Stock>()
                    .FirstOrDefaultAsync(s => s.ProductId == item.ProductId && s.WarehouseId == warehouseId);

                if (stock == null) continue;

                var qtyBefore = stock.Quantity;

                // Restore quantity
                stock.Quantity += item.Quantity;
                stock.UpdateAt = DateTime.UtcNow;
                _unitOfWork.Repository<Stock>().Update(stock);

                // Update product total stock
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity;
                    _unitOfWork.Products.Update(product);
                }

                // Log Movement
                var returnMovement = new StockMovement
                {
                    ProductId = item.ProductId,
                    WarehouseId = warehouseId,
                    Quantity = item.Quantity, // Positive for return
                    Type = StockMovementType.Return,
                    QuantityBefore = qtyBefore,
                    QuantityAfter = stock.Quantity,
                    ReferenceCode = order.OrderNumber,
                    ReferenceId = order.Id,
                    Notes = $"Hoàn kho do hủy đơn hàng {order.OrderNumber}",
                    CreatedBy = _currentUser.UserId
                };
                _unitOfWork.StockMovements.Add(returnMovement);
            }
        }

        private static void ValidateStatusTransition(OrderStatus current, OrderStatus next)
        {
            var validTransitions = new Dictionary<OrderStatus, OrderStatus[]>
            {
                { OrderStatus.Pending, new[] { OrderStatus.Confirmed, OrderStatus.Cancelled } },
                { OrderStatus.Confirmed, new[] { OrderStatus.Shipping, OrderStatus.Cancelled } },
                { OrderStatus.Shipping, new[] { OrderStatus.Completed } },
                { OrderStatus.Completed, Array.Empty<OrderStatus>() },
                { OrderStatus.Cancelled, Array.Empty<OrderStatus>() }
            };

            if (!validTransitions.ContainsKey(current) || !validTransitions[current].Contains(next))
            {
                throw new InvalidOperationException(
                    $"Không thể chuyển trạng thái từ '{current}' sang '{next}'.");
            }
        }

        #endregion
    }
}
