using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaSaaS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddModuleIntegrationLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                schema: "public",
                table: "PurchaseOrders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ExpenseAccountId",
                schema: "public",
                table: "Projects",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RevenueAccountId",
                schema: "public",
                table: "Projects",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CogsAccountId",
                schema: "public",
                table: "Products",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InventoryAccountId",
                schema: "public",
                table: "Products",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RevenueAccountId",
                schema: "public",
                table: "Products",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BankAccountId",
                schema: "public",
                table: "Payrolls",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                schema: "public",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                schema: "public",
                table: "Invoices",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_ProjectId",
                schema: "public",
                table: "PurchaseOrders",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ExpenseAccountId",
                schema: "public",
                table: "Projects",
                column: "ExpenseAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_RevenueAccountId",
                schema: "public",
                table: "Projects",
                column: "RevenueAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CogsAccountId",
                schema: "public",
                table: "Products",
                column: "CogsAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_InventoryAccountId",
                schema: "public",
                table: "Products",
                column: "InventoryAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_RevenueAccountId",
                schema: "public",
                table: "Products",
                column: "RevenueAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Payrolls_BankAccountId",
                schema: "public",
                table: "Payrolls",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ProjectId",
                schema: "public",
                table: "Orders",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ProjectId",
                schema: "public",
                table: "Invoices",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Projects_ProjectId",
                schema: "public",
                table: "Invoices",
                column: "ProjectId",
                principalSchema: "public",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Projects_ProjectId",
                schema: "public",
                table: "Orders",
                column: "ProjectId",
                principalSchema: "public",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payrolls_BankAccounts_BankAccountId",
                schema: "public",
                table: "Payrolls",
                column: "BankAccountId",
                principalSchema: "public",
                principalTable: "BankAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ChartOfAccounts_CogsAccountId",
                schema: "public",
                table: "Products",
                column: "CogsAccountId",
                principalSchema: "public",
                principalTable: "ChartOfAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ChartOfAccounts_InventoryAccountId",
                schema: "public",
                table: "Products",
                column: "InventoryAccountId",
                principalSchema: "public",
                principalTable: "ChartOfAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ChartOfAccounts_RevenueAccountId",
                schema: "public",
                table: "Products",
                column: "RevenueAccountId",
                principalSchema: "public",
                principalTable: "ChartOfAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_ChartOfAccounts_ExpenseAccountId",
                schema: "public",
                table: "Projects",
                column: "ExpenseAccountId",
                principalSchema: "public",
                principalTable: "ChartOfAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_ChartOfAccounts_RevenueAccountId",
                schema: "public",
                table: "Projects",
                column: "RevenueAccountId",
                principalSchema: "public",
                principalTable: "ChartOfAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Projects_ProjectId",
                schema: "public",
                table: "PurchaseOrders",
                column: "ProjectId",
                principalSchema: "public",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Projects_ProjectId",
                schema: "public",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Projects_ProjectId",
                schema: "public",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Payrolls_BankAccounts_BankAccountId",
                schema: "public",
                table: "Payrolls");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_ChartOfAccounts_CogsAccountId",
                schema: "public",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_ChartOfAccounts_InventoryAccountId",
                schema: "public",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_ChartOfAccounts_RevenueAccountId",
                schema: "public",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_ChartOfAccounts_ExpenseAccountId",
                schema: "public",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_ChartOfAccounts_RevenueAccountId",
                schema: "public",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Projects_ProjectId",
                schema: "public",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_ProjectId",
                schema: "public",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_Projects_ExpenseAccountId",
                schema: "public",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_RevenueAccountId",
                schema: "public",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Products_CogsAccountId",
                schema: "public",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_InventoryAccountId",
                schema: "public",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_RevenueAccountId",
                schema: "public",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Payrolls_BankAccountId",
                schema: "public",
                table: "Payrolls");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ProjectId",
                schema: "public",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_ProjectId",
                schema: "public",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                schema: "public",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "ExpenseAccountId",
                schema: "public",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "RevenueAccountId",
                schema: "public",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CogsAccountId",
                schema: "public",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "InventoryAccountId",
                schema: "public",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "RevenueAccountId",
                schema: "public",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BankAccountId",
                schema: "public",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                schema: "public",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                schema: "public",
                table: "Invoices");
        }
    }
}
