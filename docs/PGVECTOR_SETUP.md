# Hướng dẫn cài đặt pgvector cho PostgreSQL

Hệ thống RAG Pipeline yêu cầu extension `pgvector` trên PostgreSQL để lưu trữ và tìm kiếm vector embeddings.
Bạn đang gặp lỗi: `0A000: extension "vector" is not available`.

## Cách 1: Sử dụng Docker (Khuyên dùng)
Cách nhanh nhất là sử dụng Docker image có sẵn `pgvector`.

1. Stop container Postgres hiện tại (nếu có):
   ```bash
   docker stop postgres_container_name
   docker rm postgres_container_name
   ```

2. Chạy container mới với pgvector:
   ```bash
   docker run -d --name novas_postgres -p 5432:5432 -e POSTGRES_PASSWORD=giang2005 -e POSTGRES_DB=NovaSaaS_Db pgvector/pgvector:pg16
   ```

## Cách 2: Cài đặt trên Windows (Nếu không dùng Docker)
Nếu bạn đang chạy PostgreSQL native trên Windows:

1. **Yêu cầu:** PostgreSQL đã được cài đặt.
2. **Download:** Truy cập https://github.com/pgvector/pgvector
3. **Cài đặt:**
   - Bạn cần làm theo hướng dẫn "Windows" trong docs của họ.
   - Hoặc đơn giản hơn, hãy dùng Docker nếu có thể.
   - Lưu ý: Cài đặt thủ công trên Windows khá phức tạp vì cần compile C++.

## Sau khi cài đặt xong
Sau khi đã có PostgreSQL với pgvector, hãy báo cho AI assistant để tiếp tục chạy migration:
```bash
dotnet ef migrations add AddRAGEntities -p NovaSaaS.Infrastructure -s NovaSaaSWebAPI
dotnet ef database update -p NovaSaaS.Infrastructure -s NovaSaaSWebAPI
```
