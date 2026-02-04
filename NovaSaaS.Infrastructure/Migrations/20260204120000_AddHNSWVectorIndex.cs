using Microsoft.EntityFrameworkCore.Migrations;

namespace NovaSaaS.Infrastructure.Migrations
{
    /// <summary>
    /// Migration để tạo HNSW index cho vector search optimization.
    /// 
    /// HNSW (Hierarchical Navigable Small World) là thuật toán tìm kiếm 
    /// approximate nearest neighbor (ANN) hiệu quả cao.
    /// 
    /// Tham số:
    /// - m: số lượng edges per node (default 16, cao hơn = chính xác hơn nhưng chậm hơn)
    /// - ef_construction: số candidate xem xét khi build (default 64)
    /// </summary>
    public partial class AddHNSWVectorIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tạo HNSW index cho embedding column trong DocumentSegments
            // Sử dụng cosine distance operator class
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ix_document_segments_embedding_hnsw
                ON ""DocumentSegments"" 
                USING hnsw (""Embedding"" vector_cosine_ops)
                WITH (m = 16, ef_construction = 64);
            ");

            // Optionally: Create GIN index cho text search trên Content
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ix_document_segments_content_gin
                ON ""DocumentSegments"" 
                USING gin (to_tsvector('english', ""Content""));
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS ix_document_segments_embedding_hnsw;
            ");
            
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS ix_document_segments_content_gin;
            ");
        }
    }
}
