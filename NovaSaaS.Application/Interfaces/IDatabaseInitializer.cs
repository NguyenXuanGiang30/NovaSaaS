using System;
using System.Threading.Tasks;

namespace NovaSaaS.Application.Interfaces
{
    public interface IDatabaseInitializer
    {
        /// <summary>
        /// Khởi tạo database cho Tenant mới: Tạo Schema, chạy Migration và Seed dữ liệu.
        /// </summary>
        Task InitializeTenantAsync(string schemaName, Guid tenantId, string adminEmail, string adminPassword);
    }
}
