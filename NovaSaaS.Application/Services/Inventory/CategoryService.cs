using NovaSaaS.Application.DTOs.Inventory;
using NovaSaaS.Domain.Entities.Inventory;
using NovaSaaS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NovaSaaS.Application.Services.Inventory
{
    /// <summary>
    /// CategoryService - Quản lý danh mục sản phẩm.
    /// </summary>
    public class CategoryService : BaseService<Category>
    {
        public CategoryService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
            : base(unitOfWork, currentUser)
        {
        }

        /// <summary>
        /// Lấy tất cả danh mục dưới dạng cây.
        /// </summary>
        public async Task<List<CategoryDto>> GetCategoryTreeAsync()
        {
            var categories = await _repository.GetAllAsync();

            // Lấy root categories (không có parent)
            var rootCategories = categories
                .Where(c => c.ParentId == null)
                .OrderBy(c => c.SortOrder)
                .Select(c => MapToDto(c, categories))
                .ToList();

            return rootCategories;
        }

        /// <summary>
        /// Lấy tất cả danh mục (flat list).
        /// </summary>
        public async Task<List<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _repository.GetAllAsync();
            return categories
                .OrderBy(c => c.SortOrder)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ParentId = c.ParentId,
                    ParentName = c.ParentId.HasValue 
                        ? categories.FirstOrDefault(p => p.Id == c.ParentId)?.Name 
                        : null,
                    SortOrder = c.SortOrder,
                    IsActive = c.IsActive,
                    CreateAt = c.CreateAt
                })
                .ToList();
        }

        /// <summary>
        /// Tạo danh mục mới.
        /// </summary>
        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
        {
            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description,
                ParentId = dto.ParentId,
                SortOrder = dto.SortOrder
            };

            await CreateAsync(category);

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ParentId = category.ParentId,
                SortOrder = category.SortOrder,
                IsActive = category.IsActive,
                CreateAt = category.CreateAt
            };
        }

        /// <summary>
        /// Cập nhật danh mục.
        /// </summary>
        public async Task<CategoryDto?> UpdateCategoryAsync(Guid id, UpdateCategoryDto dto)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null) return null;

            // Không cho phép set parent là chính nó
            if (dto.ParentId == id)
                throw new InvalidOperationException("Danh mục không thể là cha của chính nó");

            category.Name = dto.Name;
            category.Description = dto.Description;
            category.ParentId = dto.ParentId;
            category.SortOrder = dto.SortOrder;
            category.IsActive = dto.IsActive;

            await UpdateAsync(category);

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ParentId = category.ParentId,
                SortOrder = category.SortOrder,
                IsActive = category.IsActive,
                CreateAt = category.CreateAt
            };
        }

        /// <summary>
        /// Map Category to CategoryDto với children.
        /// </summary>
        private CategoryDto MapToDto(Category category, IEnumerable<Category> allCategories)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ParentId = category.ParentId,
                SortOrder = category.SortOrder,
                IsActive = category.IsActive,
                CreateAt = category.CreateAt,
                Children = allCategories
                    .Where(c => c.ParentId == category.Id)
                    .OrderBy(c => c.SortOrder)
                    .Select(c => MapToDto(c, allCategories))
                    .ToList()
            };
        }

        protected override Task ValidateCreateAsync(Category entity)
        {
            if (string.IsNullOrWhiteSpace(entity.Name))
                throw new ArgumentException("Tên danh mục không được để trống");
            return Task.CompletedTask;
        }
    }
}
