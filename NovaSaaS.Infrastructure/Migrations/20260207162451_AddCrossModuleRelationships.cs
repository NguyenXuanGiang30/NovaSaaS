using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaSaaS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCrossModuleRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BankAccountId",
                schema: "public",
                table: "VendorPayments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeId",
                schema: "public",
                table: "Timesheets",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ExpenseAccountId",
                schema: "public",
                table: "ProjectExpenses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "JournalEntryId",
                schema: "public",
                table: "ProjectExpenses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "JournalEntryId",
                schema: "public",
                table: "Payrolls",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SalaryExpenseAccountId",
                schema: "public",
                table: "Payrolls",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BankAccountId",
                schema: "public",
                table: "PaymentTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendorPayments_BankAccountId",
                schema: "public",
                table: "VendorPayments",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Timesheets_EmployeeId",
                schema: "public",
                table: "Timesheets",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectExpenses_ExpenseAccountId",
                schema: "public",
                table: "ProjectExpenses",
                column: "ExpenseAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectExpenses_JournalEntryId",
                schema: "public",
                table: "ProjectExpenses",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_Payrolls_JournalEntryId",
                schema: "public",
                table: "Payrolls",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_Payrolls_SalaryExpenseAccountId",
                schema: "public",
                table: "Payrolls",
                column: "SalaryExpenseAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_BankAccountId",
                schema: "public",
                table: "PaymentTransactions",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceipts_WarehouseId",
                schema: "public",
                table: "GoodsReceipts",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsReceipts_Warehouses_WarehouseId",
                schema: "public",
                table: "GoodsReceipts",
                column: "WarehouseId",
                principalSchema: "public",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTransactions_BankAccounts_BankAccountId",
                schema: "public",
                table: "PaymentTransactions",
                column: "BankAccountId",
                principalSchema: "public",
                principalTable: "BankAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payrolls_ChartOfAccounts_SalaryExpenseAccountId",
                schema: "public",
                table: "Payrolls",
                column: "SalaryExpenseAccountId",
                principalSchema: "public",
                principalTable: "ChartOfAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payrolls_JournalEntries_JournalEntryId",
                schema: "public",
                table: "Payrolls",
                column: "JournalEntryId",
                principalSchema: "public",
                principalTable: "JournalEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectExpenses_ChartOfAccounts_ExpenseAccountId",
                schema: "public",
                table: "ProjectExpenses",
                column: "ExpenseAccountId",
                principalSchema: "public",
                principalTable: "ChartOfAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectExpenses_JournalEntries_JournalEntryId",
                schema: "public",
                table: "ProjectExpenses",
                column: "JournalEntryId",
                principalSchema: "public",
                principalTable: "JournalEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Timesheets_Employees_EmployeeId",
                schema: "public",
                table: "Timesheets",
                column: "EmployeeId",
                principalSchema: "public",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VendorPayments_BankAccounts_BankAccountId",
                schema: "public",
                table: "VendorPayments",
                column: "BankAccountId",
                principalSchema: "public",
                principalTable: "BankAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoodsReceipts_Warehouses_WarehouseId",
                schema: "public",
                table: "GoodsReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTransactions_BankAccounts_BankAccountId",
                schema: "public",
                table: "PaymentTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Payrolls_ChartOfAccounts_SalaryExpenseAccountId",
                schema: "public",
                table: "Payrolls");

            migrationBuilder.DropForeignKey(
                name: "FK_Payrolls_JournalEntries_JournalEntryId",
                schema: "public",
                table: "Payrolls");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectExpenses_ChartOfAccounts_ExpenseAccountId",
                schema: "public",
                table: "ProjectExpenses");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectExpenses_JournalEntries_JournalEntryId",
                schema: "public",
                table: "ProjectExpenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Timesheets_Employees_EmployeeId",
                schema: "public",
                table: "Timesheets");

            migrationBuilder.DropForeignKey(
                name: "FK_VendorPayments_BankAccounts_BankAccountId",
                schema: "public",
                table: "VendorPayments");

            migrationBuilder.DropIndex(
                name: "IX_VendorPayments_BankAccountId",
                schema: "public",
                table: "VendorPayments");

            migrationBuilder.DropIndex(
                name: "IX_Timesheets_EmployeeId",
                schema: "public",
                table: "Timesheets");

            migrationBuilder.DropIndex(
                name: "IX_ProjectExpenses_ExpenseAccountId",
                schema: "public",
                table: "ProjectExpenses");

            migrationBuilder.DropIndex(
                name: "IX_ProjectExpenses_JournalEntryId",
                schema: "public",
                table: "ProjectExpenses");

            migrationBuilder.DropIndex(
                name: "IX_Payrolls_JournalEntryId",
                schema: "public",
                table: "Payrolls");

            migrationBuilder.DropIndex(
                name: "IX_Payrolls_SalaryExpenseAccountId",
                schema: "public",
                table: "Payrolls");

            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_BankAccountId",
                schema: "public",
                table: "PaymentTransactions");

            migrationBuilder.DropIndex(
                name: "IX_GoodsReceipts_WarehouseId",
                schema: "public",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "BankAccountId",
                schema: "public",
                table: "VendorPayments");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                schema: "public",
                table: "Timesheets");

            migrationBuilder.DropColumn(
                name: "ExpenseAccountId",
                schema: "public",
                table: "ProjectExpenses");

            migrationBuilder.DropColumn(
                name: "JournalEntryId",
                schema: "public",
                table: "ProjectExpenses");

            migrationBuilder.DropColumn(
                name: "JournalEntryId",
                schema: "public",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "SalaryExpenseAccountId",
                schema: "public",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "BankAccountId",
                schema: "public",
                table: "PaymentTransactions");
        }
    }
}
