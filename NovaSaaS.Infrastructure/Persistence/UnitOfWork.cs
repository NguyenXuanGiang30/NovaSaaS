using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NovaSaaS.Domain.Entities.AI;
using NovaSaaS.Domain.Entities.Business;
using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Entities.Identity;
using NovaSaaS.Domain.Entities.Inventory;
using NovaSaaS.Domain.Entities.Master;
using NovaSaaS.Domain.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace NovaSaaS.Infrastructure.Persistence
{
    /// <summary>
    /// Unit of Work Implementation - Quản lý giao dịch cho tất cả Repositories.
    /// 
    /// Mục tiêu:
    /// - Đảm bảo tất cả thay đổi được commit trong một transaction duy nhất
    /// - Lazy loading repositories (chỉ tạo khi cần)
    /// - Hỗ trợ transaction thủ công khi cần
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _currentTransaction;
        private bool _disposed = false;

        // Cache các repositories đã tạo (Lazy Loading)
        private readonly ConcurrentDictionary<Type, object> _repositories = new();

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Identity Repositories

        private IRepository<User>? _users;
        public IRepository<User> Users => _users ??= new Repository<User>(_context);

        private IRepository<Role>? _roles;
        public IRepository<Role> Roles => _roles ??= new Repository<Role>(_context);

        private IRepository<Permission>? _permissions;
        public IRepository<Permission> Permissions => _permissions ??= new Repository<Permission>(_context);

        private IRepository<RefreshToken>? _refreshTokens;
        public IRepository<RefreshToken> RefreshTokens => _refreshTokens ??= new Repository<RefreshToken>(_context);

        #endregion

        #region Inventory Repositories

        private IRepository<Product>? _products;
        public IRepository<Product> Products => _products ??= new Repository<Product>(_context);

        private IRepository<Category>? _categories;
        public IRepository<Category> Categories => _categories ??= new Repository<Category>(_context);

        private IRepository<Unit>? _units;
        public IRepository<Unit> Units => _units ??= new Repository<Unit>(_context);

        private IRepository<Warehouse>? _warehouses;
        public IRepository<Warehouse> Warehouses => _warehouses ??= new Repository<Warehouse>(_context);

        private IRepository<Stock>? _stocks;
        public IRepository<Stock> Stocks => _stocks ??= new Repository<Stock>(_context);

        private IRepository<StockMovement>? _stockMovements;
        public IRepository<StockMovement> StockMovements => _stockMovements ??= new Repository<StockMovement>(_context);

        #endregion

        #region Business Repositories

        private IRepository<Customer>? _customers;
        public IRepository<Customer> Customers => _customers ??= new Repository<Customer>(_context);

        private IRepository<Order>? _orders;
        public IRepository<Order> Orders => _orders ??= new Repository<Order>(_context);

        private IRepository<OrderItem>? _orderItems;
        public IRepository<OrderItem> OrderItems => _orderItems ??= new Repository<OrderItem>(_context);

        private IRepository<Invoice>? _invoices;
        public IRepository<Invoice> Invoices => _invoices ??= new Repository<Invoice>(_context);

        private IRepository<Coupon>? _coupons;
        public IRepository<Coupon> Coupons => _coupons ??= new Repository<Coupon>(_context);

        private IRepository<PaymentTransaction>? _paymentTransactions;
        public IRepository<PaymentTransaction> PaymentTransactions => _paymentTransactions ??= new Repository<PaymentTransaction>(_context);

        #endregion

        #region Master Repositories

        private IRepository<Tenant>? _tenants;
        public IRepository<Tenant> Tenants => _tenants ??= new Repository<Tenant>(_context);

        private IRepository<SubscriptionPlan>? _subscriptionPlans;
        public IRepository<SubscriptionPlan> SubscriptionPlans => _subscriptionPlans ??= new Repository<SubscriptionPlan>(_context);

        private IRepository<UsageLog>? _usageLogs;
        public IRepository<UsageLog> UsageLogs => _usageLogs ??= new Repository<UsageLog>(_context);

        private IRepository<SystemLog>? _systemLogs;
        public IRepository<SystemLog> SystemLogs => _systemLogs ??= new Repository<SystemLog>(_context);

        #endregion

        #region Common Repositories

        private IRepository<TenantSetting>? _tenantSettings;
        public IRepository<TenantSetting> TenantSettings => _tenantSettings ??= new Repository<TenantSetting>(_context);

        private IRepository<AuditLog>? _auditLogs;
        public IRepository<AuditLog> AuditLogs => _auditLogs ??= new Repository<AuditLog>(_context);

        private IRepository<Notification>? _notifications;
        public IRepository<Notification> Notifications => _notifications ??= new Repository<Notification>(_context);

        #endregion

        #region Identity Extended

        private IRepository<InvitationToken>? _invitationTokens;
        public IRepository<InvitationToken> InvitationTokens => _invitationTokens ??= new Repository<InvitationToken>(_context);

        #endregion

        #region AI Repositories

        private IRepository<KnowledgeDocument>? _knowledgeDocuments;
        public IRepository<KnowledgeDocument> KnowledgeDocuments => _knowledgeDocuments ??= new Repository<KnowledgeDocument>(_context);

        private IRepository<DocumentSegment>? _documentSegments;
        public IRepository<DocumentSegment> DocumentSegments => _documentSegments ??= new Repository<DocumentSegment>(_context);

        private IRepository<ChatHistory>? _chatHistories;
        public IRepository<ChatHistory> ChatHistories => _chatHistories ??= new Repository<ChatHistory>(_context);

        #endregion

        #region Generic Repository Access

        /// <summary>
        /// Lấy Repository cho bất kỳ entity nào (Dynamic access).
        /// </summary>
        public IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
        {
            var type = typeof(TEntity);

            if (!_repositories.ContainsKey(type))
            {
                var repositoryInstance = new Repository<TEntity>(_context);
                _repositories.TryAdd(type, repositoryInstance);
            }

            return (IRepository<TEntity>)_repositories[type];
        }

        #endregion

        #region Transaction Methods

        /// <summary>
        /// Commit tất cả thay đổi vào database.
        /// </summary>
        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Bắt đầu một transaction mới.
        /// </summary>
        public async Task BeginTransactionAsync()
        {
            _currentTransaction = await _context.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// Commit transaction hiện tại.
        /// </summary>
        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                if (_currentTransaction != null)
                {
                    await _currentTransaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        /// <summary>
        /// Rollback transaction hiện tại.
        /// </summary>
        public async Task RollbackTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync();
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        /// <summary>
        /// Thực thi lệnh SQL raw (Dùng cho tối ưu hóa session connection).
        /// </summary>
        public async Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters)
        {
            return await _context.Database.ExecuteSqlRawAsync(sql, parameters);
        }

        #endregion

        #region Dispose

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _currentTransaction?.Dispose();
                    _context.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
