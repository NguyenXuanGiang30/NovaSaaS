using System.Threading.Tasks;
using NovaSaaS.Application.DTOs;

namespace NovaSaaS.Application.Interfaces
{
    /// <summary>
    /// Service điều phối quy trình đăng ký Tenant mới.
    /// "Bộ phận điều hành và quản lý hợp đồng" trong kiến trúc NovaSaaS.
    /// 
    /// 4 Trách nhiệm chính:
    /// 1. Validation - Kiểm tra tính hợp lệ của dữ liệu đầu vào
    /// 2. Master Data Management - Ghi danh vào bảng Tenants
    /// 3. Infrastructure Triggering - Gọi DatabaseInitializer
    /// 4. Super User Setup - Tạo Admin đầu tiên
    /// </summary>
    public interface ITenantRegistrationService
    {
        /// <summary>
        /// Đăng ký một Tenant mới vào hệ thống.
        /// </summary>
        /// <param name="dto">Thông tin đăng ký</param>
        /// <returns>Kết quả đăng ký với thông tin chi tiết</returns>
        Task<RegistrationResult> RegisterAsync(RegisterTenantDto dto);

        /// <summary>
        /// Kiểm tra subdomain có khả dụng không.
        /// </summary>
        /// <param name="subdomain">Subdomain cần kiểm tra</param>
        /// <returns>True nếu subdomain có thể sử dụng</returns>
        Task<bool> IsSubdomainAvailableAsync(string subdomain);
    }
}
