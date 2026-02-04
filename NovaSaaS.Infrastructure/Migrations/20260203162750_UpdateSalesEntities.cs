using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaSaaS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSalesEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Orders_OrderID",
                schema: "public",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_OrderID",
                schema: "public",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "InvoiceNo",
                schema: "public",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                schema: "public",
                table: "Invoices");

            migrationBuilder.RenameColumn(
                name: "OderNumber",
                schema: "public",
                table: "Orders",
                newName: "OrderNumber");

            migrationBuilder.RenameColumn(
                name: "OrderID",
                schema: "public",
                table: "Invoices",
                newName: "OrderId");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "Orders");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "public",
                table: "Orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                schema: "public",
                table: "Orders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                schema: "public",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SubTotal",
                schema: "public",
                table: "Orders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                schema: "public",
                table: "Orders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                schema: "public",
                table: "Invoices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceNumber",
                schema: "public",
                table: "Invoices",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidDate",
                schema: "public",
                table: "Invoices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                schema: "public",
                table: "Invoices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "public",
                table: "Invoices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                schema: "public",
                table: "Customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "public",
                table: "Customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Rank",
                schema: "public",
                table: "Customers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalSpending",
                schema: "public",
                table: "Customers",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                schema: "public",
                table: "Customers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_OrderId",
                schema: "public",
                table: "Invoices",
                column: "OrderId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Orders_OrderId",
                schema: "public",
                table: "Invoices",
                column: "OrderId",
                principalSchema: "public",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Orders_OrderId",
                schema: "public",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_OrderId",
                schema: "public",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                schema: "public",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Note",
                schema: "public",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SubTotal",
                schema: "public",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                schema: "public",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DueDate",
                schema: "public",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "InvoiceNumber",
                schema: "public",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PaidDate",
                schema: "public",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                schema: "public",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Address",
                schema: "public",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Email",
                schema: "public",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Rank",
                schema: "public",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "TotalSpending",
                schema: "public",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Type",
                schema: "public",
                table: "Customers");

            migrationBuilder.RenameColumn(
                name: "OrderNumber",
                schema: "public",
                table: "Orders",
                newName: "OderNumber");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                schema: "public",
                table: "Invoices",
                newName: "OrderID");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "public",
                table: "Orders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "InvoiceNo",
                schema: "public",
                table: "Invoices",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                schema: "public",
                table: "Invoices",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_OrderID",
                schema: "public",
                table: "Invoices",
                column: "OrderID");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Orders_OrderID",
                schema: "public",
                table: "Invoices",
                column: "OrderID",
                principalSchema: "public",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
