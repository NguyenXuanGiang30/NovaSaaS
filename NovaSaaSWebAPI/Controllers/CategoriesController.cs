using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaSaaS.Application.DTOs.Inventory;
using NovaSaaS.Application.Services.Inventory;
using System;
using System.Threading.Tasks;

namespace NovaSaaS.WebAPI.Controllers
{
    /// <summary>
    /// API quản lý danh mục sản phẩm.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly CategoryService _categoryService;

        public CategoriesController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Lấy danh sách danh mục (cây).
        /// </summary>
        [HttpGet("tree")]
        public async Task<IActionResult> GetCategoryTree()
        {
            var result = await _categoryService.GetCategoryTreeAsync();
            return Ok(result);
        }

        /// <summary>
        /// Lấy tất cả danh mục (flat list).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _categoryService.GetAllCategoriesAsync();
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh mục theo ID.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
                return NotFound(new { message = "Danh mục không tồn tại" });

            return Ok(category);
        }

        /// <summary>
        /// Tạo danh mục mới.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _categoryService.CreateCategoryAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật danh mục.
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _categoryService.UpdateCategoryAsync(id, dto);
                if (result == null)
                    return NotFound(new { message = "Danh mục không tồn tại" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Xóa danh mục (soft delete).
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _categoryService.DeleteAsync(id);
            if (!success)
                return NotFound(new { message = "Danh mục không tồn tại" });

            return NoContent();
        }
    }
}
