using NovaSaaS.Application.DTOs.Inventory;
using NovaSaaS.Domain.Entities.Inventory;
using NovaSaaS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NovaSaaS.Application.Services.Inventory
{
    public class ProductVariantService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public ProductVariantService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Guid> CreateAsync(CreateProductVariantDto dto)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
            if (product == null) throw new ArgumentException("Sản phẩm không tồn tại.");

            var sku = dto.SKU ?? await GenerateSKUAsync(product.SKU);

            var existingSku = await _unitOfWork.Repository<ProductVariant>()
                .FirstOrDefaultAsync(v => v.SKU == sku);
            if (existingSku != null)
                throw new InvalidOperationException($"SKU '{sku}' đã tồn tại.");

            var variant = new ProductVariant
            {
                ProductId = dto.ProductId,
                SKU = sku,
                Name = dto.Name,
                AttributeName1 = dto.AttributeName1,
                AttributeValue1 = dto.AttributeValue1,
                AttributeName2 = dto.AttributeName2,
                AttributeValue2 = dto.AttributeValue2,
                Price = dto.Price,
                CostPrice = dto.CostPrice,
                Barcode = dto.Barcode,
                IsActive = true,
                CreateAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId
            };

            _unitOfWork.Repository<ProductVariant>().Add(variant);
            await _unitOfWork.CompleteAsync();
            return variant.Id;
        }

        public async Task UpdateAsync(Guid id, UpdateProductVariantDto dto)
        {
            var variant = await _unitOfWork.Repository<ProductVariant>().GetByIdAsync(id);
            if (variant == null) throw new ArgumentException("Biến thể không tồn tại.");

            variant.Name = dto.Name;
            variant.AttributeName1 = dto.AttributeName1;
            variant.AttributeValue1 = dto.AttributeValue1;
            variant.AttributeName2 = dto.AttributeName2;
            variant.AttributeValue2 = dto.AttributeValue2;
            variant.Price = dto.Price;
            variant.CostPrice = dto.CostPrice;
            variant.Barcode = dto.Barcode;
            variant.IsActive = dto.IsActive;
            variant.UpdateAt = DateTime.UtcNow;
            variant.UpdatedBy = _currentUser.UserId;

            _unitOfWork.Repository<ProductVariant>().Update(variant);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var variant = await _unitOfWork.Repository<ProductVariant>().GetByIdAsync(id);
            if (variant == null) return false;

            _unitOfWork.Repository<ProductVariant>().Remove(variant);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<List<ProductVariantDto>> GetByProductIdAsync(Guid productId)
        {
            var variants = await _unitOfWork.Repository<ProductVariant>()
                .FindAsync(v => v.ProductId == productId);
            var product = await _unitOfWork.Products.GetByIdAsync(productId);

            return variants.Select(v => new ProductVariantDto
            {
                Id = v.Id,
                ProductId = v.ProductId,
                ProductName = product?.Name ?? "",
                SKU = v.SKU,
                Name = v.Name,
                AttributeName1 = v.AttributeName1,
                AttributeValue1 = v.AttributeValue1,
                AttributeName2 = v.AttributeName2,
                AttributeValue2 = v.AttributeValue2,
                Price = v.Price,
                CostPrice = v.CostPrice,
                Barcode = v.Barcode,
                ImageUrl = v.ImageUrl,
                IsActive = v.IsActive,
                StockQuantity = v.StockQuantity,
                CreateAt = v.CreateAt
            }).OrderBy(v => v.Name).ToList();
        }

        public async Task<ProductVariantDto?> GetByIdAsync(Guid id)
        {
            var v = await _unitOfWork.Repository<ProductVariant>().GetByIdAsync(id);
            if (v == null) return null;

            var product = await _unitOfWork.Products.GetByIdAsync(v.ProductId);

            return new ProductVariantDto
            {
                Id = v.Id,
                ProductId = v.ProductId,
                ProductName = product?.Name ?? "",
                SKU = v.SKU,
                Name = v.Name,
                AttributeName1 = v.AttributeName1,
                AttributeValue1 = v.AttributeValue1,
                AttributeName2 = v.AttributeName2,
                AttributeValue2 = v.AttributeValue2,
                Price = v.Price,
                CostPrice = v.CostPrice,
                Barcode = v.Barcode,
                ImageUrl = v.ImageUrl,
                IsActive = v.IsActive,
                StockQuantity = v.StockQuantity,
                CreateAt = v.CreateAt
            };
        }

        private async Task<string> GenerateSKUAsync(string parentSku)
        {
            var count = await _unitOfWork.Repository<ProductVariant>().CountAsync();
            return $"{parentSku}-V{(count + 1):D3}";
        }
    }
}
