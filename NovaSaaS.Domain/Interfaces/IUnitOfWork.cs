using NovaSaaS.Domain.Entities.Business;
using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Entities.Identity;
using NovaSaaS.Domain.Entities.Inventory;
using NovaSaaS.Domain.Entities.Master;
using System;
using System.Threading.Tasks;

namespace NovaSaaS.Domain.Interfaces
{
    /// <summary>
    /// Unit of Work Interface - Quản lý giao dịch cho tất cả Repositories.
    /// 
    /// Thay vì gọi SaveChanges() riêng lẻ cho từng Repository,
    /// IUnitOfWork đảm bảo tất cả thay đổi được commit trong một transaction duy nhất.
    /// 
    /// Ví dụ:
    /// _unitOfWork.Products.Add(product);
    /// await _unitOfWork.CompleteAsync(); // Commit
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        #region Identity Repositories

        IRepository<User> Users { get; }
        IRepository<Role> Roles { get; }
        IRepository<Permission> Permissions { get; }
        IRepository<RefreshToken> RefreshTokens { get; }

        #endregion

        #region Inventory Repositories

        IRepository<Product> Products { get; }
        IRepository<Category> Categories { get; }
        IRepository<Unit> Units { get; }
        IRepository<Warehouse> Warehouses { get; }
        IRepository<Stock> Stocks { get; }
        IRepository<StockMovement> StockMovements { get; }

        #endregion

        #region Business Repositories

        IRepository<Customer> Customers { get; }
        IRepository<Order> Orders { get; }
        IRepository<OrderItem> OrderItems { get; }
        IRepository<Invoice> Invoices { get; }
        IRepository<Coupon> Coupons { get; }
        IRepository<PaymentTransaction> PaymentTransactions { get; }

        #endregion

        #region Master Repositories

        IRepository<Tenant> Tenants { get; }
        IRepository<SubscriptionPlan> SubscriptionPlans { get; }
        IRepository<UsageLog> UsageLogs { get; }
        IRepository<SystemLog> SystemLogs { get; }

        #endregion

        #region Common Repositories

        IRepository<TenantSetting> TenantSettings { get; }
        IRepository<AuditLog> AuditLogs { get; }
        IRepository<Notification> Notifications { get; }

        #endregion

        #region Identity Extended

        IRepository<InvitationToken> InvitationTokens { get; }

        #endregion

        #region AI Repositories

        IRepository<Entities.AI.KnowledgeDocument> KnowledgeDocuments { get; }
        IRepository<Entities.AI.DocumentSegment> DocumentSegments { get; }
        IRepository<Entities.AI.ChatHistory> ChatHistories { get; }

        #endregion


        #region Transaction Methods

        /// <summary>
        /// Commit tất cả thay đổi vào database.
        /// </summary>
        /// <returns>Số dòng bị ảnh hưởng</returns>
        Task<int> CompleteAsync();

        /// <summary>
        /// Bắt đầu một transaction mới.
        /// </summary>
        Task BeginTransactionAsync();

        /// <summary>
        /// Commit transaction hiện tại.
        /// </summary>
        Task CommitTransactionAsync();

        /// <summary>
        /// Rollback transaction hiện tại.
        /// </summary>
        Task RollbackTransactionAsync();

        /// <summary>
        /// Thực thi lệnh SQL raw (Dùng cho tối ưu hóa session connection).
        /// </summary>
        Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters);

        #endregion

        #region Generic Repository Access

        /// <summary>
        /// Lấy Repository cho bất kỳ entity nào.
        /// Hữu ích khi cần truy cập dynamic entity.
        /// </summary>
        IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;

        #endregion
    }
}
