using NovaSaaS.Application.DTOs.Business;
using NovaSaaS.Domain.Entities.Business;
using NovaSaaS.Domain.Enums;
using NovaSaaS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NovaSaaS.Application.Services.Business
{
    public class QuotationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public QuotationService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Guid> CreateAsync(CreateQuotationDto dto)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(dto.CustomerId);
            if (customer == null) throw new ArgumentException("Khách hàng không tồn tại.");

            if (dto.Items == null || !dto.Items.Any())
                throw new InvalidOperationException("Báo giá phải có ít nhất một sản phẩm.");

            var quotation = new Quotation
            {
                QuotationNumber = await GenerateQuotationNumberAsync(),
                CustomerId = dto.CustomerId,
                OpportunityId = dto.OpportunityId,
                Status = QuotationStatus.Draft,
                ValidUntil = dto.ValidUntil,
                Notes = dto.Notes,
                CreateAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId
            };

            decimal subTotal = 0;
            decimal discountTotal = 0;

            foreach (var item in dto.Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product == null)
                    throw new ArgumentException($"Sản phẩm {item.ProductId} không tồn tại.");

                var unitPrice = item.UnitPrice > 0 ? item.UnitPrice : product.Price;
                var itemTotal = item.Quantity * unitPrice;
                var itemDiscount = itemTotal * item.DiscountPercent / 100;

                subTotal += itemTotal;
                discountTotal += itemDiscount;

                var quotationItem = new QuotationItem
                {
                    QuotationId = quotation.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    DiscountPercent = item.DiscountPercent,
                    Notes = item.Notes,
                    CreateAt = DateTime.UtcNow,
                    CreatedBy = _currentUser.UserId
                };

                _unitOfWork.Repository<QuotationItem>().Add(quotationItem);
            }

            quotation.SubTotal = subTotal;
            quotation.DiscountAmount = discountTotal;
            quotation.TaxAmount = (subTotal - discountTotal) * 0.1m; // 10% VAT
            quotation.TotalAmount = subTotal - discountTotal + quotation.TaxAmount;

            _unitOfWork.Repository<Quotation>().Add(quotation);
            await _unitOfWork.CompleteAsync();
            return quotation.Id;
        }

        public async Task UpdateStatusAsync(Guid id, UpdateQuotationStatusDto dto)
        {
            var quotation = await _unitOfWork.Repository<Quotation>().GetByIdAsync(id);
            if (quotation == null) throw new ArgumentException("Báo giá không tồn tại.");

            quotation.Status = dto.NewStatus;
            if (!string.IsNullOrEmpty(dto.Notes))
                quotation.Notes = dto.Notes;
            quotation.UpdateAt = DateTime.UtcNow;
            quotation.UpdatedBy = _currentUser.UserId;

            _unitOfWork.Repository<Quotation>().Update(quotation);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var quotation = await _unitOfWork.Repository<Quotation>().GetByIdAsync(id);
            if (quotation == null) return false;

            if (quotation.Status != QuotationStatus.Draft)
                throw new InvalidOperationException("Chỉ có thể xóa báo giá ở trạng thái Nháp.");

            var items = await _unitOfWork.Repository<QuotationItem>()
                .FindAsync(i => i.QuotationId == id);
            _unitOfWork.Repository<QuotationItem>().RemoveRange(items);

            _unitOfWork.Repository<Quotation>().Remove(quotation);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<PaginatedResult<QuotationDto>> GetAllAsync(QuotationFilterDto? filter = null)
        {
            filter ??= new QuotationFilterDto();

            var allQuotations = await _unitOfWork.Repository<Quotation>().GetAllAsync();
            var query = allQuotations.AsQueryable();

            if (filter.Status.HasValue)
                query = query.Where(q => q.Status == filter.Status.Value);
            if (filter.CustomerId.HasValue)
                query = query.Where(q => q.CustomerId == filter.CustomerId.Value);
            if (filter.FromDate.HasValue)
                query = query.Where(q => q.CreateAt >= filter.FromDate.Value);
            if (filter.ToDate.HasValue)
                query = query.Where(q => q.CreateAt <= filter.ToDate.Value);
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var term = filter.SearchTerm.ToLower();
                query = query.Where(q => q.QuotationNumber.ToLower().Contains(term));
            }

            var totalCount = query.Count();
            var customers = await _unitOfWork.Customers.GetAllAsync();
            var customerDict = customers.ToDictionary(c => c.Id, c => c.Name);

            var items = query
                .OrderByDescending(q => q.CreateAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(q => new QuotationDto
                {
                    Id = q.Id,
                    QuotationNumber = q.QuotationNumber,
                    CustomerId = q.CustomerId,
                    CustomerName = customerDict.ContainsKey(q.CustomerId) ? customerDict[q.CustomerId] : "",
                    OpportunityId = q.OpportunityId,
                    Status = q.Status,
                    SubTotal = q.SubTotal,
                    TaxAmount = q.TaxAmount,
                    DiscountAmount = q.DiscountAmount,
                    TotalAmount = q.TotalAmount,
                    ValidUntil = q.ValidUntil,
                    Notes = q.Notes,
                    ConvertedOrderId = q.ConvertedOrderId,
                    CreateAt = q.CreateAt
                }).ToList();

            return new PaginatedResult<QuotationDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<QuotationDto?> GetByIdAsync(Guid id)
        {
            var q = await _unitOfWork.Repository<Quotation>().GetByIdAsync(id);
            if (q == null) return null;

            var customer = await _unitOfWork.Customers.GetByIdAsync(q.CustomerId);
            var qItems = await _unitOfWork.Repository<QuotationItem>()
                .FindAsync(i => i.QuotationId == id);
            var products = await _unitOfWork.Products.GetAllAsync();
            var productDict = products.ToDictionary(p => p.Id);

            return new QuotationDto
            {
                Id = q.Id,
                QuotationNumber = q.QuotationNumber,
                CustomerId = q.CustomerId,
                CustomerName = customer?.Name ?? "",
                OpportunityId = q.OpportunityId,
                Status = q.Status,
                SubTotal = q.SubTotal,
                TaxAmount = q.TaxAmount,
                DiscountAmount = q.DiscountAmount,
                TotalAmount = q.TotalAmount,
                ValidUntil = q.ValidUntil,
                Notes = q.Notes,
                ConvertedOrderId = q.ConvertedOrderId,
                CreateAt = q.CreateAt,
                Items = qItems.Select(i => new QuotationItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = productDict.ContainsKey(i.ProductId) ? productDict[i.ProductId].Name : "",
                    ProductSKU = productDict.ContainsKey(i.ProductId) ? productDict[i.ProductId].SKU : "",
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    DiscountPercent = i.DiscountPercent,
                    Notes = i.Notes
                }).ToList()
            };
        }

        private async Task<string> GenerateQuotationNumberAsync()
        {
            var count = await _unitOfWork.Repository<Quotation>().CountAsync();
            return $"QT-{DateTime.UtcNow:yyyyMM}-{(count + 1):D4}";
        }
    }
}
