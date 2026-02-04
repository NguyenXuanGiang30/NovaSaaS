using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaSaaS.Application.DTOs.Inventory;
using NovaSaaS.Application.Services.Inventory;
using System;
using System.Threading.Tasks;

namespace NovaSaaS.WebAPI.Controllers
{
    /// <summary>
    /// API quản lý sản phẩm - Core của Inventory Module.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductsController(ProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Lấy danh sách sản phẩm.
        /// </summary>
        /// <param name="search">Tìm theo tên, SKU, barcode</param>
        /// <param name="categoryId">Lọc theo danh mục</param>
        /// <param name="isActive">Lọc theo trạng thái</param>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] Guid? categoryId = null,
            [FromQuery] bool? isActive = null)
        {
            var result = await _productService.GetProductListAsync(search, categoryId, isActive);
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết sản phẩm.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var product = await _productService.GetProductDetailAsync(id);
            if (product == null)
                return NotFound(new { message = "Sản phẩm không tồn tại" });

            return Ok(product);
        }

        /// <summary>
        /// Tạo sản phẩm mới.
        /// SKU sẽ tự động tạo nếu không cung cấp.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _productService.CreateProductAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật sản phẩm.
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _productService.UpdateProductAsync(id, dto);
                if (result == null)
                    return NotFound(new { message = "Sản phẩm không tồn tại" });

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Xóa sản phẩm (soft delete).
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _productService.DeleteAsync(id);
            if (!success)
                return NotFound(new { message = "Sản phẩm không tồn tại" });

            return NoContent();
        }

        /// <summary>
        /// Kiểm tra SKU có tồn tại không.
        /// </summary>
        [HttpGet("check-sku/{sku}")]
        public async Task<IActionResult> CheckSku(string sku)
        {
            var exists = await _productService.ExistsAsync(Guid.Empty); // TODO: Implement SKU check
            return Ok(new { exists });
        }
    }
}
