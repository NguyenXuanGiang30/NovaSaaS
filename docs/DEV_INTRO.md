# 👋 CHÀO MỪNG ĐẾN VỚI DỰ ÁN NOVASAAS!
# Tài liệu giới thiệu dành cho đội ngũ phát triển

---

## 🎯 DỰ ÁN NÀY LÀ GÌ?

**NovaSaaS** là nền tảng **ERP SaaS đa năng** giúp doanh nghiệp SME quản lý toàn bộ hoạt động kinh doanh trên một nền tảng duy nhất.

```
┌─────────────────────────────────────────────────────────────────┐
│                                                                 │
│   "Một nền tảng - Mọi nghiệp vụ - Mọi doanh nghiệp"            │
│                                                                 │
│   ERP + CRM + HRM + Kế toán + Kho + Mua hàng + Dự án + AI      │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🤔 TẠI SAO LÀM DỰ ÁN NÀY?

### Vấn đề hiện tại của SME Việt Nam:

| ❌ Vấn đề | 💡 NovaSaaS giải quyết |
|----------|------------------------|
| Phần mềm ERP đắt (100-500 triệu) | Chỉ từ 500K/tháng |
| Cài đặt phức tạp, mất vài tháng | Đăng ký → Dùng ngay 5 phút |
| Dùng nhiều phần mềm rời rạc | 1 nền tảng tích hợp tất cả |
| Không có AI hỗ trợ | AI Assistant thông minh |
| Mua gói lớn, không dùng hết | Chọn module cần, trả tiền đủ |

---

## 🏗️ CẤU TRÚC DỰ ÁN

### Các Module:

```
┌─────────────────────────────────────────────────────────────────┐
│                         ERP CORE                                │
│              (Bắt buộc - Nền tảng cơ sở)                       │
│   ┌─────────┬──────────┬───────────┬─────────────┬──────────┐  │
│   │   IAM   │ Settings │ Dashboard │Notification │    AI    │  │
│   │Phân quyền│ Cấu hình │ Tổng quan │  Thông báo  │ Trợ lý   │  │
│   └─────────┴──────────┴───────────┴─────────────┴──────────┘  │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                    OPTIONAL MODULES                             │
│              (Khách hàng tự chọn theo nhu cầu)                 │
│                                                                 │
│   ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐           │
│   │   HRM   │  │   CRM   │  │   INV   │  │   ACC   │           │
│   │ Nhân sự │  │ Bán hàng│  │   Kho   │  │ Kế toán │           │
│   │17 entity│  │15 entity│  │13 entity│  │15 entity│           │
│   └─────────┘  └─────────┘  └─────────┘  └─────────┘           │
│                                                                 │
│   ┌─────────┐  ┌─────────┐  ┌─────────┐                        │
│   │   SCM   │  │   PM    │  │   DMS   │                        │
│   │ Mua hàng│  │ Dự án   │  │Tài liệu │                        │
│   │10 entity│  │12 entity│  │11 entity│                        │
│   └─────────┘  └─────────┘  └─────────┘                        │
│                                                                 │
│   TỔNG: 8 modules, ~103 entities                               │
└─────────────────────────────────────────────────────────────────┘
```

### Giải thích từng module:

| Module | Tên đầy đủ | Làm gì? |
|--------|-----------|---------|
| **CORE** | ERP Core | Đăng nhập, phân quyền, cấu hình, dashboard |
| **HRM** | Human Resource | Quản lý nhân viên, chấm công, tính lương |
| **CRM** | Customer Relationship | Khách hàng, đơn hàng, hóa đơn |
| **INV** | Inventory | Sản phẩm, kho, tồn kho, xuất nhập |
| **ACC** | Accounting | Sổ kế toán, công nợ, báo cáo tài chính |
| **SCM** | Supply Chain | Nhà cung cấp, đơn mua hàng |
| **PM** | Project Management | Dự án, task, timesheet |
| **DMS** | Document Management | Quản lý tài liệu, file |

---

## 🛠️ CÔNG NGHỆ SỬ DỤNG

```
┌─────────────────────────────────────────────────────────────────┐
│                      TECH STACK                                 │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   FRONTEND                    BACKEND                           │
│   ════════                    ═══════                           │
│   • Next.js 14 (React)        • .NET 8 (C#)                    │
│   • TypeScript                • Clean Architecture             │
│   • TailwindCSS               • Entity Framework Core          │
│                                                                 │
│   DATABASE                    INFRASTRUCTURE                    │
│   ════════                    ══════════════                    │
│   • PostgreSQL 16             • Docker                          │
│   • Redis (cache)             • GitHub Actions (CI/CD)          │
│                                                                 │
│   AI                                                            │
│   ══                                                            │
│   • OpenAI / Gemini API                                         │
│   • RAG (Retrieval Augmented Generation)                        │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## 👥 ĐỐI TƯỢNG SỬ DỤNG (ACTORS)

### Cấp hệ thống (System Level):

| Actor | Vai trò |
|-------|---------|
| **Master Admin** | Quản trị toàn bộ hệ thống, tất cả tenants |
| **Support Staff** | Hỗ trợ khách hàng, xem dữ liệu |

### Cấp doanh nghiệp (Tenant Level):

| Actor | Vai trò |
|-------|---------|
| **Tenant Admin** | Chủ doanh nghiệp, full quyền trong tenant |
| **Manager** | Quản lý bộ phận, duyệt đơn |
| **HR Manager** | Quản lý nhân sự |
| **Sales Staff** | Nhân viên bán hàng |
| **Warehouse Staff** | Nhân viên kho |
| **Accountant** | Kế toán viên |
| **Employee** | Nhân viên thường (xem lương, nghỉ phép) |
| **Customer** | Khách hàng (portal khách hàng) |

---

## 🔑 ĐIỂM ĐẶC BIỆT CỦA DỰ ÁN

### 1. Multi-tenant Architecture
```
Mỗi khách hàng (tenant) có database schema riêng
→ Dữ liệu hoàn toàn tách biệt
→ Bảo mật cao
```

### 2. Modular Design
```
Khách hàng mua module nào → Bật module đó
→ Không trả tiền cho thứ không dùng
→ Flexible pricing
```

### 3. AI Assistant
```
Tích hợp AI để:
→ Hỏi đáp thông tin
→ Tạo báo cáo tự động
→ Gợi ý hành động
```

### 4. Tích hợp liền mạch
```
Bán hàng (CRM) → Trừ kho (INV) → Ghi sổ (ACC)
→ Tự động, không cần thao tác thủ công
```

---

## 📁 CẤU TRÚC THƯ MỤC

```
NovaSaaS/
│
├── 📂 NovaSaaS.Domain/          ← Entities, business logic
├── 📂 NovaSaaS.Application/     ← Use cases, services
├── 📂 NovaSaaS.Infrastructure/  ← Database, external services
├── 📂 NovaSaaSWebAPI/           ← API controllers
├── 📂 NovaSaaS.UnitTests/       ← Unit tests
│
├── 📂 docs/                     ← Tài liệu
│   ├── ERP_MODULES_SPECIFICATION.md  ← Đặc tả chi tiết
│   ├── NOVASAAS_PITCH_DECK.md        ← Tài liệu cho NĐT
│   ├── DEV_GUIDE.md                  ← Hướng dẫn kỹ thuật
│   └── DEV_INTRO.md                  ← Tài liệu này
│
├── 📄 docker-compose.yml        ← Chạy local
└── 📄 README.md                 ← Hướng dẫn chung
```

---

## 🚀 BẮT ĐẦU NHƯ THẾ NÀO?

### Bước 1: Đọc tài liệu (30 phút)
```
1. Đọc file này (DEV_INTRO.md) - 10 phút
2. Đọc ERP_MODULES_SPECIFICATION.md (phần liên quan) - 20 phút
```

### Bước 2: Setup môi trường (30 phút)
```bash
# Clone code
git clone https://github.com/xxx/NovaSaaS.git

# Chạy database
docker-compose up -d

# Chạy backend
cd NovaSaaSWebAPI && dotnet run

# Chạy frontend
cd frontend && npm install && npm run dev
```

### Bước 3: Khám phá code (1 giờ)
```
1. Xem cấu trúc NovaSaaS.Domain/Entities
2. Xem API endpoints trong NovaSaaSWebAPI/Controllers
3. Chạy thử Swagger UI tại http://localhost:5000/swagger
```

---

## 📋 PHÂN CÔNG CÔNG VIỆC (GỢI Ý)

| Dev | Focus Area | Modules |
|-----|------------|---------|
| Dev 1 | Backend Core | Core + Auth + IAM |
| Dev 2 | Backend Business | CRM + INV |
| Dev 3 | Backend Business | ACC + SCM |
| Dev 4 | Frontend | Dashboard + CRM |
| Dev 5 | Frontend | INV + ACC |

---

## 📞 CẦN HỖ TRỢ?

| Tài liệu | Mô tả |
|----------|-------|
| `ERP_MODULES_SPECIFICATION.md` | Chi tiết entities, chức năng từng module |
| `DEV_GUIDE.md` | Hướng dẫn kỹ thuật chi tiết |
| `NOVASAAS_PITCH_DECK.md` | Tổng quan business (cho context) |

---

## 💪 LET'S BUILD SOMETHING GREAT!

```
┌─────────────────────────────────────────────────────────────────┐
│                                                                 │
│        "Code hôm nay - Triệu doanh nghiệp dùng ngày mai"       │
│                                                                 │
│                     🚀 NovaSaaS Team 🚀                         │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

*Welcome to the team! 🎉*
