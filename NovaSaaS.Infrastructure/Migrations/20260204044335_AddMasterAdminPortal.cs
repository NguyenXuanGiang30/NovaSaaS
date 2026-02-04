using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaSaaS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMasterAdminPortal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "public",
                table: "Tenants");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastBillingDate",
                schema: "public",
                table: "Tenants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxMonthlyAICalls",
                schema: "public",
                table: "Tenants",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxStorageMB",
                schema: "public",
                table: "Tenants",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "public",
                table: "Tenants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionEndDate",
                schema: "public",
                table: "Tenants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionStartDate",
                schema: "public",
                table: "Tenants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuspendReason",
                schema: "public",
                table: "Tenants",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AIEnabled",
                schema: "public",
                table: "SubscriptionPlans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowOverage",
                schema: "public",
                table: "SubscriptionPlans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "public",
                table: "SubscriptionPlans",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GracePeriodDays",
                schema: "public",
                table: "SubscriptionPlans",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxDocuments",
                schema: "public",
                table: "SubscriptionPlans",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxMonthlyAICalls",
                schema: "public",
                table: "SubscriptionPlans",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxStorageMB",
                schema: "public",
                table: "SubscriptionPlans",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "OveragePricePer1000Calls",
                schema: "public",
                table: "SubscriptionPlans",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "YearlyPrice",
                schema: "public",
                table: "SubscriptionPlans",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SystemLogs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    ExceptionType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    StackTrace = table.Column<string>(type: "text", nullable: true),
                    RequestPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    HttpMethod = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ClientIp = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DurationMs = table.Column<long>(type: "bigint", nullable: true),
                    AdditionalData = table.Column<string>(type: "text", nullable: true),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemLogs_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "public",
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UsageLogs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    PromptTokens = table.Column<int>(type: "integer", nullable: true),
                    CompletionTokens = table.Column<int>(type: "integer", nullable: true),
                    EstimatedCostUSD = table.Column<decimal>(type: "numeric", nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ModelId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsageLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsageLogs_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "public",
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_TenantId",
                schema: "public",
                table: "SystemLogs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UsageLogs_TenantId",
                schema: "public",
                table: "UsageLogs",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemLogs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "UsageLogs",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "LastBillingDate",
                schema: "public",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "MaxMonthlyAICalls",
                schema: "public",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "MaxStorageMB",
                schema: "public",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "SubscriptionEndDate",
                schema: "public",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "SubscriptionStartDate",
                schema: "public",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "SuspendReason",
                schema: "public",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "AIEnabled",
                schema: "public",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "AllowOverage",
                schema: "public",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "public",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "GracePeriodDays",
                schema: "public",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "MaxDocuments",
                schema: "public",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "MaxMonthlyAICalls",
                schema: "public",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "MaxStorageMB",
                schema: "public",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "OveragePricePer1000Calls",
                schema: "public",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "YearlyPrice",
                schema: "public",
                table: "SubscriptionPlans");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "public",
                table: "Tenants",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
