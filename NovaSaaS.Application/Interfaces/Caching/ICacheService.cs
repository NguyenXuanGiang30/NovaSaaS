using System;
using System.Threading.Tasks;

namespace NovaSaaS.Application.Interfaces.Caching
{
    /// <summary>
    /// ICacheService - Interface cho bộ nhớ đệm.
    /// Hỗ trợ multi-tenant caching với key patterns.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Lấy giá trị từ cache.
        /// </summary>
        Task<T?> GetAsync<T>(string key) where T : class;

        /// <summary>
        /// Lưu giá trị vào cache với TTL.
        /// </summary>
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class;

        /// <summary>
        /// Xóa một key khỏi cache.
        /// </summary>
        Task RemoveAsync(string key);

        /// <summary>
        /// Xóa tất cả keys theo pattern (ví dụ: tenant:demo:*).
        /// </summary>
        Task RemoveByPatternAsync(string pattern);

        /// <summary>
        /// Kiểm tra key tồn tại.
        /// </summary>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// Lấy giá trị từ cache, nếu không có thì load từ factory và cache lại.
        /// </summary>
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null) where T : class;
    }
}
