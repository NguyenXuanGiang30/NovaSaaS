using NovaSaaS.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Application.DTOs.Business
{
    public class CreateCustomerDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(20)]
        public string TaxCode { get; set; } = string.Empty;
        
        public string? Address { get; set; }
        public string? Email { get; set; }
        
        public CustomerType Type { get; set; } = CustomerType.Retail;
    }

    public class CustomerDto : CreateCustomerDto
    {
        public Guid Id { get; set; }
        public CustomerRank Rank { get; set; }
        public decimal TotalSpending { get; set; }
        public DateTime CreateAt { get; set; }
    }

    public class UpdateCustomerDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(20)]
        public string TaxCode { get; set; } = string.Empty;
        
        public string? Address { get; set; }
        public string? Email { get; set; }
        
        public CustomerType Type { get; set; } = CustomerType.Retail;
    }

    public class CustomerFilterDto
    {
        public CustomerType? Type { get; set; }
        public CustomerRank? Rank { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
