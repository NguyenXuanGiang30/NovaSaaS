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
    /// <summary>
    /// InvoiceService - Quản lý hóa đơn và trạng thái thanh toán.
    /// </summary>
    public class InvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public InvoiceService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        #region Query Operations

        public async Task<PaginatedResult<InvoiceDto>> GetAllAsync(InvoiceFilterDto? filter = null)
        {
            filter ??= new InvoiceFilterDto();

            var allInvoices = await _unitOfWork.Invoices.FindAsync(
                _ => true,
                i => i.Order);

            var query = allInvoices.AsQueryable();

            // Apply filters
            if (filter.Status.HasValue)
                query = query.Where(i => i.Status == filter.Status.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(i => i.CreateAt >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(i => i.CreateAt <= filter.ToDate.Value);

            var totalCount = query.Count();

            var items = query
                .OrderByDescending(i => i.CreateAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(i => MapToDto(i))
                .ToList();

            return new PaginatedResult<InvoiceDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<InvoiceDto?> GetByIdAsync(Guid id)
        {
            var invoices = await _unitOfWork.Invoices.FindAsync(
                i => i.Id == id,
                i => i.Order);

            var invoice = invoices.FirstOrDefault();
            if (invoice == null) return null;

            return MapToDto(invoice);
        }

        public async Task<InvoiceDto?> GetByOrderIdAsync(Guid orderId)
        {
            var invoices = await _unitOfWork.Invoices.FindAsync(
                i => i.OrderId == orderId,
                i => i.Order);

            var invoice = invoices.FirstOrDefault();
            if (invoice == null) return null;

            return MapToDto(invoice);
        }

        #endregion

        #region Status Management

        /// <summary>
        /// Đánh dấu hóa đơn đã thanh toán.
        /// </summary>
        public async Task<InvoiceDto> MarkAsPaidAsync(Guid id, MarkAsPaidRequest request)
        {
            var invoice = await _unitOfWork.Invoices.GetByIdAsync(id);
            if (invoice == null)
                throw new ArgumentException("Hóa đơn không tồn tại.");

            // Validate current status
            if (invoice.Status != InvoiceStatus.Unpaid && invoice.Status != InvoiceStatus.Draft)
                throw new InvalidOperationException(
                    $"Không thể đánh dấu thanh toán cho hóa đơn ở trạng thái '{invoice.Status}'.");

            invoice.Status = InvoiceStatus.Paid;
            invoice.PaymentMethod = request.PaymentMethod;
            invoice.PaidDate = DateTime.UtcNow;
            invoice.UpdateAt = DateTime.UtcNow;
            invoice.UpdatedBy = _currentUser.UserId;

            _unitOfWork.Invoices.Update(invoice);
            await _unitOfWork.CompleteAsync();

            return (await GetByIdAsync(id))!;
        }

        /// <summary>
        /// Hoàn tiền hóa đơn.
        /// Note: Việc hoàn kho được xử lý bởi OrderService.CancelOrderAsync
        /// </summary>
        public async Task<InvoiceDto> RefundAsync(Guid id, RefundRequest request)
        {
            var invoice = await _unitOfWork.Invoices.GetByIdAsync(id);
            if (invoice == null)
                throw new ArgumentException("Hóa đơn không tồn tại.");

            // Only paid invoices can be refunded
            if (invoice.Status != InvoiceStatus.Paid)
                throw new InvalidOperationException(
                    $"Chỉ có thể hoàn tiền cho hóa đơn đã thanh toán. Trạng thái hiện tại: '{invoice.Status}'.");

            invoice.Status = InvoiceStatus.Refunded;
            invoice.UpdateAt = DateTime.UtcNow;
            invoice.UpdatedBy = _currentUser.UserId;

            _unitOfWork.Invoices.Update(invoice);
            await _unitOfWork.CompleteAsync();

            // Note: Stock restoration should be handled separately via OrderService.CancelOrderAsync
            // if request.RestoreStock is true, the caller should also call OrderService

            return (await GetByIdAsync(id))!;
        }

        /// <summary>
        /// Hủy hóa đơn (ví dụ: do nhập sai thông tin).
        /// </summary>
        public async Task<InvoiceDto> CancelAsync(Guid id, string reason)
        {
            var invoice = await _unitOfWork.Invoices.GetByIdAsync(id);
            if (invoice == null)
                throw new ArgumentException("Hóa đơn không tồn tại.");

            // Cannot cancel paid or refunded invoices
            if (invoice.Status == InvoiceStatus.Paid || invoice.Status == InvoiceStatus.Refunded)
                throw new InvalidOperationException(
                    $"Không thể hủy hóa đơn đã thanh toán hoặc đã hoàn tiền.");

            invoice.Status = InvoiceStatus.Cancelled;
            invoice.UpdateAt = DateTime.UtcNow;
            invoice.UpdatedBy = _currentUser.UserId;

            _unitOfWork.Invoices.Update(invoice);
            await _unitOfWork.CompleteAsync();

            return (await GetByIdAsync(id))!;
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Lấy thống kê doanh thu theo trạng thái hóa đơn.
        /// </summary>
        public async Task<InvoiceStatisticsDto> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var allInvoices = await _unitOfWork.Invoices.GetAllAsync();
            var query = allInvoices.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(i => i.CreateAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(i => i.CreateAt <= toDate.Value);

            var invoices = query.ToList();

            return new InvoiceStatisticsDto
            {
                TotalCount = invoices.Count,
                TotalAmount = invoices.Sum(i => i.TotalAmount),
                PaidCount = invoices.Count(i => i.Status == InvoiceStatus.Paid),
                PaidAmount = invoices.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.TotalAmount),
                UnpaidCount = invoices.Count(i => i.Status == InvoiceStatus.Unpaid),
                UnpaidAmount = invoices.Where(i => i.Status == InvoiceStatus.Unpaid).Sum(i => i.TotalAmount),
                OverdueCount = invoices.Count(i => i.Status == InvoiceStatus.Unpaid && i.DueDate < DateTime.UtcNow),
                OverdueAmount = invoices
                    .Where(i => i.Status == InvoiceStatus.Unpaid && i.DueDate < DateTime.UtcNow)
                    .Sum(i => i.TotalAmount)
            };
        }

        #endregion

        #region Private Helpers

        private static InvoiceDto MapToDto(Invoice i)
        {
            return new InvoiceDto
            {
                Id = i.Id,
                OrderId = i.OrderId,
                InvoiceNumber = i.InvoiceNumber,
                TotalAmount = i.TotalAmount,
                Status = i.Status,
                DueDate = i.DueDate,
                PaidDate = i.PaidDate,
                PaymentMethod = i.PaymentMethod,
                CreateAt = i.CreateAt
            };
        }

        #endregion
    }

    /// <summary>
    /// DTO cho thống kê hóa đơn.
    /// </summary>
    public class InvoiceStatisticsDto
    {
        public int TotalCount { get; set; }
        public decimal TotalAmount { get; set; }
        public int PaidCount { get; set; }
        public decimal PaidAmount { get; set; }
        public int UnpaidCount { get; set; }
        public decimal UnpaidAmount { get; set; }
        public int OverdueCount { get; set; }
        public decimal OverdueAmount { get; set; }
    }
}
