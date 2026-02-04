namespace NovaSaaS.Application.Interfaces.Caching
{
    /// <summary>
    /// CacheKeys - Helper class cho cache key patterns.
    /// Đảm bảo multi-tenant isolation.
    /// </summary>
    public static class CacheKeys
    {
        private const string TenantPrefix = "tenant";
        private const string GlobalPrefix = "global";

        #region Tenant-Scoped Keys

        /// <summary>
        /// Key cho thông tin Tenant: tenant:{tenantId}:info
        /// TTL: 5 phút
        /// </summary>
        public static string TenantInfo(string tenantId) 
            => $"{TenantPrefix}:{tenantId}:info";

        /// <summary>
        /// Key cho danh sách Categories: tenant:{tenantId}:categories
        /// TTL: 30 phút
        /// </summary>
        public static string Categories(string tenantId) 
            => $"{TenantPrefix}:{tenantId}:categories";

        /// <summary>
        /// Key cho danh sách Units: tenant:{tenantId}:units
        /// TTL: 30 phút
        /// </summary>
        public static string Units(string tenantId) 
            => $"{TenantPrefix}:{tenantId}:units";

        /// <summary>
        /// Key cho danh sách Warehouses: tenant:{tenantId}:warehouses
        /// TTL: 30 phút
        /// </summary>
        public static string Warehouses(string tenantId) 
            => $"{TenantPrefix}:{tenantId}:warehouses";

        /// <summary>
        /// Key cho danh sách Products: tenant:{tenantId}:products
        /// TTL: 15 phút
        /// </summary>
        public static string Products(string tenantId) 
            => $"{TenantPrefix}:{tenantId}:products";

        /// <summary>
        /// Key cho User permissions: tenant:{tenantId}:user:{userId}:permissions
        /// TTL: 10 phút
        /// </summary>
        public static string UserPermissions(string tenantId, string userId) 
            => $"{TenantPrefix}:{tenantId}:user:{userId}:permissions";

        /// <summary>
        /// Key cho User info: tenant:{tenantId}:user:{userId}:info
        /// TTL: 10 phút
        /// </summary>
        public static string UserInfo(string tenantId, string userId) 
            => $"{TenantPrefix}:{tenantId}:user:{userId}:info";

        /// <summary>
        /// Key cho TenantSettings: tenant:{tenantId}:settings
        /// TTL: 30 phút
        /// </summary>
        public static string TenantSettings(string tenantId) 
            => $"{TenantPrefix}:{tenantId}:settings";

        /// <summary>
        /// Pattern để invalidate tất cả cache của một tenant.
        /// </summary>
        public static string TenantPattern(string tenantId) 
            => $"{TenantPrefix}:{tenantId}:*";

        #endregion

        #region Global Keys

        /// <summary>
        /// Key cho danh sách Subscription Plans: global:plans
        /// TTL: 60 phút
        /// </summary>
        public static string SubscriptionPlans() 
            => $"{GlobalPrefix}:plans";

        /// <summary>
        /// Key cho danh sách Roles: global:roles
        /// TTL: 60 phút
        /// </summary>
        public static string Roles() 
            => $"{GlobalPrefix}:roles";

        /// <summary>
        /// Key cho danh sách Permissions: global:permissions
        /// TTL: 60 phút
        /// </summary>
        public static string Permissions() 
            => $"{GlobalPrefix}:permissions";

        #endregion

        #region TTL Defaults

        /// <summary>
        /// TTL cho thông tin tenant (5 phút).
        /// </summary>
        public static readonly TimeSpan TenantInfoTTL = TimeSpan.FromMinutes(5);

        /// <summary>
        /// TTL cho dữ liệu master (categories, units) (30 phút).
        /// </summary>
        public static readonly TimeSpan MasterDataTTL = TimeSpan.FromMinutes(30);

        /// <summary>
        /// TTL cho products (15 phút).
        /// </summary>
        public static readonly TimeSpan ProductsTTL = TimeSpan.FromMinutes(15);

        /// <summary>
        /// TTL cho user permissions (10 phút).
        /// </summary>
        public static readonly TimeSpan UserPermissionsTTL = TimeSpan.FromMinutes(10);

        /// <summary>
        /// TTL cho global data (plans, roles) (60 phút).
        /// </summary>
        public static readonly TimeSpan GlobalDataTTL = TimeSpan.FromMinutes(60);

        #endregion
    }
}
