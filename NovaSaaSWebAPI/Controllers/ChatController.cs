using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovaSaaS.Application.Interfaces.AI;
using NovaSaaS.Application.Services.AI;
using NovaSaaS.Domain.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NovaSaaSWebAPI.Controllers
{
    /// <summary>
    /// API Chat với AI (RAG).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly RAGService _ragService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAIFunctionService _functionService;

        public ChatController(
            RAGService ragService, 
            IUnitOfWork unitOfWork,
            IAIFunctionService functionService)
        {
            _ragService = ragService;
            _unitOfWork = unitOfWork;
            _functionService = functionService;
        }

        /// <summary>
        /// Đặt câu hỏi cho AI.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
                return BadRequest("Vui lòng nhập câu hỏi");

            try
            {
                var response = await _ragService.AskAsync(request.Question);

                return Ok(new
                {
                    response.Answer,
                    response.ConfidenceScore,
                    response.ResponseTimeMs,
                    SourceCount = response.Sources.Count,
                    Sources = response.Sources.Select(s => new
                    {
                        s.SegmentId,
                        s.DocumentId,
                        ContentPreview = s.Content.Length > 200 ? s.Content[..200] + "..." : s.Content,
                        s.SimilarityScore
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi khi xử lý câu hỏi", Error = ex.Message });
            }
        }

        /// <summary>
        /// Đặt câu hỏi cho AI với khả năng gọi functions để thực hiện các hành động nghiệp vụ.
        /// AI có thể tự động kiểm tra tồn kho, tra cứu đơn hàng, lấy thông tin khách hàng, v.v.
        /// </summary>
        [HttpPost("with-functions")]
        public async Task<IActionResult> AskWithFunctions([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
                return BadRequest("Vui lòng nhập câu hỏi");

            try
            {
                var answer = await _functionService.ChatWithFunctionsAsync(request.Question);

                return Ok(new
                {
                    Answer = answer,
                    Mode = "function-calling",
                    AvailableFunctions = _functionService.GetAvailableFunctions().Select(f => new
                    {
                        f.Name,
                        f.Description
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi khi xử lý câu hỏi", Error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách các functions AI có thể gọi.
        /// </summary>
        [HttpGet("functions")]
        public IActionResult GetAvailableFunctions()
        {
            var functions = _functionService.GetAvailableFunctions();
            return Ok(functions.Select(f => new
            {
                f.Name,
                f.Description,
                f.Parameters
            }));
        }

        /// <summary>
        /// Thực thi trực tiếp một function (dùng cho testing/debugging).
        /// </summary>
        [HttpPost("functions/{functionName}/execute")]
        public async Task<IActionResult> ExecuteFunction(string functionName, [FromBody] object arguments)
        {
            try
            {
                var argsJson = System.Text.Json.JsonSerializer.Serialize(arguments);
                var result = await _functionService.ExecuteFunctionAsync(functionName, argsJson);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi khi thực thi function", Error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy lịch sử chat.
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var history = await _unitOfWork.ChatHistories
                .Query()
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.CreateAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new
                {
                    c.Id,
                    c.Question,
                    c.Answer,
                    c.ConfidenceScore,
                    c.ResponseTimeMs,
                    c.RetrievedCount,
                    c.UserRating,
                    c.CreateAt
                })
                .ToListAsync();

            var total = await _unitOfWork.ChatHistories
                .Query()
                .CountAsync(c => !c.IsDeleted);

            return Ok(new
            {
                Data = history,
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            });
        }

        /// <summary>
        /// Lấy chi tiết một chat.
        /// </summary>
        [HttpGet("history/{id}")]
        public async Task<IActionResult> GetHistoryById(Guid id)
        {
            var chat = await _unitOfWork.ChatHistories.GetByIdAsync(id);
            if (chat == null)
                return NotFound("Không tìm thấy");

            return Ok(new
            {
                chat.Id,
                chat.Question,
                chat.Answer,
                chat.RetrievedSegmentIds,
                chat.RetrievedCount,
                chat.ConfidenceScore,
                chat.ResponseTimeMs,
                chat.PromptTokens,
                chat.CompletionTokens,
                chat.UserRating,
                chat.UserFeedback,
                chat.CreateAt
            });
        }

        /// <summary>
        /// Rate câu trả lời (1-5 sao).
        /// </summary>
        [HttpPost("history/{id}/rate")]
        public async Task<IActionResult> Rate(Guid id, [FromBody] RateRequest request)
        {
            var chat = await _unitOfWork.ChatHistories.GetByIdAsync(id);
            if (chat == null)
                return NotFound("Không tìm thấy");

            if (request.Rating < 1 || request.Rating > 5)
                return BadRequest("Rating phải từ 1 đến 5");

            chat.UserRating = request.Rating;
            chat.UserFeedback = request.Feedback;
            _unitOfWork.ChatHistories.Update(chat);
            await _unitOfWork.CompleteAsync();

            return Ok(new { Message = "Đã lưu đánh giá" });
        }

        /// <summary>
        /// Xóa chat history.
        /// </summary>
        [HttpDelete("history/{id}")]
        public async Task<IActionResult> DeleteHistory(Guid id)
        {
            var chat = await _unitOfWork.ChatHistories.GetByIdAsync(id);
            if (chat == null)
                return NotFound("Không tìm thấy");

            _unitOfWork.ChatHistories.SoftDelete(chat);
            await _unitOfWork.CompleteAsync();

            return Ok(new { Message = "Đã xóa" });
        }
    }

    public class ChatRequest
    {
        public string Question { get; set; } = string.Empty;
    }

    public class RateRequest
    {
        public int Rating { get; set; }
        public string? Feedback { get; set; }
    }
}
