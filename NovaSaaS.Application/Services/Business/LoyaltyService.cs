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
    public class LoyaltyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public LoyaltyService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        #region LoyaltyProgram CRUD

        public async Task<Guid> CreateProgramAsync(CreateLoyaltyProgramDto dto)
        {
            var program = new LoyaltyProgram
            {
                Name = dto.Name,
                Description = dto.Description,
                PointsPerAmount = dto.PointsPerAmount,
                PointValue = dto.PointValue,
                MinRedeemPoints = dto.MinRedeemPoints,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = true,
                CreateAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId
            };

            _unitOfWork.Repository<LoyaltyProgram>().Add(program);
            await _unitOfWork.CompleteAsync();
            return program.Id;
        }

        public async Task UpdateProgramAsync(Guid id, UpdateLoyaltyProgramDto dto)
        {
            var program = await _unitOfWork.Repository<LoyaltyProgram>().GetByIdAsync(id);
            if (program == null) throw new ArgumentException("Chương trình loyalty không tồn tại.");

            program.Name = dto.Name;
            program.Description = dto.Description;
            program.PointsPerAmount = dto.PointsPerAmount;
            program.PointValue = dto.PointValue;
            program.MinRedeemPoints = dto.MinRedeemPoints;
            program.StartDate = dto.StartDate;
            program.EndDate = dto.EndDate;
            program.IsActive = dto.IsActive;
            program.UpdateAt = DateTime.UtcNow;
            program.UpdatedBy = _currentUser.UserId;

            _unitOfWork.Repository<LoyaltyProgram>().Update(program);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<List<LoyaltyProgramDto>> GetAllProgramsAsync()
        {
            var programs = await _unitOfWork.Repository<LoyaltyProgram>().GetAllAsync();
            var transactions = await _unitOfWork.Repository<LoyaltyTransaction>().GetAllAsync();

            return programs.Select(p => new LoyaltyProgramDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                PointsPerAmount = p.PointsPerAmount,
                PointValue = p.PointValue,
                MinRedeemPoints = p.MinRedeemPoints,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                IsActive = p.IsActive,
                TotalTransactions = transactions.Count(t => t.LoyaltyProgramId == p.Id),
                CreateAt = p.CreateAt
            }).OrderByDescending(p => p.CreateAt).ToList();
        }

        public async Task<LoyaltyProgramDto?> GetProgramByIdAsync(Guid id)
        {
            var p = await _unitOfWork.Repository<LoyaltyProgram>().GetByIdAsync(id);
            if (p == null) return null;

            var txCount = await _unitOfWork.Repository<LoyaltyTransaction>()
                .CountAsync(t => t.LoyaltyProgramId == id);

            return new LoyaltyProgramDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                PointsPerAmount = p.PointsPerAmount,
                PointValue = p.PointValue,
                MinRedeemPoints = p.MinRedeemPoints,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                IsActive = p.IsActive,
                TotalTransactions = txCount,
                CreateAt = p.CreateAt
            };
        }

        #endregion

        #region Loyalty Transactions

        /// <summary>
        /// Tích điểm cho khách hàng khi mua hàng.
        /// </summary>
        public async Task EarnPointsAsync(Guid programId, Guid customerId, decimal orderAmount, string? referenceCode = null, Guid? referenceId = null)
        {
            var program = await _unitOfWork.Repository<LoyaltyProgram>().GetByIdAsync(programId);
            if (program == null || !program.IsActive) return;

            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null) return;

            var points = (int)(orderAmount / program.PointsPerAmount);
            if (points <= 0) return;

            customer.LoyaltyPoints += points;
            _unitOfWork.Customers.Update(customer);

            var transaction = new LoyaltyTransaction
            {
                LoyaltyProgramId = programId,
                CustomerId = customerId,
                Type = LoyaltyTransactionType.Earn,
                Points = points,
                BalanceAfter = customer.LoyaltyPoints,
                ReferenceCode = referenceCode,
                ReferenceId = referenceId,
                Notes = $"Tích {points} điểm từ đơn hàng {referenceCode}",
                CreateAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId
            };

            _unitOfWork.Repository<LoyaltyTransaction>().Add(transaction);
            await _unitOfWork.CompleteAsync();
        }

        /// <summary>
        /// Đổi điểm cho khách hàng.
        /// </summary>
        public async Task<decimal> RedeemPointsAsync(Guid programId, Guid customerId, int points)
        {
            var program = await _unitOfWork.Repository<LoyaltyProgram>().GetByIdAsync(programId);
            if (program == null || !program.IsActive)
                throw new InvalidOperationException("Chương trình loyalty không hợp lệ.");

            if (points < program.MinRedeemPoints)
                throw new InvalidOperationException($"Số điểm tối thiểu để đổi là {program.MinRedeemPoints}.");

            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null) throw new ArgumentException("Khách hàng không tồn tại.");

            if (customer.LoyaltyPoints < points)
                throw new InvalidOperationException("Không đủ điểm để đổi.");

            var discountValue = points * program.PointValue;
            customer.LoyaltyPoints -= points;
            _unitOfWork.Customers.Update(customer);

            var transaction = new LoyaltyTransaction
            {
                LoyaltyProgramId = programId,
                CustomerId = customerId,
                Type = LoyaltyTransactionType.Redeem,
                Points = -points,
                BalanceAfter = customer.LoyaltyPoints,
                Notes = $"Đổi {points} điểm = {discountValue:N0} VND",
                CreateAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId
            };

            _unitOfWork.Repository<LoyaltyTransaction>().Add(transaction);
            await _unitOfWork.CompleteAsync();

            return discountValue;
        }

        public async Task<List<LoyaltyTransactionDto>> GetTransactionsByCustomerAsync(Guid customerId)
        {
            var transactions = await _unitOfWork.Repository<LoyaltyTransaction>()
                .FindAsync(t => t.CustomerId == customerId);
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);

            return transactions
                .OrderByDescending(t => t.CreateAt)
                .Select(t => new LoyaltyTransactionDto
                {
                    Id = t.Id,
                    LoyaltyProgramId = t.LoyaltyProgramId,
                    CustomerId = t.CustomerId,
                    CustomerName = customer?.Name ?? "",
                    Type = t.Type,
                    Points = t.Points,
                    BalanceAfter = t.BalanceAfter,
                    ReferenceCode = t.ReferenceCode,
                    Notes = t.Notes,
                    ExpiresAt = t.ExpiresAt,
                    CreateAt = t.CreateAt
                }).ToList();
        }

        #endregion
    }
}
