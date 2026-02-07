using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NovaSaaS.Domain.Entities.AI;
using NovaSaaS.Domain.Entities.Business;
using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Entities.Identity;
using NovaSaaS.Domain.Entities.Inventory;
using NovaSaaS.Domain.Entities.Master;
using NovaSaaS.Domain.Interfaces;

namespace NovaSaaS.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        private readonly ITenantService _tenantService;
        private readonly ICurrentUserService _currentUserService;

        public string? SchemaName => _tenantService.SchemaName;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options, 
            ITenantService tenantService,
            ICurrentUserService currentUserService)
            : base(options)
        {
            _tenantService = tenantService;
            _currentUserService = currentUserService;
        }

        protected ApplicationDbContext(
            DbContextOptions options,
            ITenantService tenantService,
            ICurrentUserService currentUserService)
            : base(options)
        {
            _tenantService = tenantService;
            _currentUserService = currentUserService;
        }

        // --- Đăng ký các DbSet ---
        // Master
        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
        public DbSet<PlanFeature> PlanFeatures => Set<PlanFeature>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<MasterAdmin> MasterAdmins => Set<MasterAdmin>();
        public DbSet<GlobalAuditLog> GlobalAuditLogs => Set<GlobalAuditLog>();
        public DbSet<UsageLog> UsageLogs => Set<UsageLog>();
        public DbSet<SystemLog> SystemLogs => Set<SystemLog>();

        // Identity
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

        // Business (CRM)
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<CustomerGroup> CustomerGroups => Set<CustomerGroup>();
        public DbSet<Contact> Contacts => Set<Contact>();
        public DbSet<Lead> Leads => Set<Lead>();
        public DbSet<Opportunity> Opportunities => Set<Opportunity>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Quotation> Quotations => Set<Quotation>();
        public DbSet<QuotationItem> QuotationItems => Set<QuotationItem>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<Coupon> Coupons => Set<Coupon>();
        public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
        public DbSet<LoyaltyProgram> LoyaltyPrograms => Set<LoyaltyProgram>();
        public DbSet<LoyaltyTransaction> LoyaltyTransactions => Set<LoyaltyTransaction>();
        public DbSet<Campaign> Campaigns => Set<Campaign>();
        public DbSet<Activity> Activities => Set<Activity>();

        // Inventory
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Warehouse> Warehouses => Set<Warehouse>();
        public DbSet<Unit> Units => Set<Unit>();
        public DbSet<Stock> Stocks => Set<Stock>();
        public DbSet<StockMovement> StockMovements => Set<StockMovement>();
        public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
        public DbSet<Location> Locations => Set<Location>();
        public DbSet<StockAdjustment> StockAdjustments => Set<StockAdjustment>();
        public DbSet<StockTransfer> StockTransfers => Set<StockTransfer>();
        public DbSet<StockTransferItem> StockTransferItems => Set<StockTransferItem>();
        public DbSet<InventoryCount> InventoryCounts => Set<InventoryCount>();
        public DbSet<InventoryCountItem> InventoryCountItems => Set<InventoryCountItem>();
        public DbSet<LotNumber> LotNumbers => Set<LotNumber>();
        public DbSet<SerialNumber> SerialNumbers => Set<SerialNumber>();

        // AI & Logs
        public DbSet<KnowledgeDocument> KnowledgeDocuments => Set<KnowledgeDocument>();
        public DbSet<DocumentSegment> DocumentSegments => Set<DocumentSegment>();
        public DbSet<ChatHistory> ChatHistories => Set<ChatHistory>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        // Settings
        public DbSet<TenantSetting> TenantSettings => Set<TenantSetting>();

        // Authentication
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        // Notifications & Invitations
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<InvitationToken> InvitationTokens => Set<InvitationToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Cấu hình Schema động cho Multi-tenancy
            if (!string.IsNullOrEmpty(_tenantService.SchemaName))
            {
                modelBuilder.HasDefaultSchema(_tenantService.SchemaName);
            }

            // 2. Ép các bảng Master LUÔN nằm ở schema 'public'
            modelBuilder.Entity<Tenant>().ToTable("Tenants", "public");
            modelBuilder.Entity<SubscriptionPlan>().ToTable("SubscriptionPlans", "public");
            modelBuilder.Entity<PlanFeature>().ToTable("PlanFeatures", "public");
            modelBuilder.Entity<Payment>().ToTable("Payments", "public");
            modelBuilder.Entity<GlobalAuditLog>().ToTable("GlobalAuditLogs", "public");
            modelBuilder.Entity<MasterAdmin>().ToTable("MasterAdmins", "public");
            modelBuilder.Entity<UsageLog>().ToTable("UsageLogs", "public");
            modelBuilder.Entity<SystemLog>().ToTable("SystemLogs", "public");

            // 3. Cấu hình Quan hệ Nhiều-Nhiều (RBAC)
            modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });
            modelBuilder.Entity<RolePermission>().HasKey(rp => new { rp.RoleId, rp.PermissionId });

            // 4. Cấu hình Quan hệ Business (Fluent API)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<QuotationItem>()
                .HasOne(qi => qi.Quotation)
                .WithMany(q => q.Items)
                .HasForeignKey(qi => qi.QuotationId);

            modelBuilder.Entity<StockTransferItem>()
                .HasOne(si => si.StockTransfer)
                .WithMany(s => s.Items)
                .HasForeignKey(si => si.StockTransferId);

            modelBuilder.Entity<InventoryCountItem>()
                .HasOne(ci => ci.InventoryCount)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.InventoryCountId);

            // StockTransfer - Warehouse relationships (avoid cascade cycles)
            modelBuilder.Entity<StockTransfer>()
                .HasOne(st => st.FromWarehouse)
                .WithMany()
                .HasForeignKey(st => st.FromWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockTransfer>()
                .HasOne(st => st.ToWarehouse)
                .WithMany()
                .HasForeignKey(st => st.ToWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            // 5. Cấu hình cho AI (Vector search)
            modelBuilder.Entity<DocumentSegment>(entity =>
            {
                // Nếu dùng pgvector, chúng sẽ cấu hình cột embedding ở đây
                entity.Property(e => e.Content).IsRequired();
            });

            // 6. Global Query Filter (Tự động lọc AuditLog theo User nếu cần)
            // Đây là chỗ bạn có thể thêm logic bảo mật nâng cao
            modelBuilder.Entity<AuditLog>().HasQueryFilter(a => 
                _currentUserService.IsAdmin || a.UserId == _currentUserService.UserId);

            // 7. Cấu hình Decimal Precision
            var decimalProps = new[]
            {
                (typeof(SubscriptionPlan), nameof(SubscriptionPlan.MonthlyPrice)),
                (typeof(Order), nameof(Order.TotalAmount)),
                (typeof(Order), nameof(Order.SubTotal)),
                (typeof(Order), nameof(Order.TaxAmount)),
                (typeof(Order), nameof(Order.DiscountAmount)),
                (typeof(OrderItem), nameof(OrderItem.UnitPrice)),
                (typeof(Product), nameof(Product.Price)),
                (typeof(Product), nameof(Product.CostPrice)),
                (typeof(Coupon), nameof(Coupon.DiscountValue)),
                (typeof(Invoice), nameof(Invoice.TotalAmount)),
                (typeof(PaymentTransaction), nameof(PaymentTransaction.Amount)),
                // CRM
                (typeof(Lead), nameof(Lead.EstimatedValue)),
                (typeof(Opportunity), nameof(Opportunity.Value)),
                (typeof(Quotation), nameof(Quotation.SubTotal)),
                (typeof(Quotation), nameof(Quotation.TaxAmount)),
                (typeof(Quotation), nameof(Quotation.DiscountAmount)),
                (typeof(Quotation), nameof(Quotation.TotalAmount)),
                (typeof(QuotationItem), nameof(QuotationItem.UnitPrice)),
                (typeof(QuotationItem), nameof(QuotationItem.DiscountPercent)),
                (typeof(CustomerGroup), nameof(CustomerGroup.DiscountPercent)),
                (typeof(LoyaltyProgram), nameof(LoyaltyProgram.PointsPerAmount)),
                (typeof(LoyaltyProgram), nameof(LoyaltyProgram.PointValue)),
                (typeof(Campaign), nameof(Campaign.Budget)),
                (typeof(Campaign), nameof(Campaign.ActualCost)),
                (typeof(Campaign), nameof(Campaign.Revenue)),
                // INV
                (typeof(ProductVariant), nameof(ProductVariant.Price)),
                (typeof(ProductVariant), nameof(ProductVariant.CostPrice))
            };

            foreach (var (type, prop) in decimalProps)
            {
                modelBuilder.Entity(type).Property(prop).HasColumnType("decimal(18,2)");
            }
        }
    }

    public class TenantModelCacheKeyFactory : IModelCacheKeyFactory
    {
        public object Create(DbContext context, bool designTime)
        {
            if (context is ApplicationDbContext dbContext)
            {
                // Trả về Tuple chứa cả Type của Context và SchemaName để làm Key cache
                return (context.GetType(), dbContext.SchemaName, designTime);
            }
            return (context.GetType(), designTime);
        }

        // Overload cho EF Core 6.0+ (nếu cần)
        public object Create(DbContext context) => Create(context, false);
    }
}
