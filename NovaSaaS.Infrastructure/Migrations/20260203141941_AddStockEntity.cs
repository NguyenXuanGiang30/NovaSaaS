using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaSaaS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStockEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryID",
                schema: "public",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "CategoryID",
                schema: "public",
                table: "Products",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_CategoryID",
                schema: "public",
                table: "Products",
                newName: "IX_Products_CategoryId");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "public",
                table: "Warehouses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "Warehouses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "public",
                table: "Warehouses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "public",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "public",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "public",
                table: "Units",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "Units",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "public",
                table: "Units",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "public",
                table: "TenantSettings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "TenantSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "public",
                table: "TenantSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "public",
                table: "Tenants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "Tenants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "public",
                table: "Tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "public",
                table: "SubscriptionPlans",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "SubscriptionPlans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "public",
                table: "SubscriptionPlans",
                type: "text",
                nullable: true);

            // Chuyển đổi Type từ text sang integer với mapping
            // 0=In, 1=Out, 2=TransferOut, 3=TransferIn, 4=AdjustmentAdd, 5=AdjustmentSubtract, 6=Sale, 7=Return
            migrationBuilder.Sql(@"
                ALTER TABLE public.""StockMovements"" 
                ALTER COLUMN ""Type"" TYPE integer 
                USING (
                    CASE ""Type""
                        WHEN 'In' THEN 0
                        WHEN 'Out' THEN 1
                        WHEN 'TransferOut' THEN 2
                        WHEN 'TransferIn' THEN 3
                        WHEN 'AdjustmentAdd' THEN 4
                        WHEN 'AdjustmentSubtract' THEN 5
                        WHEN 'Sale' THEN 6
                        WHEN 'Return' THEN 7
                        ELSE 0
                    END
                );
            ");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "public",
                table: "StockMovements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DestinationWarehouseId",
                schema: "public",
                table: "StockMovements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "StockMovements",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                schema: "public",
                table: "StockMovements",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuantityAfter",
                schema: "public",
                table: "StockMovements",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuantityBefore",
                schema: "public",
                table: "StockMovements",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceCode",
                schema: "public",
                table: "StockMovements",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReferenceId",
                schema: "public",
                table: "StockMovements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "public",
                table: "StockMovements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "public",
                table: "Roles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "Roles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "public",
                table: "Roles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "public",
                table: "RefreshTokens",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "RefreshTokens",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "public",
                table: "RefreshTokens",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Barcode",
                schema: "public",
                table: "Products",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CostPrice",
                schema: "public",
                table: "Products",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "public",
                table: "Products",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "public",
                table: "Products",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                schema: "public",
                table: "Products",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "public",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MinStockLevel",
                schema: "public",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "public",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "public",
                table: "PlanFeatures",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "PlanFeatures",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "public",
                table: "PlanFeatures",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "public",
                table: "Permissions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "Permissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "public",
                table: "Permissions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "public",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "Payments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "public",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "public",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "public",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "public",
                table: "OrderItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "OrderItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "public",
                table: "OrderItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "public",
                table: "MasterAdmins",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "MasterAdmins",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "public",
                table: "MasterAdmins",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "public",
                table: "KnowledgeDocuments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "KnowledgeDocuments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "public",
                table: "KnowledgeDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "public",
                table: "Invoices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "Invoices",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "public",
                table: "Invoices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "public",
                table: "GlobalAuditLogs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "GlobalAuditLogs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "public",
                table: "GlobalAuditLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "public",
                table: "DocumentSegments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "DocumentSegments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "public",
                table: "DocumentSegments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "public",
                table: "Customers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "Customers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "public",
                table: "Customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "public",
                table: "Coupons",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "Coupons",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "public",
                table: "Coupons",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "public",
                table: "ChatHistories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "ChatHistories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "public",
                table: "ChatHistories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "public",
                table: "Categories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "public",
                table: "Categories",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "public",
                table: "Categories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "Categories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                schema: "public",
                table: "Categories",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "Categories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "public",
                table: "Categories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "public",
                table: "AuditLogs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "AuditLogs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "public",
                table: "AuditLogs",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Stocks",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    ReservedQuantity = table.Column<int>(type: "integer", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: true),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stocks_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "public",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Stocks_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalSchema: "public",
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentId",
                schema: "public",
                table: "Categories",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_ProductId",
                schema: "public",
                table: "Stocks",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_WarehouseId",
                schema: "public",
                table: "Stocks",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Categories_ParentId",
                schema: "public",
                table: "Categories",
                column: "ParentId",
                principalSchema: "public",
                principalTable: "Categories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                schema: "public",
                table: "Products",
                column: "CategoryId",
                principalSchema: "public",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Categories_ParentId",
                schema: "public",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                schema: "public",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Stocks",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_Categories_ParentId",
                schema: "public",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "public",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "public",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "public",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "public",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "public",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "public",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "public",
                table: "TenantSettings");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "TenantSettings");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "public",
                table: "TenantSettings");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "public",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "public",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "public",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "public",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "public",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "DestinationWarehouseId",
                schema: "public",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "Notes",
                schema: "public",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "QuantityAfter",
                schema: "public",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "QuantityBefore",
                schema: "public",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "ReferenceCode",
                schema: "public",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "ReferenceId",
                schema: "public",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "public",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "public",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "public",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "public",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "public",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "Barcode",
                schema: "public",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CostPrice",
                schema: "public",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "public",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "public",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                schema: "public",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "public",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MinStockLevel",
                schema: "public",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "public",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "public",
                table: "PlanFeatures");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "PlanFeatures");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "public",
                table: "PlanFeatures");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "public",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "public",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "public",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "public",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "public",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "public",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "public",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "public",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "public",
                table: "MasterAdmins");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "MasterAdmins");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "public",
                table: "MasterAdmins");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "public",
                table: "KnowledgeDocuments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "KnowledgeDocuments");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "public",
                table: "KnowledgeDocuments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "public",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "public",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "public",
                table: "GlobalAuditLogs");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "GlobalAuditLogs");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "public",
                table: "GlobalAuditLogs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "public",
                table: "DocumentSegments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "DocumentSegments");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "public",
                table: "DocumentSegments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "public",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "public",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "public",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "public",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "public",
                table: "ChatHistories");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "ChatHistories");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "public",
                table: "ChatHistories");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "public",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "public",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "public",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "ParentId",
                schema: "public",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "public",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "public",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "public",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "public",
                table: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                schema: "public",
                table: "Products",
                newName: "CategoryID");

            migrationBuilder.RenameIndex(
                name: "IX_Products_CategoryId",
                schema: "public",
                table: "Products",
                newName: "IX_Products_CategoryID");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                schema: "public",
                table: "StockMovements",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryID",
                schema: "public",
                table: "Products",
                column: "CategoryID",
                principalSchema: "public",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
