using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NovaSaaS.Domain.Entities.AI;
using NovaSaaS.Domain.Entities.Business;
using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Entities.Identity;
using NovaSaaS.Domain.Entities.Inventory;
using NovaSaaS.Domain.Entities.Master;
using NovaSaaS.Domain.Entities.HRM;
using NovaSaaS.Domain.Entities.Accounting;
using NovaSaaS.Domain.Entities.SCM;
using NovaSaaS.Domain.Entities.PM;
using NovaSaaS.Domain.Entities.DMS;
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

        // =============================================
        // Master (public schema)
        // =============================================
        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
        public DbSet<PlanFeature> PlanFeatures => Set<PlanFeature>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<MasterAdmin> MasterAdmins => Set<MasterAdmin>();
        public DbSet<GlobalAuditLog> GlobalAuditLogs => Set<GlobalAuditLog>();
        public DbSet<UsageLog> UsageLogs => Set<UsageLog>();
        public DbSet<SystemLog> SystemLogs => Set<SystemLog>();

        // =============================================
        // Identity
        // =============================================
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<InvitationToken> InvitationTokens => Set<InvitationToken>();

        // =============================================
        // CRM (Business)
        // =============================================
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

        // =============================================
        // Inventory (INV)
        // =============================================
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

        // =============================================
        // HRM (Nhân sự)
        // =============================================
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Position> Positions => Set<Position>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<EmployeeContract> EmployeeContracts => Set<EmployeeContract>();
        public DbSet<WorkShift> WorkShifts => Set<WorkShift>();
        public DbSet<Attendance> Attendances => Set<Attendance>();
        public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
        public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
        public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
        public DbSet<PayrollPeriod> PayrollPeriods => Set<PayrollPeriod>();
        public DbSet<Payroll> Payrolls => Set<Payroll>();
        public DbSet<PayrollDetail> PayrollDetails => Set<PayrollDetail>();
        public DbSet<PerformanceReview> PerformanceReviews => Set<PerformanceReview>();
        public DbSet<KPI> KPIs => Set<KPI>();
        public DbSet<Recruitment> Recruitments => Set<Recruitment>();
        public DbSet<Candidate> Candidates => Set<Candidate>();
        public DbSet<Training> Trainings => Set<Training>();
        public DbSet<TrainingParticipant> TrainingParticipants => Set<TrainingParticipant>();

        // =============================================
        // Accounting (ACC)
        // =============================================
        public DbSet<ChartOfAccount> ChartOfAccounts => Set<ChartOfAccount>();
        public DbSet<FiscalYear> FiscalYears => Set<FiscalYear>();
        public DbSet<FiscalPeriod> FiscalPeriods => Set<FiscalPeriod>();
        public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
        public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();
        public DbSet<AccountReceivable> AccountReceivables => Set<AccountReceivable>();
        public DbSet<AccountPayable> AccountPayables => Set<AccountPayable>();
        public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
        public DbSet<BankTransaction> BankTransactions => Set<BankTransaction>();
        public DbSet<BankReconciliation> BankReconciliations => Set<BankReconciliation>();
        public DbSet<Budget> Budgets => Set<Budget>();
        public DbSet<BudgetLine> BudgetLines => Set<BudgetLine>();
        public DbSet<TaxRate> TaxRates => Set<TaxRate>();
        public DbSet<TaxTransaction> TaxTransactions => Set<TaxTransaction>();

        // =============================================
        // SCM (Chuỗi cung ứng)
        // =============================================
        public DbSet<Vendor> Vendors => Set<Vendor>();
        public DbSet<VendorContact> VendorContacts => Set<VendorContact>();
        public DbSet<PurchaseRequisition> PurchaseRequisitions => Set<PurchaseRequisition>();
        public DbSet<PurchaseRequisitionItem> PurchaseRequisitionItems => Set<PurchaseRequisitionItem>();
        public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
        public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
        public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();
        public DbSet<GoodsReceiptItem> GoodsReceiptItems => Set<GoodsReceiptItem>();
        public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();
        public DbSet<VendorPayment> VendorPayments => Set<VendorPayment>();
        public DbSet<ReturnToVendor> ReturnToVendors => Set<ReturnToVendor>();
        public DbSet<ReturnToVendorItem> ReturnToVendorItems => Set<ReturnToVendorItem>();
        public DbSet<VendorPriceList> VendorPriceLists => Set<VendorPriceList>();
        public DbSet<VendorPriceListItem> VendorPriceListItems => Set<VendorPriceListItem>();

        // =============================================
        // PM (Quản lý Dự án)
        // =============================================
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<ProjectPhase> ProjectPhases => Set<ProjectPhase>();
        public DbSet<ProjectMilestone> ProjectMilestones => Set<ProjectMilestone>();
        public DbSet<ProjectTask> ProjectTasks => Set<ProjectTask>();
        public DbSet<TaskDependency> TaskDependencies => Set<TaskDependency>();
        public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
        public DbSet<Timesheet> Timesheets => Set<Timesheet>();
        public DbSet<TimesheetEntry> TimesheetEntries => Set<TimesheetEntry>();
        public DbSet<ProjectExpense> ProjectExpenses => Set<ProjectExpense>();
        public DbSet<ProjectComment> ProjectComments => Set<ProjectComment>();

        // =============================================
        // DMS (Quản lý Tài liệu)
        // =============================================
        public DbSet<Folder> Folders => Set<Folder>();
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();
        public DbSet<DocumentComment> DocumentComments => Set<DocumentComment>();
        public DbSet<DocumentPermission> DocumentPermissions => Set<DocumentPermission>();
        public DbSet<DocumentShare> DocumentShares => Set<DocumentShare>();
        public DbSet<DocumentWorkflow> DocumentWorkflows => Set<DocumentWorkflow>();
        public DbSet<DocumentWorkflowStep> DocumentWorkflowSteps => Set<DocumentWorkflowStep>();
        public DbSet<DocumentTemplate> DocumentTemplates => Set<DocumentTemplate>();

        // =============================================
        // AI & Logs
        // =============================================
        public DbSet<KnowledgeDocument> KnowledgeDocuments => Set<KnowledgeDocument>();
        public DbSet<DocumentSegment> DocumentSegments => Set<DocumentSegment>();
        public DbSet<ChatHistory> ChatHistories => Set<ChatHistory>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        // Settings
        public DbSet<TenantSetting> TenantSettings => Set<TenantSetting>();

        // Notifications
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =============================================
            // 1. Schema Multi-tenancy
            // =============================================
            if (!string.IsNullOrEmpty(_tenantService.SchemaName))
            {
                modelBuilder.HasDefaultSchema(_tenantService.SchemaName);
            }

            // Master tables -> always in 'public' schema
            modelBuilder.Entity<Tenant>().ToTable("Tenants", "public");
            modelBuilder.Entity<SubscriptionPlan>().ToTable("SubscriptionPlans", "public");
            modelBuilder.Entity<PlanFeature>().ToTable("PlanFeatures", "public");
            modelBuilder.Entity<Payment>().ToTable("Payments", "public");
            modelBuilder.Entity<GlobalAuditLog>().ToTable("GlobalAuditLogs", "public");
            modelBuilder.Entity<MasterAdmin>().ToTable("MasterAdmins", "public");
            modelBuilder.Entity<UsageLog>().ToTable("UsageLogs", "public");
            modelBuilder.Entity<SystemLog>().ToTable("SystemLogs", "public");

            // =============================================
            // 2. Identity (RBAC) - Composite Keys
            // =============================================
            modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });
            modelBuilder.Entity<RolePermission>().HasKey(rp => new { rp.RoleId, rp.PermissionId });

            // =============================================
            // 3. CRM Relationships
            // =============================================
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<QuotationItem>()
                .HasOne(qi => qi.Quotation)
                .WithMany(q => q.Items)
                .HasForeignKey(qi => qi.QuotationId);

            modelBuilder.Entity<Contact>()
                .HasOne(c => c.Customer)
                .WithMany(cu => cu.Contacts)
                .HasForeignKey(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LoyaltyTransaction>()
                .HasOne(lt => lt.LoyaltyProgram)
                .WithMany(lp => lp.Transactions)
                .HasForeignKey(lt => lt.LoyaltyProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LoyaltyTransaction>()
                .HasOne(lt => lt.Customer)
                .WithMany(c => c.LoyaltyTransactions)
                .HasForeignKey(lt => lt.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Activity>()
                .HasOne(a => a.Customer)
                .WithMany(c => c.Activities)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Opportunity>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Opportunities)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Invoice -> PaymentTransactions
            modelBuilder.Entity<PaymentTransaction>()
                .HasOne(pt => pt.Invoice)
                .WithMany(i => i.PaymentTransactions)
                .HasForeignKey(pt => pt.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            // =============================================
            // 4. INV Relationships
            // =============================================
            modelBuilder.Entity<StockTransferItem>()
                .HasOne(si => si.StockTransfer)
                .WithMany(s => s.Items)
                .HasForeignKey(si => si.StockTransferId);

            modelBuilder.Entity<InventoryCountItem>()
                .HasOne(ci => ci.InventoryCount)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.InventoryCountId);

            // StockTransfer - Warehouse (avoid cascade)
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

            // Category self-reference
            modelBuilder.Entity<Category>()
                .HasOne(c => c.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // =============================================
            // 5. HRM Relationships
            // =============================================
            // Department self-reference
            modelBuilder.Entity<Department>()
                .HasOne(d => d.ParentDepartment)
                .WithMany(d => d.ChildDepartments)
                .HasForeignKey(d => d.ParentDepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Department>()
                .HasOne(d => d.ManagerEmployee)
                .WithMany()
                .HasForeignKey(d => d.ManagerEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Employee self-reference (Manager)
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Manager)
                .WithMany(e => e.DirectReports)
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Position)
                .WithMany(p => p.Employees)
                .HasForeignKey(e => e.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Position>()
                .HasOne(p => p.Department)
                .WithMany(d => d.Positions)
                .HasForeignKey(p => p.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmployeeContract>()
                .HasOne(c => c.Employee)
                .WithMany(e => e.Contracts)
                .HasForeignKey(c => c.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Employee)
                .WithMany(e => e.Attendances)
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeaveRequest>()
                .HasOne(lr => lr.Employee)
                .WithMany(e => e.LeaveRequests)
                .HasForeignKey(lr => lr.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeaveBalance>()
                .HasOne(lb => lb.Employee)
                .WithMany(e => e.LeaveBalances)
                .HasForeignKey(lb => lb.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payroll>()
                .HasOne(p => p.Employee)
                .WithMany(e => e.Payrolls)
                .HasForeignKey(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payroll>()
                .HasOne(p => p.PayrollPeriod)
                .WithMany(pp => pp.Payrolls)
                .HasForeignKey(p => p.PayrollPeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PayrollDetail>()
                .HasOne(pd => pd.Payroll)
                .WithMany(p => p.Details)
                .HasForeignKey(pd => pd.PayrollId);

            modelBuilder.Entity<PerformanceReview>()
                .HasOne(pr => pr.Employee)
                .WithMany(e => e.PerformanceReviews)
                .HasForeignKey(pr => pr.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<KPI>()
                .HasOne(k => k.PerformanceReview)
                .WithMany(pr => pr.KPIs)
                .HasForeignKey(k => k.PerformanceReviewId);

            modelBuilder.Entity<Recruitment>()
                .HasOne(r => r.Department)
                .WithMany()
                .HasForeignKey(r => r.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Recruitment>()
                .HasOne(r => r.Position)
                .WithMany()
                .HasForeignKey(r => r.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Candidate>()
                .HasOne(c => c.Recruitment)
                .WithMany(r => r.Candidates)
                .HasForeignKey(c => c.RecruitmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TrainingParticipant>()
                .HasOne(tp => tp.Training)
                .WithMany(t => t.Participants)
                .HasForeignKey(tp => tp.TrainingId);

            // =============================================
            // 6. Accounting Relationships
            // =============================================
            // ChartOfAccount self-reference
            modelBuilder.Entity<ChartOfAccount>()
                .HasOne(a => a.ParentAccount)
                .WithMany(a => a.ChildAccounts)
                .HasForeignKey(a => a.ParentAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FiscalPeriod>()
                .HasOne(fp => fp.FiscalYear)
                .WithMany(fy => fy.Periods)
                .HasForeignKey(fp => fp.FiscalYearId);

            modelBuilder.Entity<JournalEntry>()
                .HasOne(je => je.FiscalPeriod)
                .WithMany(fp => fp.JournalEntries)
                .HasForeignKey(je => je.FiscalPeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<JournalEntry>()
                .HasOne(je => je.ReversedFrom)
                .WithMany()
                .HasForeignKey(je => je.ReversedFromId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<JournalEntryLine>()
                .HasOne(jl => jl.JournalEntry)
                .WithMany(je => je.Lines)
                .HasForeignKey(jl => jl.JournalEntryId);

            modelBuilder.Entity<JournalEntryLine>()
                .HasOne(jl => jl.Account)
                .WithMany(a => a.JournalEntryLines)
                .HasForeignKey(jl => jl.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BankTransaction>()
                .HasOne(bt => bt.BankAccount)
                .WithMany(ba => ba.Transactions)
                .HasForeignKey(bt => bt.BankAccountId);

            modelBuilder.Entity<BankReconciliation>()
                .HasOne(br => br.BankAccount)
                .WithMany(ba => ba.Reconciliations)
                .HasForeignKey(br => br.BankAccountId);

            modelBuilder.Entity<BudgetLine>()
                .HasOne(bl => bl.Budget)
                .WithMany(b => b.Lines)
                .HasForeignKey(bl => bl.BudgetId);

            modelBuilder.Entity<BudgetLine>()
                .HasOne(bl => bl.Account)
                .WithMany()
                .HasForeignKey(bl => bl.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaxRate>()
                .HasOne(tr => tr.OutputAccount)
                .WithMany()
                .HasForeignKey(tr => tr.OutputAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaxRate>()
                .HasOne(tr => tr.InputAccount)
                .WithMany()
                .HasForeignKey(tr => tr.InputAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaxTransaction>()
                .HasOne(tt => tt.TaxRate)
                .WithMany(tr => tr.TaxTransactions)
                .HasForeignKey(tt => tt.TaxRateId);

            // =============================================
            // 7. SCM Relationships
            // =============================================
            modelBuilder.Entity<VendorContact>()
                .HasOne(vc => vc.Vendor)
                .WithMany(v => v.Contacts)
                .HasForeignKey(vc => vc.VendorId);

            modelBuilder.Entity<PurchaseRequisitionItem>()
                .HasOne(pri => pri.PurchaseRequisition)
                .WithMany(pr => pr.Items)
                .HasForeignKey(pri => pri.PurchaseRequisitionId);

            modelBuilder.Entity<PurchaseOrder>()
                .HasOne(po => po.Vendor)
                .WithMany(v => v.PurchaseOrders)
                .HasForeignKey(po => po.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseOrderItem>()
                .HasOne(poi => poi.PurchaseOrder)
                .WithMany(po => po.Items)
                .HasForeignKey(poi => poi.PurchaseOrderId);

            modelBuilder.Entity<GoodsReceipt>()
                .HasOne(gr => gr.PurchaseOrder)
                .WithMany(po => po.GoodsReceipts)
                .HasForeignKey(gr => gr.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GoodsReceiptItem>()
                .HasOne(gri => gri.GoodsReceipt)
                .WithMany(gr => gr.Items)
                .HasForeignKey(gri => gri.GoodsReceiptId);

            modelBuilder.Entity<PurchaseInvoice>()
                .HasOne(pi => pi.Vendor)
                .WithMany(v => v.PurchaseInvoices)
                .HasForeignKey(pi => pi.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VendorPayment>()
                .HasOne(vp => vp.PurchaseInvoice)
                .WithMany(pi => pi.Payments)
                .HasForeignKey(vp => vp.PurchaseInvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VendorPayment>()
                .HasOne(vp => vp.Vendor)
                .WithMany()
                .HasForeignKey(vp => vp.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReturnToVendor>()
                .HasOne(rtv => rtv.Vendor)
                .WithMany()
                .HasForeignKey(rtv => rtv.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReturnToVendorItem>()
                .HasOne(rtvi => rtvi.ReturnToVendor)
                .WithMany(rtv => rtv.Items)
                .HasForeignKey(rtvi => rtvi.ReturnToVendorId);

            modelBuilder.Entity<VendorPriceList>()
                .HasOne(vpl => vpl.Vendor)
                .WithMany(v => v.PriceLists)
                .HasForeignKey(vpl => vpl.VendorId);

            modelBuilder.Entity<VendorPriceListItem>()
                .HasOne(vpli => vpli.VendorPriceList)
                .WithMany(vpl => vpl.Items)
                .HasForeignKey(vpli => vpli.VendorPriceListId);

            // =============================================
            // 8. PM Relationships
            // =============================================
            modelBuilder.Entity<ProjectPhase>()
                .HasOne(pp => pp.Project)
                .WithMany(p => p.Phases)
                .HasForeignKey(pp => pp.ProjectId);

            modelBuilder.Entity<ProjectMilestone>()
                .HasOne(pm => pm.Project)
                .WithMany(p => p.Milestones)
                .HasForeignKey(pm => pm.ProjectId);

            modelBuilder.Entity<ProjectTask>()
                .HasOne(pt => pt.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(pt => pt.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectTask>()
                .HasOne(pt => pt.ProjectPhase)
                .WithMany(pp => pp.Tasks)
                .HasForeignKey(pt => pt.ProjectPhaseId)
                .OnDelete(DeleteBehavior.Restrict);

            // ProjectTask self-reference (sub-tasks)
            modelBuilder.Entity<ProjectTask>()
                .HasOne(pt => pt.ParentTask)
                .WithMany(pt => pt.SubTasks)
                .HasForeignKey(pt => pt.ParentTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            // TaskDependency
            modelBuilder.Entity<TaskDependency>()
                .HasOne(td => td.PredecessorTask)
                .WithMany(pt => pt.Dependents)
                .HasForeignKey(td => td.PredecessorTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskDependency>()
                .HasOne(td => td.SuccessorTask)
                .WithMany(pt => pt.Dependencies)
                .HasForeignKey(td => td.SuccessorTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectMember>()
                .HasOne(pm => pm.Project)
                .WithMany(p => p.Members)
                .HasForeignKey(pm => pm.ProjectId);

            modelBuilder.Entity<Timesheet>()
                .HasOne(ts => ts.Project)
                .WithMany()
                .HasForeignKey(ts => ts.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TimesheetEntry>()
                .HasOne(te => te.Timesheet)
                .WithMany(ts => ts.Entries)
                .HasForeignKey(te => te.TimesheetId);

            modelBuilder.Entity<TimesheetEntry>()
                .HasOne(te => te.ProjectTask)
                .WithMany(pt => pt.TimesheetEntries)
                .HasForeignKey(te => te.ProjectTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectExpense>()
                .HasOne(pe => pe.Project)
                .WithMany(p => p.Expenses)
                .HasForeignKey(pe => pe.ProjectId);

            modelBuilder.Entity<ProjectComment>()
                .HasOne(pc => pc.ProjectTask)
                .WithMany(pt => pt.Comments)
                .HasForeignKey(pc => pc.ProjectTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectComment>()
                .HasOne(pc => pc.ParentComment)
                .WithMany()
                .HasForeignKey(pc => pc.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            // =============================================
            // 9. DMS Relationships
            // =============================================
            // Folder self-reference
            modelBuilder.Entity<Folder>()
                .HasOne(f => f.ParentFolder)
                .WithMany(f => f.ChildFolders)
                .HasForeignKey(f => f.ParentFolderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Document>()
                .HasOne(d => d.Folder)
                .WithMany(f => f.Documents)
                .HasForeignKey(d => d.FolderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DocumentVersion>()
                .HasOne(dv => dv.Document)
                .WithMany(d => d.Versions)
                .HasForeignKey(dv => dv.DocumentId);

            modelBuilder.Entity<DocumentComment>()
                .HasOne(dc => dc.Document)
                .WithMany(d => d.Comments)
                .HasForeignKey(dc => dc.DocumentId);

            modelBuilder.Entity<DocumentComment>()
                .HasOne(dc => dc.ParentComment)
                .WithMany()
                .HasForeignKey(dc => dc.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DocumentPermission>()
                .HasOne(dp => dp.Document)
                .WithMany(d => d.Permissions)
                .HasForeignKey(dp => dp.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DocumentPermission>()
                .HasOne(dp => dp.Folder)
                .WithMany(f => f.Permissions)
                .HasForeignKey(dp => dp.FolderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DocumentShare>()
                .HasOne(ds => ds.Document)
                .WithMany(d => d.Shares)
                .HasForeignKey(ds => ds.DocumentId);

            modelBuilder.Entity<DocumentWorkflow>()
                .HasOne(dw => dw.Document)
                .WithMany(d => d.Workflows)
                .HasForeignKey(dw => dw.DocumentId);

            modelBuilder.Entity<DocumentWorkflowStep>()
                .HasOne(dws => dws.DocumentWorkflow)
                .WithMany(dw => dw.Steps)
                .HasForeignKey(dws => dws.DocumentWorkflowId);

            // =============================================
            // 10. AI (Vector search)
            // =============================================
            modelBuilder.Entity<DocumentSegment>(entity =>
            {
                entity.Property(e => e.Content).IsRequired();
            });

            // =============================================
            // 11. Global Query Filter
            // =============================================
            modelBuilder.Entity<AuditLog>().HasQueryFilter(a => 
                _currentUserService.IsAdmin || a.UserId == _currentUserService.UserId);

            // =============================================
            // 12. Decimal Precision Configuration (18,2)
            // =============================================
            var decimalProps = new[]
            {
                // Master
                (typeof(SubscriptionPlan), nameof(SubscriptionPlan.MonthlyPrice)),
                // CRM
                (typeof(Order), nameof(Order.TotalAmount)),
                (typeof(Order), nameof(Order.SubTotal)),
                (typeof(Order), nameof(Order.TaxAmount)),
                (typeof(Order), nameof(Order.DiscountAmount)),
                (typeof(Order), nameof(Order.ShippingFee)),
                (typeof(OrderItem), nameof(OrderItem.UnitPrice)),
                (typeof(Product), nameof(Product.Price)),
                (typeof(Product), nameof(Product.CostPrice)),
                (typeof(Coupon), nameof(Coupon.DiscountValue)),
                (typeof(Coupon), nameof(Coupon.MinOrderAmount)),
                (typeof(Invoice), nameof(Invoice.TotalAmount)),
                (typeof(Invoice), nameof(Invoice.SubTotal)),
                (typeof(Invoice), nameof(Invoice.TaxAmount)),
                (typeof(Invoice), nameof(Invoice.DiscountAmount)),
                (typeof(Invoice), nameof(Invoice.PaidAmount)),
                (typeof(PaymentTransaction), nameof(PaymentTransaction.Amount)),
                (typeof(Lead), nameof(Lead.EstimatedValue)),
                (typeof(Opportunity), nameof(Opportunity.Value)),
                (typeof(Quotation), nameof(Quotation.SubTotal)),
                (typeof(Quotation), nameof(Quotation.TaxAmount)),
                (typeof(Quotation), nameof(Quotation.DiscountAmount)),
                (typeof(Quotation), nameof(Quotation.TotalAmount)),
                (typeof(QuotationItem), nameof(QuotationItem.UnitPrice)),
                (typeof(QuotationItem), nameof(QuotationItem.DiscountPercent)),
                (typeof(CustomerGroup), nameof(CustomerGroup.DiscountPercent)),
                (typeof(Customer), nameof(Customer.TotalSpending)),
                (typeof(Customer), nameof(Customer.CreditLimit)),
                (typeof(Customer), nameof(Customer.CurrentDebt)),
                (typeof(LoyaltyProgram), nameof(LoyaltyProgram.PointsPerAmount)),
                (typeof(LoyaltyProgram), nameof(LoyaltyProgram.PointValue)),
                (typeof(Campaign), nameof(Campaign.Budget)),
                (typeof(Campaign), nameof(Campaign.ActualCost)),
                (typeof(Campaign), nameof(Campaign.Revenue)),
                (typeof(ProductVariant), nameof(ProductVariant.Price)),
                (typeof(ProductVariant), nameof(ProductVariant.CostPrice)),
                // HRM
                (typeof(Employee), nameof(Employee.BaseSalary)),
                (typeof(EmployeeContract), nameof(EmployeeContract.BaseSalary)),
                (typeof(EmployeeContract), nameof(EmployeeContract.Allowance)),
                (typeof(Payroll), nameof(Payroll.BaseSalary)),
                (typeof(Payroll), nameof(Payroll.ProRataSalary)),
                (typeof(Payroll), nameof(Payroll.OvertimePay)),
                (typeof(Payroll), nameof(Payroll.TotalAllowance)),
                (typeof(Payroll), nameof(Payroll.TotalBonus)),
                (typeof(Payroll), nameof(Payroll.GrossSalary)),
                (typeof(Payroll), nameof(Payroll.SocialInsurance)),
                (typeof(Payroll), nameof(Payroll.HealthInsurance)),
                (typeof(Payroll), nameof(Payroll.UnemploymentInsurance)),
                (typeof(Payroll), nameof(Payroll.IncomeTax)),
                (typeof(Payroll), nameof(Payroll.TotalDeduction)),
                (typeof(Payroll), nameof(Payroll.NetSalary)),
                (typeof(PayrollPeriod), nameof(PayrollPeriod.TotalGrossSalary)),
                (typeof(PayrollPeriod), nameof(PayrollPeriod.TotalNetSalary)),
                (typeof(PayrollDetail), nameof(PayrollDetail.Amount)),
                (typeof(KPI), nameof(KPI.Weight)),
                // Accounting
                (typeof(ChartOfAccount), nameof(ChartOfAccount.CurrentBalance)),
                (typeof(JournalEntry), nameof(JournalEntry.TotalDebit)),
                (typeof(JournalEntry), nameof(JournalEntry.TotalCredit)),
                (typeof(JournalEntryLine), nameof(JournalEntryLine.DebitAmount)),
                (typeof(JournalEntryLine), nameof(JournalEntryLine.CreditAmount)),
                (typeof(AccountReceivable), nameof(AccountReceivable.TotalAmount)),
                (typeof(AccountReceivable), nameof(AccountReceivable.PaidAmount)),
                (typeof(AccountPayable), nameof(AccountPayable.TotalAmount)),
                (typeof(AccountPayable), nameof(AccountPayable.PaidAmount)),
                (typeof(BankAccount), nameof(BankAccount.CurrentBalance)),
                (typeof(BankTransaction), nameof(BankTransaction.Amount)),
                (typeof(BankTransaction), nameof(BankTransaction.BalanceAfter)),
                (typeof(Budget), nameof(Budget.TotalBudgetAmount)),
                (typeof(Budget), nameof(Budget.TotalActualAmount)),
                (typeof(BudgetLine), nameof(BudgetLine.BudgetAmount)),
                (typeof(BudgetLine), nameof(BudgetLine.ActualAmount)),
                (typeof(TaxRate), nameof(TaxRate.Rate)),
                (typeof(TaxTransaction), nameof(TaxTransaction.BaseAmount)),
                (typeof(TaxTransaction), nameof(TaxTransaction.TaxAmount)),
                // SCM
                (typeof(Vendor), nameof(Vendor.CreditLimit)),
                (typeof(Vendor), nameof(Vendor.CurrentDebt)),
                (typeof(PurchaseOrder), nameof(PurchaseOrder.SubTotal)),
                (typeof(PurchaseOrder), nameof(PurchaseOrder.TaxAmount)),
                (typeof(PurchaseOrder), nameof(PurchaseOrder.DiscountAmount)),
                (typeof(PurchaseOrder), nameof(PurchaseOrder.ShippingFee)),
                (typeof(PurchaseOrder), nameof(PurchaseOrder.TotalAmount)),
                (typeof(PurchaseOrderItem), nameof(PurchaseOrderItem.UnitPrice)),
                (typeof(PurchaseOrderItem), nameof(PurchaseOrderItem.LineTotal)),
                (typeof(PurchaseInvoice), nameof(PurchaseInvoice.SubTotal)),
                (typeof(PurchaseInvoice), nameof(PurchaseInvoice.TaxAmount)),
                (typeof(PurchaseInvoice), nameof(PurchaseInvoice.TotalAmount)),
                (typeof(PurchaseInvoice), nameof(PurchaseInvoice.PaidAmount)),
                (typeof(VendorPayment), nameof(VendorPayment.Amount)),
                (typeof(ReturnToVendor), nameof(ReturnToVendor.TotalAmount)),
                (typeof(ReturnToVendor), nameof(ReturnToVendor.RefundAmount)),
                (typeof(ReturnToVendorItem), nameof(ReturnToVendorItem.UnitPrice)),
                (typeof(VendorPriceListItem), nameof(VendorPriceListItem.UnitPrice)),
                // PM
                (typeof(Project), nameof(Project.Budget)),
                (typeof(Project), nameof(Project.ActualCost)),
                (typeof(ProjectExpense), nameof(ProjectExpense.Amount)),
                (typeof(TimesheetEntry), nameof(TimesheetEntry.Hours)),
                (typeof(Timesheet), nameof(Timesheet.TotalHours))
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
                return (context.GetType(), dbContext.SchemaName, designTime);
            }
            return (context.GetType(), designTime);
        }

        public object Create(DbContext context) => Create(context, false);
    }
}
