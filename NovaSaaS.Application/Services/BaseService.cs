using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace NovaSaaS.Application.Services
{
    /// <summary>
    /// Base Service - Lớp nghiệp vụ cơ sở cho tất cả entities.
    /// 
    /// Thay vì viết 27 services riêng lẻ, bạn chỉ cần kế thừa:
    /// public class ProductService : BaseService<Product> { }
    /// 
    /// Hỗ trợ:
    /// - CRUD operations cơ bản
    /// - Validation hooks (override để thêm logic)
    /// - Audit logging tự động
    /// </summary>
    public abstract class BaseService<TEntity> where TEntity : BaseEntity
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IRepository<TEntity> _repository;
        protected readonly ICurrentUserService _currentUser;

        protected BaseService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _repository = unitOfWork.Repository<TEntity>();
            _currentUser = currentUser;
        }

        #region Query Operations

        /// <summary>
        /// Lấy entity theo ID.
        /// </summary>
        public virtual async Task<TEntity?> GetByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }

        /// <summary>
        /// Lấy entity theo ID với Include.
        /// </summary>
        public virtual async Task<TEntity?> GetByIdAsync(Guid id, params Expression<Func<TEntity, object>>[] includes)
        {
            return await _repository.GetByIdAsync(id, includes);
        }

        /// <summary>
        /// Lấy tất cả entities.
        /// </summary>
        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        /// <summary>
        /// Lấy tất cả entities với Include.
        /// </summary>
        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(params Expression<Func<TEntity, object>>[] includes)
        {
            return await _repository.GetAllAsync(includes);
        }

        /// <summary>
        /// Tìm kiếm theo điều kiện.
        /// </summary>
        public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _repository.FindAsync(predicate);
        }

        /// <summary>
        /// Đếm số lượng.
        /// </summary>
        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null)
        {
            return await _repository.CountAsync(predicate);
        }

        /// <summary>
        /// Kiểm tra tồn tại.
        /// </summary>
        public virtual async Task<bool> ExistsAsync(Guid id)
        {
            return await _repository.AnyAsync(e => e.Id == id);
        }

        #endregion

        #region Command Operations

        /// <summary>
        /// Thêm entity mới.
        /// </summary>
        public virtual async Task<TEntity> CreateAsync(TEntity entity)
        {
            // Validation hook
            await ValidateCreateAsync(entity);

            // Set audit fields
            entity.CreatedBy = _currentUser.UserId;
            entity.CreateAt = DateTime.UtcNow;

            _repository.Add(entity);
            await _unitOfWork.CompleteAsync();

            return entity;
        }

        /// <summary>
        /// Cập nhật entity.
        /// </summary>
        public virtual async Task<TEntity> UpdateAsync(TEntity entity)
        {
            // Validation hook
            await ValidateUpdateAsync(entity);

            // Set audit fields
            entity.UpdatedBy = _currentUser.UserId;
            entity.UpdateAt = DateTime.UtcNow;

            _repository.Update(entity);
            await _unitOfWork.CompleteAsync();

            return entity;
        }

        /// <summary>
        /// Xóa mềm entity (Soft Delete).
        /// </summary>
        public virtual async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return false;

            // Validation hook
            await ValidateDeleteAsync(entity);

            _repository.SoftDelete(entity);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        /// <summary>
        /// Xóa cứng entity (Hard Delete).
        /// </summary>
        public virtual async Task<bool> HardDeleteAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return false;

            _repository.Remove(entity);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        /// <summary>
        /// Khôi phục entity đã xóa mềm.
        /// </summary>
        public virtual async Task<bool> RestoreAsync(Guid id)
        {
            // Cần query bao gồm cả deleted entities
            var entity = await _repository.FirstOrDefaultAsync(e => e.Id == id);
            if (entity == null)
                return false;

            _repository.Restore(entity);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        #endregion

        #region Validation Hooks (Override để thêm logic)

        /// <summary>
        /// Hook validation khi tạo mới. Override để thêm logic.
        /// </summary>
        protected virtual Task ValidateCreateAsync(TEntity entity)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Hook validation khi cập nhật. Override để thêm logic.
        /// </summary>
        protected virtual Task ValidateUpdateAsync(TEntity entity)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Hook validation khi xóa. Override để thêm logic.
        /// </summary>
        protected virtual Task ValidateDeleteAsync(TEntity entity)
        {
            return Task.CompletedTask;
        }

        #endregion
    }
}
