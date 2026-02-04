using Microsoft.EntityFrameworkCore;
using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace NovaSaaS.Infrastructure.Persistence
{
    /// <summary>
    /// Generic Repository Implementation - "Khuôn đúc" cho 27 bảng.
    /// 
    /// Tất cả các thao tác CRUD đều đi qua đây.
    /// Hỗ trợ:
    /// - Soft Delete tự động (filter entities đã xóa)
    /// - Include cho eager loading
    /// - IQueryable cho query phức tạp
    /// </summary>
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        #region Query Operations

        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null && entity.IsDeleted)
                return null;
            return entity;
        }

        public virtual async Task<T?> GetByIdAsync(Guid id, params Expression<Func<T, object?>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            var entity = await query.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            return entity;
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.Where(e => !e.IsDeleted).ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object?>>[] includes)
        {
            IQueryable<T> query = _dbSet.Where(e => !e.IsDeleted);

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(e => !e.IsDeleted).Where(predicate).ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate, 
            params Expression<Func<T, object?>>[] includes)
        {
            IQueryable<T> query = _dbSet.Where(e => !e.IsDeleted).Where(predicate);

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.ToListAsync();
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(e => !e.IsDeleted).FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(e => !e.IsDeleted).AnyAsync(predicate);
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            var query = _dbSet.Where(e => !e.IsDeleted);
            
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return await query.CountAsync();
        }

        public virtual IQueryable<T> Query()
        {
            return _dbSet.Where(e => !e.IsDeleted);
        }

        public virtual IQueryable<T> QueryNoTracking()
        {
            return _dbSet.AsNoTracking().Where(e => !e.IsDeleted);
        }

        #endregion

        #region Command Operations

        public virtual void Add(T entity)
        {
            entity.CreateAt = DateTime.UtcNow;
            _dbSet.Add(entity);
        }

        public virtual void AddRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                entity.CreateAt = DateTime.UtcNow;
            }
            _dbSet.AddRange(entities);
        }

        public virtual void Update(T entity)
        {
            entity.UpdateAt = DateTime.UtcNow;
            _dbSet.Update(entity);
        }

        public virtual void UpdateRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                entity.UpdateAt = DateTime.UtcNow;
            }
            _dbSet.UpdateRange(entities);
        }

        public virtual void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        public virtual void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public virtual void SoftDelete(T entity)
        {
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
        }

        public virtual void Restore(T entity)
        {
            entity.IsDeleted = false;
            entity.DeletedAt = null;
            _dbSet.Update(entity);
        }

        #endregion

        #region Save Operations

        public virtual async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        #endregion
    }
}
