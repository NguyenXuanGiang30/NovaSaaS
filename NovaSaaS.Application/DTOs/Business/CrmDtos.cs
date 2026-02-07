using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Application.DTOs.Business
{
    // ==================== CustomerGroup DTOs ====================

    public class CreateCustomerGroupDto
    {
        [Required(ErrorMessage = "Tên nhóm là bắt buộc")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Range(0, 100)]
        public decimal DiscountPercent { get; set; } = 0;
    }

    public class UpdateCustomerGroupDto
    {
        [Required(ErrorMessage = "Tên nhóm là bắt buộc")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Range(0, 100)]
        public decimal DiscountPercent { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }

    public class CustomerGroupDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal DiscountPercent { get; set; }
        public bool IsActive { get; set; }
        public int CustomerCount { get; set; }
        public DateTime CreateAt { get; set; }
    }

    // ==================== Contact DTOs ====================

    public class CreateContactDto
    {
        [Required]
        public Guid CustomerId { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? JobTitle { get; set; }

        [MaxLength(200)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(20)]
        [Phone]
        public string? Phone { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsPrimary { get; set; } = false;
    }

    public class UpdateContactDto
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? JobTitle { get; set; }

        [MaxLength(200)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(20)]
        [Phone]
        public string? Phone { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsPrimary { get; set; } = false;
    }

    public class ContactDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? JobTitle { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Notes { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime CreateAt { get; set; }
    }

    // ==================== Lead DTOs ====================

    public class CreateLeadDto
    {
        [Required(ErrorMessage = "Tên liên hệ là bắt buộc")]
        [MaxLength(100)]
        public string ContactName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? CompanyName { get; set; }

        [MaxLength(200)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(20)]
        [Phone]
        public string? Phone { get; set; }

        public LeadSource Source { get; set; } = LeadSource.Other;

        [Range(0, double.MaxValue)]
        public decimal EstimatedValue { get; set; } = 0;

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public Guid? AssignedToUserId { get; set; }
    }

    public class UpdateLeadDto
    {
        [Required(ErrorMessage = "Tên liên hệ là bắt buộc")]
        [MaxLength(100)]
        public string ContactName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? CompanyName { get; set; }

        [MaxLength(200)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(20)]
        [Phone]
        public string? Phone { get; set; }

        public LeadSource Source { get; set; }
        public LeadStatus Status { get; set; }

        [Range(0, double.MaxValue)]
        public decimal EstimatedValue { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public Guid? AssignedToUserId { get; set; }
    }

    public class LeadDto
    {
        public Guid Id { get; set; }
        public string ContactName { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public LeadSource Source { get; set; }
        public string SourceName => Source.ToString();
        public LeadStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public decimal EstimatedValue { get; set; }
        public string? Notes { get; set; }
        public Guid? AssignedToUserId { get; set; }
        public Guid? ConvertedCustomerId { get; set; }
        public DateTime? ConvertedAt { get; set; }
        public DateTime CreateAt { get; set; }
        public int ActivityCount { get; set; }
    }

    public class LeadFilterDto
    {
        public LeadStatus? Status { get; set; }
        public LeadSource? Source { get; set; }
        public Guid? AssignedToUserId { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class ConvertLeadDto
    {
        public CustomerType CustomerType { get; set; } = CustomerType.Retail;
        public string? Address { get; set; }
    }

    // ==================== Opportunity DTOs ====================

    public class CreateOpportunityDto
    {
        [Required(ErrorMessage = "Tên cơ hội là bắt buộc")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public Guid CustomerId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Value { get; set; } = 0;

        [Range(0, 100)]
        public int Probability { get; set; } = 0;

        public DateTime? ExpectedCloseDate { get; set; }

        public Guid? AssignedToUserId { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public class UpdateOpportunityDto
    {
        [Required(ErrorMessage = "Tên cơ hội là bắt buộc")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public OpportunityStage Stage { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Value { get; set; }

        [Range(0, 100)]
        public int Probability { get; set; }

        public DateTime? ExpectedCloseDate { get; set; }

        public Guid? AssignedToUserId { get; set; }

        [MaxLength(500)]
        public string? LostReason { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public class OpportunityDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public OpportunityStage Stage { get; set; }
        public string StageName => Stage.ToString();
        public decimal Value { get; set; }
        public int Probability { get; set; }
        public decimal WeightedValue => Value * Probability / 100;
        public DateTime? ExpectedCloseDate { get; set; }
        public DateTime? ActualCloseDate { get; set; }
        public Guid? AssignedToUserId { get; set; }
        public string? LostReason { get; set; }
        public string? Notes { get; set; }
        public Guid? ConvertedOrderId { get; set; }
        public DateTime CreateAt { get; set; }
        public int ActivityCount { get; set; }
        public int QuotationCount { get; set; }
    }

    public class OpportunityFilterDto
    {
        public OpportunityStage? Stage { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? AssignedToUserId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // ==================== Quotation DTOs ====================

    public class CreateQuotationDto
    {
        [Required]
        public Guid CustomerId { get; set; }

        public Guid? OpportunityId { get; set; }

        public DateTime? ValidUntil { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public List<CreateQuotationItemDto> Items { get; set; } = new();
    }

    public class CreateQuotationItemDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Range(0, 100)]
        public decimal DiscountPercent { get; set; } = 0;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class QuotationDto
    {
        public Guid Id { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public Guid? OpportunityId { get; set; }
        public QuotationStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? ValidUntil { get; set; }
        public string? Notes { get; set; }
        public Guid? ConvertedOrderId { get; set; }
        public DateTime CreateAt { get; set; }
        public List<QuotationItemDto> Items { get; set; } = new();
    }

    public class QuotationItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice * (1 - DiscountPercent / 100);
        public string? Notes { get; set; }
    }

    public class QuotationFilterDto
    {
        public QuotationStatus? Status { get; set; }
        public Guid? CustomerId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class UpdateQuotationStatusDto
    {
        public QuotationStatus NewStatus { get; set; }
        public string? Notes { get; set; }
    }

    // ==================== LoyaltyProgram DTOs ====================

    public class CreateLoyaltyProgramDto
    {
        [Required(ErrorMessage = "Tên chương trình là bắt buộc")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Range(1, double.MaxValue)]
        public decimal PointsPerAmount { get; set; } = 10000;

        [Range(0, double.MaxValue)]
        public decimal PointValue { get; set; } = 100;

        [Range(0, int.MaxValue)]
        public int MinRedeemPoints { get; set; } = 100;

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class UpdateLoyaltyProgramDto
    {
        [Required(ErrorMessage = "Tên chương trình là bắt buộc")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Range(1, double.MaxValue)]
        public decimal PointsPerAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PointValue { get; set; }

        [Range(0, int.MaxValue)]
        public int MinRedeemPoints { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class LoyaltyProgramDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal PointsPerAmount { get; set; }
        public decimal PointValue { get; set; }
        public int MinRedeemPoints { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public int TotalTransactions { get; set; }
        public DateTime CreateAt { get; set; }
    }

    public class LoyaltyTransactionDto
    {
        public Guid Id { get; set; }
        public Guid LoyaltyProgramId { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public LoyaltyTransactionType Type { get; set; }
        public string TypeName => Type.ToString();
        public int Points { get; set; }
        public int BalanceAfter { get; set; }
        public string? ReferenceCode { get; set; }
        public string? Notes { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime CreateAt { get; set; }
    }

    // ==================== Campaign DTOs ====================

    public class CreateCampaignDto
    {
        [Required(ErrorMessage = "Tên chiến dịch là bắt buộc")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        public CampaignType Type { get; set; } = CampaignType.Promotion;

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Budget { get; set; } = 0;

        public Guid? CouponId { get; set; }
        public Guid? TargetCustomerGroupId { get; set; }

        [MaxLength(1000)]
        public string? InternalNotes { get; set; }
    }

    public class UpdateCampaignDto
    {
        [Required(ErrorMessage = "Tên chiến dịch là bắt buộc")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        public CampaignType Type { get; set; }
        public CampaignStatus Status { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Budget { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ActualCost { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Revenue { get; set; }

        public Guid? CouponId { get; set; }
        public Guid? TargetCustomerGroupId { get; set; }

        [MaxLength(1000)]
        public string? InternalNotes { get; set; }
    }

    public class CampaignDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public CampaignType Type { get; set; }
        public string TypeName => Type.ToString();
        public CampaignStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Budget { get; set; }
        public decimal ActualCost { get; set; }
        public decimal Revenue { get; set; }
        public decimal ROI => ActualCost > 0 ? (Revenue - ActualCost) / ActualCost * 100 : 0;
        public int LeadsGenerated { get; set; }
        public int OrdersGenerated { get; set; }
        public Guid? CouponId { get; set; }
        public Guid? TargetCustomerGroupId { get; set; }
        public string? TargetCustomerGroupName { get; set; }
        public string? InternalNotes { get; set; }
        public DateTime CreateAt { get; set; }
    }

    public class CampaignFilterDto
    {
        public CampaignStatus? Status { get; set; }
        public CampaignType? Type { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // ==================== Activity DTOs ====================

    public class CreateActivityDto
    {
        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public ActivityType Type { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? ReminderAt { get; set; }

        public Guid? AssignedToUserId { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? LeadId { get; set; }
        public Guid? OpportunityId { get; set; }
    }

    public class UpdateActivityDto
    {
        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public ActivityType Type { get; set; }
        public ActivityStatus Status { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? ReminderAt { get; set; }

        public Guid? AssignedToUserId { get; set; }

        [MaxLength(2000)]
        public string? Outcome { get; set; }
    }

    public class ActivityDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public ActivityType Type { get; set; }
        public string TypeName => Type.ToString();
        public ActivityStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public string? Description { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? ReminderAt { get; set; }
        public Guid? AssignedToUserId { get; set; }
        public Guid? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public Guid? LeadId { get; set; }
        public string? LeadContactName { get; set; }
        public Guid? OpportunityId { get; set; }
        public string? OpportunityName { get; set; }
        public string? Outcome { get; set; }
        public DateTime CreateAt { get; set; }
    }

    public class ActivityFilterDto
    {
        public ActivityType? Type { get; set; }
        public ActivityStatus? Status { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? LeadId { get; set; }
        public Guid? OpportunityId { get; set; }
        public Guid? AssignedToUserId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
