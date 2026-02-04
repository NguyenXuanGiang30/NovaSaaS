using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace NovaSaaS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRAGEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsProcessed",
                schema: "public",
                table: "KnowledgeDocuments");

            migrationBuilder.AlterColumn<string>(
                name: "FilePath",
                schema: "public",
                table: "KnowledgeDocuments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                schema: "public",
                table: "KnowledgeDocuments",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "public",
                table: "KnowledgeDocuments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                schema: "public",
                table: "KnowledgeDocuments",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtractedContent",
                schema: "public",
                table: "KnowledgeDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                schema: "public",
                table: "KnowledgeDocuments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "FileType",
                schema: "public",
                table: "KnowledgeDocuments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedAt",
                schema: "public",
                table: "KnowledgeDocuments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SegmentCount",
                schema: "public",
                table: "KnowledgeDocuments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "public",
                table: "KnowledgeDocuments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                schema: "public",
                table: "KnowledgeDocuments",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Vector>(
                name: "Embedding",
                schema: "public",
                table: "DocumentSegments",
                type: "vector",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EndPosition",
                schema: "public",
                table: "DocumentSegments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                schema: "public",
                table: "DocumentSegments",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SegmentIndex",
                schema: "public",
                table: "DocumentSegments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StartPosition",
                schema: "public",
                table: "DocumentSegments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TokenCount",
                schema: "public",
                table: "DocumentSegments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CompletionTokens",
                schema: "public",
                table: "ChatHistories",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "ConfidenceScore",
                schema: "public",
                table: "ChatHistories",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PromptTokens",
                schema: "public",
                table: "ChatHistories",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResponseTimeMs",
                schema: "public",
                table: "ChatHistories",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetrievedCount",
                schema: "public",
                table: "ChatHistories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RetrievedSegmentIds",
                schema: "public",
                table: "ChatHistories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserFeedback",
                schema: "public",
                table: "ChatHistories",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserRating",
                schema: "public",
                table: "ChatHistories",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                schema: "public",
                table: "KnowledgeDocuments");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                schema: "public",
                table: "KnowledgeDocuments");

            migrationBuilder.DropColumn(
                name: "ExtractedContent",
                schema: "public",
                table: "KnowledgeDocuments");

            migrationBuilder.DropColumn(
                name: "FileSize",
                schema: "public",
                table: "KnowledgeDocuments");

            migrationBuilder.DropColumn(
                name: "FileType",
                schema: "public",
                table: "KnowledgeDocuments");

            migrationBuilder.DropColumn(
                name: "ProcessedAt",
                schema: "public",
                table: "KnowledgeDocuments");

            migrationBuilder.DropColumn(
                name: "SegmentCount",
                schema: "public",
                table: "KnowledgeDocuments");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "KnowledgeDocuments");

            migrationBuilder.DropColumn(
                name: "Tags",
                schema: "public",
                table: "KnowledgeDocuments");

            migrationBuilder.DropColumn(
                name: "Embedding",
                schema: "public",
                table: "DocumentSegments");

            migrationBuilder.DropColumn(
                name: "EndPosition",
                schema: "public",
                table: "DocumentSegments");

            migrationBuilder.DropColumn(
                name: "Metadata",
                schema: "public",
                table: "DocumentSegments");

            migrationBuilder.DropColumn(
                name: "SegmentIndex",
                schema: "public",
                table: "DocumentSegments");

            migrationBuilder.DropColumn(
                name: "StartPosition",
                schema: "public",
                table: "DocumentSegments");

            migrationBuilder.DropColumn(
                name: "TokenCount",
                schema: "public",
                table: "DocumentSegments");

            migrationBuilder.DropColumn(
                name: "CompletionTokens",
                schema: "public",
                table: "ChatHistories");

            migrationBuilder.DropColumn(
                name: "ConfidenceScore",
                schema: "public",
                table: "ChatHistories");

            migrationBuilder.DropColumn(
                name: "PromptTokens",
                schema: "public",
                table: "ChatHistories");

            migrationBuilder.DropColumn(
                name: "ResponseTimeMs",
                schema: "public",
                table: "ChatHistories");

            migrationBuilder.DropColumn(
                name: "RetrievedCount",
                schema: "public",
                table: "ChatHistories");

            migrationBuilder.DropColumn(
                name: "RetrievedSegmentIds",
                schema: "public",
                table: "ChatHistories");

            migrationBuilder.DropColumn(
                name: "UserFeedback",
                schema: "public",
                table: "ChatHistories");

            migrationBuilder.DropColumn(
                name: "UserRating",
                schema: "public",
                table: "ChatHistories");

            migrationBuilder.AlterColumn<string>(
                name: "FilePath",
                schema: "public",
                table: "KnowledgeDocuments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                schema: "public",
                table: "KnowledgeDocuments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<bool>(
                name: "IsProcessed",
                schema: "public",
                table: "KnowledgeDocuments",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
