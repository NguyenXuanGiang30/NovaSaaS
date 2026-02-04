using NovaSaaS.Application.DTOs.Inventory;

using NovaSaaS.Domain.Entities.Inventory;
using NovaSaaS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NovaSaaS.Application.Services.Inventory
{
    /// <summary>
    /// ProductService - Quản lý sản phẩm (Entity chính của Inventory).
    /// 
    /// Xử lý logic:
    /// - Tự động tạo SKU
    /// - Kiểm tra trùng SKU trong Tenant
    /// - Quản lý tồn kho cơ bản
    /// </summary>
    public class ProductService : BaseService<Product>
    {
        public ProductService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
            : base(unitOfWork, currentUser)
        {
        }

        #region Query Operations

        /// <summary>
        /// Lấy danh sách sản phẩm.
        /// </summary>
        public async Task<List<ProductListDto>> GetProductListAsync(
            string? searchTerm = null,
            Guid? categoryId = null,
            bool? isActive = null)
        {
            var products = await _repository
                .GetAllAsync(p => p.Category, p => p.Unit);

            var query = products.AsQueryable();

            // Filter by search term
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(p => 
                    p.Name.ToLower().Contains(term) || 
                    p.SKU.ToLower().Contains(term) ||
                    (p.Barcode != null && p.Barcode.ToLower().Contains(term)));
            }

            // Filter by category
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            // Filter by active status
            if (isActive.HasValue)
            {
                query = query.Where(p => p.IsActive == isActive);
            }

            return query
                .OrderByDescending(p => p.CreateAt)
                .Select(p => new ProductListDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    ImageUrl = p.ImageUrl,
                    CategoryName = p.Category.Name,
                    UnitName = p.Unit.Name,
                    IsActive = p.IsActive,
                    IsLowStock = p.StockQuantity <= p.MinStockLevel
                })
                .ToList();
        }

        /// <summary>
        /// Lấy chi tiết sản phẩm.
        /// </summary>
        public async Task<ProductDetailDto?> GetProductDetailAsync(Guid id)
        {
            var product = await _repository.GetByIdAsync(id, p => p.Category, p => p.Unit);
            if (product == null) return null;

            // Lấy tồn kho theo từng kho
            var stocks = await _unitOfWork.Repository<Stock>()
                .FindAsync(s => s.ProductId == id, s => s.Warehouse);

            return new ProductDetailDto
            {
                Id = product.Id,
                Name = product.Name,
                SKU = product.SKU,
                Description = product.Description,
                Price = product.Price,
                CostPrice = product.CostPrice,
                StockQuantity = product.StockQuantity,
                MinStockLevel = product.MinStockLevel,
                ImageUrl = product.ImageUrl,
                Barcode = product.Barcode,
                IsActive = product.IsActive,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                UnitId = product.UnitId,
                UnitName = product.Unit.Name,
                CreateAt = product.CreateAt,
                UpdateAt = product.UpdateAt,
                StockByWarehouse = stocks.Select(s => new StockByWarehouseDto
                {
                    WarehouseId = s.WarehouseId,
                    WarehouseName = s.Warehouse.Name,
                    Quantity = s.Quantity,
                    ReservedQuantity = s.ReservedQuantity,
                    AvailableQuantity = s.AvailableQuantity,
                    Location = s.Location
                }).ToList()
            };
        }

        #endregion

        #region Create Product

        /// <summary>
        /// Tạo sản phẩm mới.
        /// </summary>
        public async Task<ProductDetailDto> CreateProductAsync(CreateProductDto dto)
        {
            // Kiểm tra Category và Unit tồn tại
            var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId);
            if (category == null)
                throw new ArgumentException("Danh mục không tồn tại");

            var unit = await _unitOfWork.Units.GetByIdAsync(dto.UnitId);
            if (unit == null)
                throw new ArgumentException("Đơn vị tính không tồn tại");

            // Tự động tạo SKU nếu không cung cấp
            var sku = string.IsNullOrWhiteSpace(dto.SKU) 
                ? await GenerateSKUAsync(dto.Name) 
                : dto.SKU;

            // Kiểm tra trùng SKU
            var skuExists = await _repository.AnyAsync(p => p.SKU.ToLower() == sku.ToLower());
            if (skuExists)
                throw new InvalidOperationException($"Mã SKU '{sku}' đã tồn tại");

            // Tạo sản phẩm
            var product = new Product
            {
                Name = dto.Name,
                SKU = sku.ToUpper(),
                Description = dto.Description,
                Price = dto.Price,
                CostPrice = dto.CostPrice,
                MinStockLevel = dto.MinStockLevel,
                Barcode = dto.Barcode,
                CategoryId = dto.CategoryId,
                UnitId = dto.UnitId,
                StockQuantity = dto.InitialStock
            };

            await CreateAsync(product);

            // Nếu có tồn kho ban đầu và kho được chỉ định
            if (dto.InitialStock > 0 && dto.WarehouseId.HasValue)
            {
                var stock = new Stock
                {
                    ProductId = product.Id,
                    WarehouseId = dto.WarehouseId.Value,
                    Quantity = dto.InitialStock
                };
                _unitOfWork.Repository<Stock>().Add(stock);

                // Ghi log nhập kho
                var movement = new StockMovement
                {
                    ProductId = product.Id,
                    WarehouseId = dto.WarehouseId.Value,
                    Quantity = dto.InitialStock,
                    Type = StockMovementType.In,
                    QuantityBefore = 0,
                    QuantityAfter = dto.InitialStock,
                    Notes = "Tồn kho ban đầu"
                };
                _unitOfWork.StockMovements.Add(movement);

                await _unitOfWork.CompleteAsync();
            }

            return (await GetProductDetailAsync(product.Id))!;
        }

        #endregion

        #region Update Product

        /// <summary>
        /// Cập nhật sản phẩm.
        /// </summary>
        public async Task<ProductDetailDto?> UpdateProductAsync(Guid id, UpdateProductDto dto)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null) return null;

            // Kiểm tra Category và Unit
            var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId);
            if (category == null)
                throw new ArgumentException("Danh mục không tồn tại");

            var unit = await _unitOfWork.Units.GetByIdAsync(dto.UnitId);
            if (unit == null)
                throw new ArgumentException("Đơn vị tính không tồn tại");

            // Lưu giá cũ để audit (nếu cần)
            var oldPrice = product.Price;

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.CostPrice = dto.CostPrice;
            product.MinStockLevel = dto.MinStockLevel;
            product.Barcode = dto.Barcode;
            product.CategoryId = dto.CategoryId;
            product.UnitId = dto.UnitId;
            product.IsActive = dto.IsActive;

            await UpdateAsync(product);

            return await GetProductDetailAsync(id);
        }

        #endregion

        #region SKU Generation

        /// <summary>
        /// Tự động tạo SKU từ tên sản phẩm.
        /// Format: 3 chữ cái đầu + 6 số ngẫu nhiên
        /// </summary>
        private async Task<string> GenerateSKUAsync(string productName)
        {
            // Lấy 3 ký tự đầu tiên từ tên (bỏ dấu, viết hoa)
            var prefix = RemoveDiacritics(productName)
                .Replace(" ", "")
                .Substring(0, Math.Min(3, productName.Length))
                .ToUpper();

            // Thêm số ngẫu nhiên
            var random = new Random();
            string sku;
            int attempts = 0;

            do
            {
                var number = random.Next(100000, 999999);
                sku = $"{prefix}{number}";
                attempts++;
            }
            while (await _repository.AnyAsync(p => p.SKU == sku) && attempts < 10);

            if (attempts >= 10)
            {
                // Fallback: Dùng timestamp
                sku = $"{prefix}{DateTime.UtcNow:yyMMddHHmm}";
            }

            return sku;
        }

        /// <summary>
        /// Loại bỏ dấu tiếng Việt.
        /// </summary>
        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
            var stringBuilder = new System.Text.StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }

        #endregion

        protected override Task ValidateCreateAsync(Product entity)
        {
            if (string.IsNullOrWhiteSpace(entity.Name))
                throw new ArgumentException("Tên sản phẩm không được để trống");
            if (entity.Price < 0)
                throw new ArgumentException("Giá bán không được âm");
            return Task.CompletedTask;
        }
    }
}
