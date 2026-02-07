using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaSaaS.Application.DTOs.Business;
using NovaSaaS.Application.DTOs.Inventory;
using NovaSaaS.Application.Services.Inventory;
using System;
using System.Threading.Tasks;

namespace NovaSaaSWebAPI.Controllers.Inventory
{
    /// <summary>
    /// API quản lý biến thể sản phẩm.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductVariantsController : ControllerBase
    {
        private readonly ProductVariantService _productVariantService;

        public ProductVariantsController(ProductVariantService productVariantService)
        {
            _productVariantService = productVariantService;
        }

        /// <summary>
        /// Lấy danh sách biến thể theo sản phẩm.
        /// </summary>
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetByProductId(Guid productId)
        {
            var result = await _productVariantService.GetByProductIdAsync(productId);
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết biến thể.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var variant = await _productVariantService.GetByIdAsync(id);
            if (variant == null)
                return NotFound(new { message = "Biến thể không tồn tại" });

            return Ok(variant);
        }

        /// <summary>
        /// Tạo biến thể mới.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductVariantDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var id = await _productVariantService.CreateAsync(dto);
                var variant = await _productVariantService.GetByIdAsync(id);
                return CreatedAtAction(nameof(GetById), new { id }, variant);
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
        /// Cập nhật biến thể.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductVariantDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _productVariantService.UpdateAsync(id, dto);
                var variant = await _productVariantService.GetByIdAsync(id);
                if (variant == null)
                    return NotFound(new { message = "Biến thể không tồn tại" });

                return Ok(variant);
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
        /// Xóa biến thể.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _productVariantService.DeleteAsync(id);
            if (!success)
                return NotFound(new { message = "Biến thể không tồn tại" });

            return NoContent();
        }
    }
}
