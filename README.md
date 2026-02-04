# 🚀 NovaSaaS - Enterprise Multi-tenant SaaS Platform

[![NovaSaaS CI](https://github.com/NguyenXuanGiang30/NovaSaaS/actions/workflows/ci.yml/badge.svg)](https://github.com/NguyenXuanGiang30/NovaSaaS/actions/workflows/ci.yml)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791)
![AI](https://img.shields.io/badge/AI-Gemini_Flash-8E75B2)

**NovaSaaS** là nền tảng quản lý bán hàng đa chi nhánh (SaaS) cấp doanh nghiệp, được xây dựng với kiến trúc **Clean Architecture**, tích hợp **AI Agent**, và tối ưu hóa cho hiệu năng cao.

> 📚 **Tài liệu chi tiết**: Xem [Báo Cáo Tổng Quan Hệ Thống (PROJECT_OVERVIEW.md)](PROJECT_OVERVIEW.md) để biết thêm về kiến trúc và database schema.

---

## 🔥 Tính Năng Nổi Bật

### 🏗️ Core & Infrastructure
*   **Multi-tenancy**: Cô lập dữ liệu tuyệt đối bằng Schema Isolation.
*   **Clean Architecture**: Phân tách 4 tầng rõ ràng (Domain, Application, Infrastructure, WebAPI).
*   **Performance**: Redis Caching (Distributed), HNSW Vector Indexing.
*   **Real-time**: SignalR Notifications (Đơn hàng, Tồn kho).
*   **Background Jobs**: Hangfire (Email, Reports, System Checks).

### 🤖 AI Agent & RAG (New)
*   **RAG Pipeline**: Tìm kiếm ngữ nghĩa (Semantic Search) trên tài liệu doanh nghiệp.
*   **Function Calling**: AI tự động thực hiện hành động (Tra cứu tồn kho, Kiểm tra đơn hàng).
*   **Vector Search**: Sử dụng `pgvector` với thuật toán HNSW tối ưu tốc độ.

### 💼 Business Modules
*   **Inventory**: Quản lý đa kho, nhập/xuất/chuyển kho.
*   **Sales**: Bán hàng, Đơn hàng, Hóa đơn, Thanh toán (Stripe).
*   **CRM**: Quản lý khách hàng, phân hạng thành viên.
*   **Reporting**: Báo cáo doanh thu, xuất Excel.

### 🛡️ Enterprise Grade
*   **Security**: Rate Limiting (Token Bucket), Data Masking (PII).
*   **Observability**: OpenTelemetry (Tracing, Metrics), Grafana Dashboard.

---

## 🛠️ Tech Stack

*   **Backend**: .NET 10, C# 13, ASP.NET Core Web API.
*   **Database**: PostgreSQL 16 + `pgvector` extension.
*   **AI/LLM**: Google Gemini 1.5 Flash, `text-embedding-004`.
*   **Cache/Queue**: Redis, Hangfire.
*   **Testing**: xUnit, FluentAssertions, Testcontainers.

---

## 🚀 Hướng Dẫn Cài Đặt

### 1. Yêu Cầu Hệ Thống
*   [.NET 10 SDK](https://dotnet.microsoft.com/download)
*   [Docker Desktop](https://www.docker.com/products/docker-desktop) (cho PostgreSQL & Redis)

### 2. Khởi Chạy Infrastructure
Sử dụng Docker Compose để chạy PostgreSQL và Redis:

```bash
docker-compose up -d
```

### 3. Cấu Hình
Cập nhật file `NovaSaaSWebAPI/appsettings.json` (nếu cần):
*   **ConnectionStrings**: `DefaultConnection` (PostgreSQL)
*   **GeminiSettings**: `ApiKey` (Google AI Studio Key)

### 4. Chạy Database Migrations
Hệ thống sử dụng cơ chế migration tự động cho tenants, nhưng cần khởi tạo database master:

```bash
cd NovaSaaS.Infrastructure
dotnet ef database update --context ApplicationDbContext
```

### 5. Build & Run
```bash
dotnet build
dotnet run --project NovaSaaSWebAPI
```

API sẽ chạy tại: `https://localhost:7129`

---

## 🧪 Testing

Chạy toàn bộ Unit Tests và Integration Tests:

```bash
dotnet test
```

Hiện tại dự án đạt **100% Build Success** với **0 Errors, 0 Warnings**.

---

## 📂 Cấu Trúc Dự Án

```
NovaSaaS/
├── NovaSaaS.Domain/           # Entities, Enums, Constants
├── NovaSaaS.Application/      # Interfaces, Services, Features
├── NovaSaaS.Infrastructure/   # EF Core, Migrations, External Services
├── NovaSaaSWebAPI/            # Controllers, Middleware
└── NovaSaaS.UnitTests/        # Tests
```

---

## 📝 License
Copyright © 2026 NovaSaaS Team.
