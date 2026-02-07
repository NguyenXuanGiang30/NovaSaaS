using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaSaaS.Application.DTOs.Business;
using NovaSaaS.Application.Services.Business;

namespace NovaSaaSWebAPI.Controllers.Business
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LoyaltyController : ControllerBase
    {
        private readonly LoyaltyService _loyaltyService;

        public LoyaltyController(LoyaltyService loyaltyService)
        {
            _loyaltyService = loyaltyService;
        }

        #region Programs

        /// <summary>
        /// Lấy danh sách chương trình loyalty.
        /// </summary>
        [HttpGet("programs")]
        public async Task<IActionResult> GetAllPrograms()
        {
            var result = await _loyaltyService.GetAllProgramsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết chương trình loyalty.
        /// </summary>
        [HttpGet("programs/{id}")]
        public async Task<IActionResult> GetProgramById(Guid id)
        {
            var program = await _loyaltyService.GetProgramByIdAsync(id);
            if (program == null) return NotFound();
            return Ok(program);
        }

        /// <summary>
        /// Tạo chương trình loyalty mới.
        /// </summary>
        [HttpPost("programs")]
        public async Task<IActionResult> CreateProgram([FromBody] CreateLoyaltyProgramDto dto)
        {
            try
            {
                var id = await _loyaltyService.CreateProgramAsync(dto);
                return CreatedAtAction(nameof(GetProgramById), new { id }, new { id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật chương trình loyalty.
        /// </summary>
        [HttpPut("programs/{id}")]
        public async Task<IActionResult> UpdateProgram(Guid id, [FromBody] UpdateLoyaltyProgramDto dto)
        {
            try
            {
                await _loyaltyService.UpdateProgramAsync(id, dto);
                var program = await _loyaltyService.GetProgramByIdAsync(id);
                return Ok(program);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        #endregion

        #region Transactions

        /// <summary>
        /// Tích điểm cho khách hàng.
        /// </summary>
        [HttpPost("earn")]
        public async Task<IActionResult> EarnPoints([FromBody] EarnPointsRequest request)
        {
            try
            {
                await _loyaltyService.EarnPointsAsync(
                    request.ProgramId, request.CustomerId,
                    request.OrderAmount, request.ReferenceCode, request.ReferenceId);
                return Ok(new { message = "Tích điểm thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Đổi điểm cho khách hàng.
        /// </summary>
        [HttpPost("redeem")]
        public async Task<IActionResult> RedeemPoints([FromBody] RedeemPointsRequest request)
        {
            try
            {
                var discountValue = await _loyaltyService.RedeemPointsAsync(
                    request.ProgramId, request.CustomerId, request.Points);
                return Ok(new { discountValue, message = $"Đổi thành công, giảm {discountValue:N0} VND." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy lịch sử giao dịch điểm của khách hàng.
        /// </summary>
        [HttpGet("transactions/customer/{customerId}")]
        public async Task<IActionResult> GetTransactions(Guid customerId)
        {
            var result = await _loyaltyService.GetTransactionsByCustomerAsync(customerId);
            return Ok(result);
        }

        #endregion
    }

    // Request DTOs for Loyalty endpoints
    public class EarnPointsRequest
    {
        public Guid ProgramId { get; set; }
        public Guid CustomerId { get; set; }
        public decimal OrderAmount { get; set; }
        public string? ReferenceCode { get; set; }
        public Guid? ReferenceId { get; set; }
    }

    public class RedeemPointsRequest
    {
        public Guid ProgramId { get; set; }
        public Guid CustomerId { get; set; }
        public int Points { get; set; }
    }
}
