using Microsoft.Extensions.Logging;
using NovaSaaS.Application.Interfaces;
using NovaSaaS.Application.Interfaces.AI;
using NovaSaaS.Domain.Enums;
using NovaSaaS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace NovaSaaS.Application.Services.AI
{
    /// <summary>
    /// AIFunctionService - Cho phép AI thực hiện actions thông qua function calling.
    /// 
    /// Các functions được định nghĩa:
    /// - get_product_stock: Kiểm tra tồn kho sản phẩm
    /// - search_products: Tìm kiếm sản phẩm
    /// - get_customer_info: Lấy thông tin khách hàng
    /// - get_order_status: Kiểm tra trạng thái đơn hàng
    /// - get_sales_summary: Lấy tóm tắt doanh thu
    /// - create_stock_alert: Tạo cảnh báo tồn kho
    /// </summary>
    public class AIFunctionService : IAIFunctionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IChatCompletionService _chatService;
        private readonly ILogger<AIFunctionService> _logger;
        private readonly List<FunctionDefinition> _functions;

        public AIFunctionService(
            IUnitOfWork unitOfWork,
            IChatCompletionService chatService,
            ILogger<AIFunctionService> logger)
        {
            _unitOfWork = unitOfWork;
            _chatService = chatService;
            _logger = logger;
            _functions = BuildFunctionDefinitions();
        }

        private static List<FunctionDefinition> BuildFunctionDefinitions()
        {
            return new List<FunctionDefinition>
            {
                new FunctionDefinition
                {
                    Name = "get_product_stock",
                    Description = "Kiểm tra số lượng tồn kho của một sản phẩm theo SKU hoặc tên.",
                    Parameters = new Dictionary<string, object>
                    {
                        ["type"] = "object",
                        ["properties"] = new Dictionary<string, object>
                        {
                            ["product_sku"] = new Dictionary<string, string>
                            {
                                ["type"] = "string",
                                ["description"] = "Mã SKU của sản phẩm"
                            },
                            ["product_name"] = new Dictionary<string, string>
                            {
                                ["type"] = "string",
                                ["description"] = "Tên sản phẩm (dùng khi không có SKU)"
                            }
                        },
                        ["required"] = new[] { "product_sku" }
                    }
                },
                new FunctionDefinition
                {
                    Name = "search_products",
                    Description = "Tìm kiếm sản phẩm theo từ khóa, trả về danh sách sản phẩm phù hợp.",
                    Parameters = new Dictionary<string, object>
                    {
                        ["type"] = "object",
                        ["properties"] = new Dictionary<string, object>
                        {
                            ["keyword"] = new Dictionary<string, string>
                            {
                                ["type"] = "string",
                                ["description"] = "Từ khóa tìm kiếm"
                            },
                            ["limit"] = new Dictionary<string, object>
                            {
                                ["type"] = "integer",
                                ["description"] = "Số lượng kết quả tối đa",
                                ["default"] = 5
                            }
                        },
                        ["required"] = new[] { "keyword" }
                    }
                },
                new FunctionDefinition
                {
                    Name = "get_customer_info",
                    Description = "Lấy thông tin chi tiết của khách hàng theo mã hoặc tên.",
                    Parameters = new Dictionary<string, object>
                    {
                        ["type"] = "object",
                        ["properties"] = new Dictionary<string, object>
                        {
                            ["customer_code"] = new Dictionary<string, string>
                            {
                                ["type"] = "string",
                                ["description"] = "Mã khách hàng"
                            },
                            ["customer_name"] = new Dictionary<string, string>
                            {
                                ["type"] = "string",
                                ["description"] = "Tên khách hàng (tìm kiếm gần đúng)"
                            }
                        }
                    }
                },
                new FunctionDefinition
                {
                    Name = "get_order_status",
                    Description = "Kiểm tra trạng thái đơn hàng theo mã đơn hàng.",
                    Parameters = new Dictionary<string, object>
                    {
                        ["type"] = "object",
                        ["properties"] = new Dictionary<string, object>
                        {
                            ["order_number"] = new Dictionary<string, string>
                            {
                                ["type"] = "string",
                                ["description"] = "Mã đơn hàng"
                            }
                        },
                        ["required"] = new[] { "order_number" }
                    }
                },
                new FunctionDefinition
                {
                    Name = "get_sales_summary",
                    Description = "Lấy tóm tắt doanh thu theo khoảng thời gian.",
                    Parameters = new Dictionary<string, object>
                    {
                        ["type"] = "object",
                        ["properties"] = new Dictionary<string, object>
                        {
                            ["period"] = new Dictionary<string, object>
                            {
                                ["type"] = "string",
                                ["description"] = "Khoảng thời gian: today, week, month, quarter, year",
                                ["enum"] = new[] { "today", "week", "month", "quarter", "year" }
                            }
                        },
                        ["required"] = new[] { "period" }
                    }
                },
                new FunctionDefinition
                {
                    Name = "get_low_stock_products",
                    Description = "Lấy danh sách sản phẩm có tồn kho thấp (dưới mức tối thiểu).",
                    Parameters = new Dictionary<string, object>
                    {
                        ["type"] = "object",
                        ["properties"] = new Dictionary<string, object>
                        {
                            ["limit"] = new Dictionary<string, object>
                            {
                                ["type"] = "integer",
                                ["description"] = "Số lượng sản phẩm tối đa",
                                ["default"] = 10
                            }
                        }
                    }
                }
            };
        }

        public IReadOnlyList<FunctionDefinition> GetAvailableFunctions() => _functions.AsReadOnly();

        public async Task<FunctionCallResult> ExecuteFunctionAsync(string functionName, string argumentsJson)
        {
            _logger.LogInformation("Executing function: {FunctionName} with args: {Args}", functionName, argumentsJson);

            try
            {
                var args = string.IsNullOrEmpty(argumentsJson) 
                    ? new Dictionary<string, object?>() 
                    : JsonSerializer.Deserialize<Dictionary<string, object?>>(argumentsJson) ?? new();

                object? result = functionName switch
                {
                    "get_product_stock" => await ExecuteGetProductStock(args),
                    "search_products" => await ExecuteSearchProducts(args),
                    "get_customer_info" => await ExecuteGetCustomerInfo(args),
                    "get_order_status" => await ExecuteGetOrderStatus(args),
                    "get_sales_summary" => await ExecuteGetSalesSummary(args),
                    "get_low_stock_products" => await ExecuteGetLowStockProducts(args),
                    _ => throw new ArgumentException($"Unknown function: {functionName}")
                };

                return new FunctionCallResult
                {
                    FunctionName = functionName,
                    Arguments = args,
                    Result = result,
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing function {FunctionName}", functionName);
                return new FunctionCallResult
                {
                    FunctionName = functionName,
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<string> ChatWithFunctionsAsync(string userMessage, int maxIterations = 5)
        {
            var systemPrompt = BuildSystemPromptWithFunctions();
            var conversationHistory = new List<string>();
            conversationHistory.Add($"User: {userMessage}");

            for (int i = 0; i < maxIterations; i++)
            {
                var prompt = $"{systemPrompt}\n\n{string.Join("\n", conversationHistory)}\n\nAssistant:";
                var response = await _chatService.GenerateAsync(prompt);

                if (!response.IsSuccess)
                    return $"Lỗi: {response.ErrorMessage}";

                var content = response.Content;

                // Check if AI wants to call a function
                if (content.Contains("<function_call>") && content.Contains("</function_call>"))
                {
                    var functionCallJson = ExtractFunctionCall(content);
                    if (functionCallJson != null)
                    {
                        var (funcName, argsJson) = ParseFunctionCall(functionCallJson);
                        var result = await ExecuteFunctionAsync(funcName, argsJson);

                        conversationHistory.Add($"Assistant: [Calling function: {funcName}]");
                        conversationHistory.Add($"Function Result: {JsonSerializer.Serialize(result.Result ?? result.ErrorMessage)}");
                        continue;
                    }
                }

                // AI gave final answer
                return CleanResponse(content);
            }

            return "Đã đạt giới hạn số lần gọi function. Vui lòng thử lại với câu hỏi đơn giản hơn.";
        }

        #region Private Function Implementations

        private async Task<object> ExecuteGetProductStock(Dictionary<string, object?> args)
        {
            var sku = args.GetValueOrDefault("product_sku")?.ToString();
            var name = args.GetValueOrDefault("product_name")?.ToString();

            var products = await _unitOfWork.Products.FindAsync(p => 
                (!string.IsNullOrEmpty(sku) && p.SKU == sku) ||
                (!string.IsNullOrEmpty(name) && p.Name.Contains(name)) &&
                !p.IsDeleted);

            var product = products.FirstOrDefault();
            if (product == null)
                return new { found = false, message = "Không tìm thấy sản phẩm" };

            return new
            {
                found = true,
                product_name = product.Name,
                sku = product.SKU,
                stock_quantity = product.StockQuantity,
                min_stock_level = product.MinStockLevel,
                is_low_stock = product.StockQuantity <= product.MinStockLevel,
                unit = product.Unit
            };
        }

        private async Task<object> ExecuteSearchProducts(Dictionary<string, object?> args)
        {
            var keyword = args.GetValueOrDefault("keyword")?.ToString() ?? "";
            var limit = args.TryGetValue("limit", out var limitObj) && limitObj != null
                ? Convert.ToInt32(limitObj) : 5;

            var products = await _unitOfWork.Products.FindAsync(p =>
                (p.Name.Contains(keyword) || p.SKU.Contains(keyword) || (p.Description != null && p.Description.Contains(keyword))) &&
                p.IsActive && !p.IsDeleted);

            return products.Take(limit).Select(p => new
            {
                id = p.Id,
                name = p.Name,
                sku = p.SKU,
                price = p.Price,
                stock = p.StockQuantity
            }).ToList();
        }

        private async Task<object> ExecuteGetCustomerInfo(Dictionary<string, object?> args)
        {
            var taxCode = args.GetValueOrDefault("customer_code")?.ToString();
            var name = args.GetValueOrDefault("customer_name")?.ToString();

            var customers = await _unitOfWork.Customers.FindAsync(c =>
                (!string.IsNullOrEmpty(taxCode) && c.TaxCode == taxCode) ||
                (!string.IsNullOrEmpty(name) && c.Name.Contains(name)) &&
                !c.IsDeleted);

            var customer = customers.FirstOrDefault();
            if (customer == null)
                return new { found = false, message = "Không tìm thấy khách hàng" };

            return new
            {
                found = true,
                tax_code = customer.TaxCode,
                name = customer.Name,
                email = customer.Email,
                phone = customer.Phone,
                address = customer.Address,
                total_spending = customer.TotalSpending
            };
        }

        private async Task<object> ExecuteGetOrderStatus(Dictionary<string, object?> args)
        {
            var orderNumber = args.GetValueOrDefault("order_number")?.ToString();
            if (string.IsNullOrEmpty(orderNumber))
                return new { found = false, message = "Cần cung cấp mã đơn hàng" };

            var orders = await _unitOfWork.Orders.FindAsync(o =>
                o.OrderNumber == orderNumber && !o.IsDeleted, o => o.Customer);

            var order = orders.FirstOrDefault();
            if (order == null)
                return new { found = false, message = "Không tìm thấy đơn hàng" };

            return new
            {
                found = true,
                order_number = order.OrderNumber,
                customer = order.Customer?.Name,
                status = order.Status.ToString(),
                total_amount = order.TotalAmount,
                created_at = order.CreateAt.ToString("dd/MM/yyyy HH:mm"),
                note = order.Note
            };
        }

        private async Task<object> ExecuteGetSalesSummary(Dictionary<string, object?> args)
        {
            var period = args.GetValueOrDefault("period")?.ToString() ?? "today";
            var today = DateTime.UtcNow.Date;
            DateTime startDate = period switch
            {
                "today" => today,
                "week" => today.AddDays(-7),
                "month" => today.AddMonths(-1),
                "quarter" => today.AddMonths(-3),
                "year" => today.AddYears(-1),
                _ => today
            };

            var orders = await _unitOfWork.Orders.FindAsync(o =>
                o.CreateAt >= startDate && o.Status != OrderStatus.Cancelled);

            var ordersList = orders.ToList();
            return new
            {
                period = period,
                start_date = startDate.ToString("dd/MM/yyyy"),
                end_date = today.ToString("dd/MM/yyyy"),
                total_revenue = ordersList.Sum(o => o.TotalAmount),
                order_count = ordersList.Count,
                average_order_value = ordersList.Any() ? ordersList.Average(o => o.TotalAmount) : 0
            };
        }

        private async Task<object> ExecuteGetLowStockProducts(Dictionary<string, object?> args)
        {
            var limit = args.TryGetValue("limit", out var limitObj) && limitObj != null
                ? Convert.ToInt32(limitObj) : 10;

            var products = await _unitOfWork.Products.FindAsync(p =>
                p.StockQuantity <= p.MinStockLevel && p.IsActive && !p.IsDeleted);

            return products
                .OrderBy(p => p.StockQuantity)
                .Take(limit)
                .Select(p => new
                {
                    name = p.Name,
                    sku = p.SKU,
                    current_stock = p.StockQuantity,
                    min_stock = p.MinStockLevel,
                    shortage = p.MinStockLevel - p.StockQuantity
                }).ToList();
        }

        #endregion

        #region Helper Methods

        private string BuildSystemPromptWithFunctions()
        {
            var functionsJson = JsonSerializer.Serialize(_functions, new JsonSerializerOptions { WriteIndented = true });
            var prompt = "Bạn là trợ lý AI thông minh của hệ thống NovaSaaS.\n" +
                "Bạn có thể thực hiện các hành động thông qua các function sau:\n\n" +
                functionsJson + "\n\n" +
                "Khi cần gọi function, sử dụng format sau:\n" +
                "<function_call>\n" +
                "{\"name\": \"function_name\", \"arguments\": {\"arg1\": \"value1\"}}\n" +
                "</function_call>\n\n" +
                "Sau khi nhận được kết quả function, hãy diễn giải và trả lời người dùng một cách tự nhiên.\n" +
                "Nếu không cần gọi function, hãy trả lời trực tiếp.";
            return prompt;
        }

        private static string? ExtractFunctionCall(string content)
        {
            var start = content.IndexOf("<function_call>") + "<function_call>".Length;
            var end = content.IndexOf("</function_call>");
            if (start < 0 || end < 0 || end <= start) return null;
            return content.Substring(start, end - start).Trim();
        }

        private static (string name, string argsJson) ParseFunctionCall(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var name = doc.RootElement.GetProperty("name").GetString() ?? "";
                var args = doc.RootElement.GetProperty("arguments").GetRawText();
                return (name, args);
            }
            catch
            {
                return ("", "{}");
            }
        }

        private static string CleanResponse(string content)
        {
            // Remove any function call tags from the response
            var start = content.IndexOf("<function_call>");
            if (start >= 0)
            {
                var end = content.IndexOf("</function_call>");
                if (end > start)
                    content = content.Remove(start, end - start + "</function_call>".Length);
            }
            return content.Trim();
        }

        #endregion
    }
}
