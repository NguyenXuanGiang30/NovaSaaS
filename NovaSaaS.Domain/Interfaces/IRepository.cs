using NovaSaaS.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace NovaSaaS.Domain.Interfaces
{
    /// <summary>
    /// Generic Repository Interface - "Khuôn đúc" cho 27 bảng.
    /// Cung cấp các thao tác CRUD cơ bản và nâng cao.
    /// </summary>
    /// <typeparam name="T">Entity type kế thừa từ BaseEntity</typeparam>
    public interface IRepository<T> where T : BaseEntity
    {
        #region Query Operations

        /// <summary>
        /// Lấy entity theo ID.
        /// </summary>
        Task<T?> GetByIdAsync(Guid id);

        /// <summary>
        /// Lấy entity theo ID với các bảng liên quan (Include).
        /// </summary>
        Task<T?> GetByIdAsync(Guid id, params Expression<Func<T, object?>>[] includes);

        /// <summary>
        /// Lấy tất cả entities (không bao gồm đã xóa mềm).
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Lấy tất cả entities với Include.
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object?>>[] includes);

        /// <summary>
        /// Lấy entities theo điều kiện.
        /// </summary>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Lấy entities theo điều kiện với Include.
        /// </summary>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object?>>[] includes);

        /// <summary>
        /// Lấy entity đầu tiên theo điều kiện.
        /// </summary>
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Kiểm tra có tồn tại entity nào thỏa điều kiện không.
        /// </summary>
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Đếm số lượng entity thỏa điều kiện.
        /// </summary>
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

        /// <summary>
        /// Trả về IQueryable để xây dựng query phức tạp.
        /// </summary>
        IQueryable<T> Query();

        /// <summary>
        /// Trả về IQueryable không tracking (ReadOnly).
        /// </summary>
        IQueryable<T> QueryNoTracking();

        #endregion

        #region Command Operations

        /// <summary>
        /// Thêm entity mới.
        /// </summary>
        void Add(T entity);

        /// <summary>
        /// Thêm nhiều entities.
        /// </summary>
        void AddRange(IEnumerable<T> entities);

        /// <summary>
        /// Cập nhật entity.
        /// </summary>
        void Update(T entity);

        /// <summary>
        /// Cập nhật nhiều entities.
        /// </summary>
        void UpdateRange(IEnumerable<T> entities);

        /// <summary>
        /// Xóa entity (Hard Delete).
        /// </summary>
        void Remove(T entity);

        /// <summary>
        /// Xóa nhiều entities.
        /// </summary>
        void RemoveRange(IEnumerable<T> entities);

        /// <summary>
        /// Xóa mềm entity (Soft Delete).
        /// </summary>
        void SoftDelete(T entity);

        /// <summary>
        /// Khôi phục entity đã xóa mềm.
        /// </summary>
        void Restore(T entity);

        #endregion

        #region Save Operations

        /// <summary>
        /// Lưu thay đổi vào database.
        /// </summary>
        Task<int> SaveChangesAsync();

        #endregion
    }
}
