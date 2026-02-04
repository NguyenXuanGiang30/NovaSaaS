using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaSaaS.Domain.Entities.Business;
using NovaSaaS.Domain.Interfaces;
using NovaSaaS.WebApi.Authorization;
using NovaSaaS.WebApi.Models;

namespace NovaSaaS.WebApi.Controllers
{
    /// <summary>
    /// Coupons Controller - Quản lý mã giảm giá.
    /// CRUD và validation logic cho Coupon.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Tags("Settings")]
    public class CouponsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CouponsController> _logger;

        public CouponsController(
            IUnitOfWork unitOfWork,
            ILogger<CouponsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách mã giảm giá.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<CouponDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCoupons([FromQuery] bool? activeOnly = null)
        {
            var coupons = await _unitOfWork.Coupons.GetAllAsync();

            if (activeOnly == true)
            {
                coupons = coupons.Where(c => !c.IsDeleted && c.ExpiryDate > DateTime.UtcNow);
            }

            var result = coupons.Select(MapToDto).ToList();
            return Ok(ApiResponse<List<CouponDto>>.SuccessResult(result));
        }

        /// <summary>
        /// Lấy chi tiết một mã giảm giá.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<CouponDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCoupon(Guid id)
        {
            var coupon = await _unitOfWork.Coupons.GetByIdAsync(id);
            if (coupon == null)
            {
                return NotFound(ApiResponse<object>.FailResult("Không tìm thấy mã giảm giá."));
            }

            return Ok(ApiResponse<CouponDto>.SuccessResult(MapToDto(coupon)));
        }

        /// <summary>
        /// Kiểm tra mã giảm giá có thể áp dụng không.
        /// </summary>
        [HttpGet("validate/{code}")]
        [ProducesResponseType(typeof(ApiResponse<CouponValidationResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ValidateCoupon(string code, [FromQuery] decimal orderAmount = 0)
        {
            var coupon = await _unitOfWork.Coupons.FirstOrDefaultAsync(c => c.Code == code.ToUpper());

            if (coupon == null)
            {
                return Ok(ApiResponse<CouponValidationResult>.SuccessResult(new CouponValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Mã giảm giá không tồn tại."
                }));
            }

            var validation = ValidateCouponInternal(coupon, orderAmount);
            return Ok(ApiResponse<CouponValidationResult>.SuccessResult(validation));
        }

        /// <summary>
        /// Tạo mã giảm giá mới.
        /// </summary>
        [HttpPost]
        [RequirePermission("settings.manage")]
        [ProducesResponseType(typeof(ApiResponse<CouponDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCoupon([FromBody] CreateCouponRequest request)
        {
            // Check trùng code
            var existing = await _unitOfWork.Coupons.FirstOrDefaultAsync(c => c.Code == request.Code.ToUpper());
            if (existing != null)
            {
                return BadRequest(ApiResponse<object>.FailResult("Mã giảm giá này đã tồn tại."));
            }

            var coupon = new Coupon
            {
                Code = request.Code.ToUpper(),
                DiscountValue = request.DiscountValue,
                ExpiryDate = request.ExpiryDate
            };

            _unitOfWork.Coupons.Add(coupon);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Created coupon: {Code}", coupon.Code);

            return CreatedAtAction(nameof(GetCoupon), new { id = coupon.Id },
                ApiResponse<CouponDto>.SuccessResult(MapToDto(coupon), "Đã tạo mã giảm giá."));
        }

        /// <summary>
        /// Cập nhật mã giảm giá.
        /// </summary>
        [HttpPut("{id}")]
        [RequirePermission("settings.manage")]
        [ProducesResponseType(typeof(ApiResponse<CouponDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCoupon(Guid id, [FromBody] UpdateCouponRequest request)
        {
            var coupon = await _unitOfWork.Coupons.GetByIdAsync(id);
            if (coupon == null)
            {
                return NotFound(ApiResponse<object>.FailResult("Không tìm thấy mã giảm giá."));
            }

            if (request.DiscountValue.HasValue)
                coupon.DiscountValue = request.DiscountValue.Value;
            if (request.ExpiryDate.HasValue)
                coupon.ExpiryDate = request.ExpiryDate.Value;

            _unitOfWork.Coupons.Update(coupon);
            await _unitOfWork.CompleteAsync();

            return Ok(ApiResponse<CouponDto>.SuccessResult(MapToDto(coupon), "Đã cập nhật mã giảm giá."));
        }

        /// <summary>
        /// Xóa mã giảm giá.
        /// </summary>
        [HttpDelete("{id}")]
        [RequirePermission("settings.manage")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCoupon(Guid id)
        {
            var coupon = await _unitOfWork.Coupons.GetByIdAsync(id);
            if (coupon == null)
            {
                return NotFound(ApiResponse<object>.FailResult("Không tìm thấy mã giảm giá."));
            }

            _unitOfWork.Coupons.SoftDelete(coupon);
            await _unitOfWork.CompleteAsync();

            return Ok(ApiResponse<bool>.SuccessResult(true, "Đã xóa mã giảm giá."));
        }

        #region Private Helpers

        private CouponDto MapToDto(Coupon coupon)
        {
            return new CouponDto
            {
                Id = coupon.Id,
                Code = coupon.Code,
                DiscountValue = coupon.DiscountValue,
                ExpiryDate = coupon.ExpiryDate,
                IsExpired = coupon.ExpiryDate < DateTime.UtcNow,
                CreatedAt = coupon.CreateAt
            };
        }

        private CouponValidationResult ValidateCouponInternal(Coupon coupon, decimal orderAmount)
        {
            if (DateTime.UtcNow > coupon.ExpiryDate)
            {
                return new CouponValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Mã giảm giá đã hết hạn."
                };
            }

            if (coupon.IsDeleted)
            {
                return new CouponValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Mã giảm giá không còn hiệu lực."
                };
            }

            // Calculate discount (treat DiscountValue as a fixed amount)
            decimal discountAmount = coupon.DiscountValue;

            // Don't exceed order amount
            if (discountAmount > orderAmount)
            {
                discountAmount = orderAmount;
            }

            return new CouponValidationResult
            {
                IsValid = true,
                DiscountAmount = discountAmount,
                FinalAmount = orderAmount - discountAmount,
                Message = $"Giảm {discountAmount:N0}"
            };
        }

        #endregion
    }

    #region DTOs

    public class CouponDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsExpired { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCouponRequest
    {
        public string Code { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public DateTime ExpiryDate { get; set; }
    }

    public class UpdateCouponRequest
    {
        public decimal? DiscountValue { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    public class CouponValidationResult
    {
        public bool IsValid { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }
    }

    #endregion
}
