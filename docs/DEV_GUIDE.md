# NOVASAAS - TÀI LIỆU KỸ THUẬT CHO ĐỘI NGŨ PHÁT TRIỂN
# Developer Technical Documentation

> **Phiên bản:** 1.0  
> **Ngày tạo:** 06/02/2026  
> **Mục đích:** Hướng dẫn kỹ thuật chi tiết cho đội ngũ phát triển

---

## 📋 MỤC LỤC

1. [Tổng quan dự án](#1-tổng-quan-dự-án)
2. [Công nghệ sử dụng](#2-công-nghệ-sử-dụng)
3. [Kiến trúc hệ thống](#3-kiến-trúc-hệ-thống)
4. [Cấu trúc source code](#4-cấu-trúc-source-code)
5. [Database Schema](#5-database-schema)
6. [API Specification](#6-api-specification)
7. [Hướng dẫn Setup](#7-hướng-dẫn-setup)
8. [Coding Standards](#8-coding-standards)
9. [Git Workflow](#9-git-workflow)
10. [Testing Guide](#10-testing-guide)
11. [Deployment Guide](#11-deployment-guide)
12. [Troubleshooting](#12-troubleshooting)

---

## 1. TỔNG QUAN DỰ ÁN

### 1.1 Mô tả

NovaSaaS là nền tảng ERP SaaS multi-tenant với các đặc điểm:

| Đặc điểm | Mô tả |
|----------|-------|
| **Loại** | Multi-tenant SaaS Platform |
| **Modules** | 8 modules (Core + 7 optional) |
| **Entities** | ~103 entities |
| **Architecture** | Clean Architecture + DDD |

### 1.2 Modules

```
┌─────────────────────────────────────────────────────────────────┐
│  CORE (Bắt buộc)                                                │
│  ├── Identity & Access Management                               │
│  ├── Tenant Settings                                            │
│  ├── Dashboard                                                  │
│  ├── Notifications                                              │
│  └── AI Assistant                                               │
├─────────────────────────────────────────────────────────────────┤
│  OPTIONAL MODULES                                               │
│  ├── HRM  - Human Resource Management     (17 entities)        │
│  ├── CRM  - Customer Relationship         (15 entities)        │
│  ├── INV  - Inventory Management          (13 entities)        │
│  ├── ACC  - Accounting & Finance          (15 entities)        │
│  ├── SCM  - Supply Chain Management       (10 entities)        │
│  ├── PM   - Project Management            (12 entities)        │
│  └── DMS  - Document Management           (11 entities)        │
└─────────────────────────────────────────────────────────────────┘
```

---

## 2. CÔNG NGHỆ SỬ DỤNG

### 2.1 Tech Stack

| Layer | Technology | Version | Note |
|-------|------------|---------|------|
| **Backend** | .NET | 8.0 LTS | C# 12 |
| **Frontend** | Next.js / React | 14.x | TypeScript |
| **Database** | PostgreSQL | 16.x | Multi-tenant |
| **ORM** | Entity Framework Core | 8.x | Code-first |
| **Cache** | Redis | 7.x | Optional |
| **Queue** | RabbitMQ | 3.x | Optional |
| **Storage** | MinIO / S3 | - | File storage |
| **Container** | Docker | 24.x | + Compose |

### 2.2 Development Tools

| Tool | Purpose | Required |
|------|---------|:--------:|
| Visual Studio 2022 / VS Code | IDE | ✅ |
| .NET 8 SDK | Backend | ✅ |
| Node.js 20+ | Frontend | ✅ |
| Docker Desktop | Containers | ✅ |
| PostgreSQL 16 | Database | ✅ |
| Git | Version control | ✅ |
| Postman / Bruno | API testing | Recommended |
| DBeaver | DB management | Recommended |

### 2.3 NuGet Packages (Backend)

```xml
<!-- Core -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.x" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.x" />

<!-- Authentication -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.x" />

<!-- Validation -->
<PackageReference Include="FluentValidation" Version="11.x" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.x" />

<!-- Mapping -->
<PackageReference Include="AutoMapper" Version="12.x" />

<!-- Logging -->
<PackageReference Include="Serilog.AspNetCore" Version="8.x" />

<!-- API Documentation -->
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.x" />

<!-- Testing -->
<PackageReference Include="xunit" Version="2.x" />
<PackageReference Include="Moq" Version="4.x" />
<PackageReference Include="FluentAssertions" Version="6.x" />
```

---

## 3. KIẾN TRÚC HỆ THỐNG

### 3.1 Clean Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         CLEAN ARCHITECTURE                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   ┌─────────────────────────────────────────────────────────┐   │
│   │                     PRESENTATION                        │   │
│   │         Controllers, Middlewares, Filters               │   │
│   │                   NovaSaaSWebAPI                        │   │
│   └──────────────────────────┬──────────────────────────────┘   │
│                              │                                  │
│   ┌──────────────────────────▼──────────────────────────────┐   │
│   │                     APPLICATION                         │   │
│   │     Use Cases, DTOs, Interfaces, Validators             │   │
│   │                NovaSaaS.Application                     │   │
│   └──────────────────────────┬──────────────────────────────┘   │
│                              │                                  │
│   ┌──────────────────────────▼──────────────────────────────┐   │
│   │                       DOMAIN                            │   │
│   │      Entities, Value Objects, Domain Events             │   │
│   │                  NovaSaaS.Domain                        │   │
│   └──────────────────────────┬──────────────────────────────┘   │
│                              │                                  │
│   ┌──────────────────────────▼──────────────────────────────┐   │
│   │                   INFRASTRUCTURE                        │   │
│   │    EF Core, Repositories, External Services             │   │
│   │               NovaSaaS.Infrastructure                   │   │
│   └─────────────────────────────────────────────────────────┘   │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### 3.2 Multi-tenant Strategy

```
┌─────────────────────────────────────────────────────────────────┐
│                  MULTI-TENANT: SCHEMA PER TENANT                │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   PostgreSQL Database: novasaas_db                              │
│   │                                                             │
│   ├── Schema: public (shared)                                   │
│   │   ├── tenants                                               │
│   │   ├── subscriptions                                         │
│   │   └── master_data                                           │
│   │                                                             │
│   ├── Schema: tenant_abc123                                     │
│   │   ├── users                                                 │
│   │   ├── customers                                             │
│   │   ├── products                                              │
│   │   └── ...                                                   │
│   │                                                             │
│   ├── Schema: tenant_xyz789                                     │
│   │   ├── users                                                 │
│   │   ├── customers                                             │
│   │   └── ...                                                   │
│   │                                                             │
│   └── ...                                                       │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### 3.3 Request Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                      REQUEST FLOW                               │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   Client Request                                                │
│        │                                                        │
│        ▼                                                        │
│   ┌─────────────┐                                               │
│   │   Nginx     │  → SSL termination, rate limiting            │
│   └──────┬──────┘                                               │
│          │                                                      │
│          ▼                                                      │
│   ┌─────────────┐                                               │
│   │ API Gateway │  → Routing, load balancing                   │
│   └──────┬──────┘                                               │
│          │                                                      │
│          ▼                                                      │
│   ┌─────────────────────────────────────────┐                   │
│   │           .NET 8 API                    │                   │
│   │  ┌─────────────────────────────────┐    │                   │
│   │  │ 1. Authentication Middleware    │    │                   │
│   │  │    → JWT validation             │    │                   │
│   │  └───────────────┬─────────────────┘    │                   │
│   │                  ▼                      │                   │
│   │  ┌─────────────────────────────────┐    │                   │
│   │  │ 2. Tenant Resolution Middleware │    │                   │
│   │  │    → Extract tenant from token  │    │                   │
│   │  └───────────────┬─────────────────┘    │                   │
│   │                  ▼                      │                   │
│   │  ┌─────────────────────────────────┐    │                   │
│   │  │ 3. Controller                   │    │                   │
│   │  │    → Handle request             │    │                   │
│   │  └───────────────┬─────────────────┘    │                   │
│   │                  ▼                      │                   │
│   │  ┌─────────────────────────────────┐    │                   │
│   │  │ 4. Application Service          │    │                   │
│   │  │    → Business logic             │    │                   │
│   │  └───────────────┬─────────────────┘    │                   │
│   │                  ▼                      │                   │
│   │  ┌─────────────────────────────────┐    │                   │
│   │  │ 5. Repository (EF Core)         │    │                   │
│   │  │    → Data access                │    │                   │
│   │  └───────────────┬─────────────────┘    │                   │
│   └──────────────────┼──────────────────────┘                   │
│                      ▼                                          │
│   ┌─────────────────────────────────────────┐                   │
│   │           PostgreSQL                    │                   │
│   │    → tenant-specific schema             │                   │
│   └─────────────────────────────────────────┘                   │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## 4. CẤU TRÚC SOURCE CODE

### 4.1 Solution Structure

```
NovaSaaS/
├── NovaSaaS.sln
│
├── src/
│   ├── NovaSaaS.Domain/                 # Domain Layer
│   │   ├── Common/
│   │   │   ├── BaseEntity.cs
│   │   │   ├── AuditableEntity.cs
│   │   │   └── IRepository.cs
│   │   ├── Entities/
│   │   │   ├── Core/
│   │   │   │   ├── Tenant.cs
│   │   │   │   ├── User.cs
│   │   │   │   ├── Role.cs
│   │   │   │   └── Permission.cs
│   │   │   ├── CRM/
│   │   │   │   ├── Customer.cs
│   │   │   │   ├── Order.cs
│   │   │   │   └── Invoice.cs
│   │   │   ├── INV/
│   │   │   │   ├── Product.cs
│   │   │   │   ├── Warehouse.cs
│   │   │   │   └── Stock.cs
│   │   │   └── ... (other modules)
│   │   ├── Enums/
│   │   ├── Exceptions/
│   │   └── Events/
│   │
│   ├── NovaSaaS.Application/            # Application Layer
│   │   ├── Common/
│   │   │   ├── Interfaces/
│   │   │   │   ├── IUnitOfWork.cs
│   │   │   │   └── ICurrentUserService.cs
│   │   │   ├── Mappings/
│   │   │   └── Behaviors/
│   │   ├── Features/
│   │   │   ├── Auth/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── Login/
│   │   │   │   │   │   ├── LoginCommand.cs
│   │   │   │   │   │   ├── LoginCommandHandler.cs
│   │   │   │   │   │   └── LoginCommandValidator.cs
│   │   │   │   │   └── Register/
│   │   │   │   └── Queries/
│   │   │   ├── Customers/
│   │   │   ├── Products/
│   │   │   └── ... (other features)
│   │   └── DTOs/
│   │
│   ├── NovaSaaS.Infrastructure/         # Infrastructure Layer
│   │   ├── Persistence/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Configurations/
│   │   │   │   ├── UserConfiguration.cs
│   │   │   │   └── ...
│   │   │   ├── Migrations/
│   │   │   └── Repositories/
│   │   ├── Services/
│   │   │   ├── JwtService.cs
│   │   │   ├── EmailService.cs
│   │   │   └── StorageService.cs
│   │   └── DependencyInjection.cs
│   │
│   └── NovaSaaSWebAPI/                  # Presentation Layer
│       ├── Controllers/
│       │   ├── AuthController.cs
│       │   ├── CustomersController.cs
│       │   └── ...
│       ├── Middlewares/
│       │   ├── TenantMiddleware.cs
│       │   └── ExceptionMiddleware.cs
│       ├── Filters/
│       ├── appsettings.json
│       └── Program.cs
│
├── tests/
│   ├── NovaSaaS.UnitTests/
│   └── NovaSaaS.IntegrationTests/
│
├── docs/
│   ├── ERP_MODULES_SPECIFICATION.md
│   ├── NOVASAAS_PITCH_DECK.md
│   └── DEV_GUIDE.md
│
├── docker-compose.yml
├── Dockerfile
└── README.md
```

### 4.2 Naming Conventions

| Item | Convention | Example |
|------|------------|---------|
| **Project** | PascalCase | `NovaSaaS.Domain` |
| **Folder** | PascalCase | `Entities`, `Services` |
| **Class** | PascalCase | `CustomerService` |
| **Interface** | I + PascalCase | `ICustomerService` |
| **Method** | PascalCase | `GetCustomerById` |
| **Property** | PascalCase | `FirstName` |
| **Variable** | camelCase | `customerName` |
| **Constant** | UPPER_SNAKE | `MAX_RETRY_COUNT` |
| **Private field** | _camelCase | `_customerRepository` |
| **DTO** | Name + Dto | `CustomerDto` |
| **Command** | Verb + Noun + Command | `CreateCustomerCommand` |
| **Query** | Get + Noun + Query | `GetCustomerByIdQuery` |

---

## 5. DATABASE SCHEMA

### 5.1 Core Tables (public schema)

```sql
-- Tenants table
CREATE TABLE public.tenants (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    subdomain VARCHAR(100) UNIQUE NOT NULL,
    schema_name VARCHAR(100) UNIQUE NOT NULL,
    status VARCHAR(50) DEFAULT 'Active',
    subscription_plan VARCHAR(50),
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP
);

-- Master modules
CREATE TABLE public.modules (
    id UUID PRIMARY KEY,
    code VARCHAR(10) NOT NULL,  -- CORE, CRM, INV, etc.
    name VARCHAR(100) NOT NULL,
    is_required BOOLEAN DEFAULT FALSE,
    price_monthly DECIMAL(18,2)
);
```

### 5.2 Tenant Tables (per-schema)

```sql
-- Users (trong mỗi tenant schema)
CREATE TABLE {tenant_schema}.users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    role_id UUID REFERENCES roles(id),
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT NOW(),
    created_by UUID,
    updated_at TIMESTAMP,
    updated_by UUID
);

-- Customers (CRM module)
CREATE TABLE {tenant_schema}.customers (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(50) UNIQUE,
    name VARCHAR(255) NOT NULL,
    email VARCHAR(255),
    phone VARCHAR(50),
    address TEXT,
    customer_group_id UUID,
    credit_limit DECIMAL(18,2) DEFAULT 0,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT NOW(),
    created_by UUID REFERENCES users(id)
);

-- Products (INV module)
CREATE TABLE {tenant_schema}.products (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    sku VARCHAR(100) UNIQUE NOT NULL,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    category_id UUID,
    unit_id UUID,
    cost_price DECIMAL(18,2),
    selling_price DECIMAL(18,2),
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT NOW()
);
```

### 5.3 Entity Relationship (Key Entities)

```
┌───────────────────────────────────────────────────────────────────────────┐
│                         KEY RELATIONSHIPS                                 │
├───────────────────────────────────────────────────────────────────────────┤
│                                                                           │
│   User ──────────────────┐                                                │
│     │                    │                                                │
│     │ 1:N                │ created_by                                     │
│     ▼                    │                                                │
│   Role ───────┐          │                                                │
│     │         │          │                                                │
│     │ N:M     │          │                                                │
│     ▼         │          │                                                │
│   Permission  │          │                                                │
│               │          │                                                │
│               │          │                                                │
│   Customer ◄──┼──────────┘                                                │
│     │                                                                     │
│     │ 1:N                                                                 │
│     ▼                                                                     │
│   Order ──────────────┬──────────────┐                                    │
│     │                 │              │                                    │
│     │ 1:N             │ 1:1          │ N:1                                │
│     ▼                 ▼              ▼                                    │
│   OrderItem       Invoice        Customer                                 │
│     │                 │                                                   │
│     │ N:1             │ 1:N                                               │
│     ▼                 ▼                                                   │
│   Product         Payment                                                 │
│     │                                                                     │
│     │ 1:N                                                                 │
│     ▼                                                                     │
│   Stock                                                                   │
│     │                                                                     │
│     │ N:1                                                                 │
│     ▼                                                                     │
│   Warehouse                                                               │
│                                                                           │
└───────────────────────────────────────────────────────────────────────────┘
```

---

## 6. API SPECIFICATION

### 6.1 API Structure

| Method | Pattern | Example |
|--------|---------|---------|
| GET list | `/api/{resource}` | `GET /api/customers` |
| GET one | `/api/{resource}/{id}` | `GET /api/customers/123` |
| POST | `/api/{resource}` | `POST /api/customers` |
| PUT | `/api/{resource}/{id}` | `PUT /api/customers/123` |
| DELETE | `/api/{resource}/{id}` | `DELETE /api/customers/123` |

### 6.2 Authentication

```
┌─────────────────────────────────────────────────────────────────┐
│                    AUTHENTICATION FLOW                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   1. Login                                                      │
│      POST /api/auth/login                                       │
│      Body: { "email": "...", "password": "..." }                │
│      Response: { "accessToken": "...", "refreshToken": "..." }  │
│                                                                 │
│   2. Attach token to requests                                   │
│      Header: Authorization: Bearer {accessToken}                │
│                                                                 │
│   3. Refresh token                                              │
│      POST /api/auth/refresh                                     │
│      Body: { "refreshToken": "..." }                            │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### 6.3 Standard Response Format

```json
// Success Response
{
    "success": true,
    "data": { ... },
    "message": "Operation successful"
}

// Error Response
{
    "success": false,
    "error": {
        "code": "VALIDATION_ERROR",
        "message": "Validation failed",
        "details": [
            { "field": "email", "message": "Email is required" }
        ]
    }
}

// Paginated Response
{
    "success": true,
    "data": {
        "items": [...],
        "pagination": {
            "page": 1,
            "pageSize": 10,
            "totalItems": 100,
            "totalPages": 10
        }
    }
}
```

### 6.4 Main Endpoints

| Module | Endpoint | Description |
|--------|----------|-------------|
| **Auth** | | |
| | `POST /api/auth/login` | Đăng nhập |
| | `POST /api/auth/register` | Đăng ký |
| | `POST /api/auth/refresh` | Refresh token |
| | `POST /api/auth/logout` | Đăng xuất |
| **Users** | | |
| | `GET /api/users` | Danh sách users |
| | `GET /api/users/{id}` | Chi tiết user |
| | `POST /api/users` | Tạo user |
| | `PUT /api/users/{id}` | Cập nhật user |
| **Customers** | | |
| | `GET /api/customers` | Danh sách KH |
| | `POST /api/customers` | Tạo KH |
| **Products** | | |
| | `GET /api/products` | Danh sách SP |
| | `POST /api/products` | Tạo SP |
| **Orders** | | |
| | `GET /api/orders` | Danh sách đơn |
| | `POST /api/orders` | Tạo đơn |

---

## 7. HƯỚNG DẪN SETUP

### 7.1 Prerequisites

```bash
# Kiểm tra các tool đã cài
dotnet --version    # >= 8.0
node --version      # >= 20.0
docker --version    # >= 24.0
git --version       # >= 2.40
```

### 7.2 Clone & Setup

```bash
# 1. Clone repository
git clone https://github.com/your-org/NovaSaaS.git
cd NovaSaaS

# 2. Start dependencies (PostgreSQL, Redis)
docker-compose up -d

# 3. Restore packages
dotnet restore

# 4. Update database
dotnet ef database update -p NovaSaaS.Infrastructure -s NovaSaaSWebAPI

# 5. Seed data (optional)
psql -h localhost -U postgres -d novasaas -f seed_data.sql

# 6. Run backend
cd NovaSaaSWebAPI
dotnet run

# 7. Run frontend (in another terminal)
cd frontend
npm install
npm run dev
```

### 7.3 Environment Variables

```bash
# .env hoặc appsettings.Development.json

# Database
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=novasaas;Username=postgres;Password=postgres

# JWT
Jwt__Secret=your-super-secret-key-at-least-32-characters
Jwt__Issuer=NovaSaaS
Jwt__Audience=NovaSaaS
Jwt__ExpiryMinutes=60

# Redis (optional)
Redis__ConnectionString=localhost:6379

# AI (optional)
OpenAI__ApiKey=sk-...
Gemini__ApiKey=...
```

### 7.4 Docker Compose

```yaml
# docker-compose.yml
version: '3.8'

services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: novasaas
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"

  api:
    build: .
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Host=postgres;...
    depends_on:
      - postgres
      - redis

volumes:
  postgres_data:
```

---

## 8. CODING STANDARDS

### 8.1 C# Guidelines

```csharp
// ✅ GOOD: Use async/await properly
public async Task<CustomerDto> GetCustomerByIdAsync(Guid id)
{
    var customer = await _repository.GetByIdAsync(id);
    if (customer == null)
        throw new NotFoundException(nameof(Customer), id);
    
    return _mapper.Map<CustomerDto>(customer);
}

// ❌ BAD: Blocking call
public CustomerDto GetCustomerById(Guid id)
{
    var customer = _repository.GetByIdAsync(id).Result; // Blocking!
    //...
}

// ✅ GOOD: Dependency Injection
public class CustomerService
{
    private readonly ICustomerRepository _repository;
    private readonly IMapper _mapper;
    
    public CustomerService(ICustomerRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }
}

// ❌ BAD: Creating dependencies manually
public class CustomerService
{
    private readonly CustomerRepository _repository = new CustomerRepository(); // Bad!
}
```

### 8.2 API Controller Template

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    
    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CustomerDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationQuery query)
    {
        var result = await _customerService.GetAllAsync(query);
        return Ok(ApiResponse.Success(result));
    }
    
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _customerService.GetByIdAsync(id);
        return Ok(ApiResponse.Success(result));
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(CustomerDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
    {
        var result = await _customerService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
```

### 8.3 Entity Template

```csharp
public class Customer : AuditableEntity
{
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string Phone { get; private set; }
    public Guid? CustomerGroupId { get; private set; }
    
    // Navigation properties
    public CustomerGroup? CustomerGroup { get; private set; }
    public ICollection<Order> Orders { get; private set; } = new List<Order>();
    
    // Private constructor for EF
    private Customer() { }
    
    // Factory method
    public static Customer Create(string name, string email, string phone)
    {
        return new Customer
        {
            Code = GenerateCode(),
            Name = name,
            Email = email,
            Phone = phone
        };
    }
    
    // Domain methods
    public void UpdateContactInfo(string email, string phone)
    {
        Email = email;
        Phone = phone;
    }
}
```

---

## 9. GIT WORKFLOW

### 9.1 Branch Strategy

```
main (production)
  │
  └── develop (staging)
        │
        ├── feature/CRM-001-customer-crud
        ├── feature/INV-002-stock-management
        ├── bugfix/CRM-003-fix-order-total
        └── hotfix/critical-security-fix
```

### 9.2 Branch Naming

| Type | Pattern | Example |
|------|---------|---------|
| Feature | `feature/{module}-{ticket}-{description}` | `feature/CRM-001-customer-crud` |
| Bugfix | `bugfix/{module}-{ticket}-{description}` | `bugfix/CRM-003-fix-order-total` |
| Hotfix | `hotfix/{description}` | `hotfix/security-patch` |
| Release | `release/v{version}` | `release/v1.0.0` |

### 9.3 Commit Message

```
<type>(<scope>): <subject>

<body>

<footer>

# Types: feat, fix, docs, style, refactor, test, chore
# Example:
feat(CRM): add customer CRUD operations

- Create CustomerController with CRUD endpoints
- Add CustomerService and repository
- Add validation for customer creation

Closes #123
```

### 9.4 Pull Request Process

```
1. Create feature branch from develop
2. Implement feature with tests
3. Push and create PR to develop
4. Code review by 1+ team member
5. CI/CD passes
6. Squash merge to develop
7. Deploy to staging for testing
8. Merge develop to main for production
```

---

## 10. TESTING GUIDE

### 10.1 Test Pyramid

```
           /\
          /  \      E2E Tests (5%)
         /    \     - Selenium/Playwright
        /──────\
       /        \   Integration Tests (15%)
      /          \  - API tests, DB tests
     /────────────\
    /              \ Unit Tests (80%)
   /                \ - Service tests, validation tests
  /──────────────────\
```

### 10.2 Unit Test Example

```csharp
public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CustomerService _sut;
    
    public CustomerServiceTests()
    {
        _repositoryMock = new Mock<ICustomerRepository>();
        _mapperMock = new Mock<IMapper>();
        _sut = new CustomerService(_repositoryMock.Object, _mapperMock.Object);
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenCustomerExists_ReturnsCustomerDto()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer { Id = customerId, Name = "Test" };
        var expectedDto = new CustomerDto { Id = customerId, Name = "Test" };
        
        _repositoryMock.Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync(customer);
        _mapperMock.Setup(x => x.Map<CustomerDto>(customer))
            .Returns(expectedDto);
        
        // Act
        var result = await _sut.GetByIdAsync(customerId);
        
        // Assert
        result.Should().BeEquivalentTo(expectedDto);
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenCustomerNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        _repositoryMock.Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync((Customer)null);
        
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.GetByIdAsync(customerId));
    }
}
```

### 10.3 Run Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific project
dotnet test NovaSaaS.UnitTests

# Run specific test
dotnet test --filter "FullyQualifiedName~CustomerServiceTests"
```

---

## 11. DEPLOYMENT GUIDE

### 11.1 Deployment Environments

| Environment | URL | Database | Purpose |
|-------------|-----|----------|---------|
| Local | localhost:5000 | Local PostgreSQL | Development |
| Staging | staging.novasaas.com | Staging DB | Testing |
| Production | app.novasaas.com | Production DB | Live |

### 11.2 CI/CD Pipeline (GitHub Actions)

```yaml
# .github/workflows/ci.yml
name: CI/CD

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [develop]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      
      - name: Restore
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore
      
      - name: Test
        run: dotnet test --no-build

  deploy-staging:
    needs: build
    if: github.ref == 'refs/heads/develop'
    runs-on: ubuntu-latest
    steps:
      - name: Deploy to staging
        run: |
          # Deploy script here

  deploy-production:
    needs: build
    if: github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    steps:
      - name: Deploy to production
        run: |
          # Deploy script here
```

### 11.3 Docker Deployment

```bash
# Build image
docker build -t novasaas-api:latest .

# Run container
docker run -d \
  --name novasaas-api \
  -p 5000:80 \
  -e ConnectionStrings__DefaultConnection="..." \
  -e Jwt__Secret="..." \
  novasaas-api:latest
```

---

## 12. TROUBLESHOOTING

### 12.1 Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| `Connection refused` | DB not running | `docker-compose up -d postgres` |
| `401 Unauthorized` | Token expired | Refresh token hoặc login lại |
| `Migration failed` | Schema conflict | `dotnet ef migrations remove`, sửa và tạo lại |
| `Port in use` | Conflict | Đổi port hoặc kill process |

### 12.2 Debug Tips

```csharp
// Enable detailed errors in Development
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Logging
_logger.LogInformation("Processing request for customer {CustomerId}", customerId);
_logger.LogError(ex, "Failed to create customer");
```

### 12.3 Useful Commands

```bash
# View logs
docker logs novasaas-api -f

# Access database
docker exec -it novasaas-postgres psql -U postgres -d novasaas

# Clear and reseed
dotnet ef database drop -f
dotnet ef database update
psql -f seed_data.sql

# Health check
curl http://localhost:5000/health
```

---

## 📞 LIÊN HỆ & HỖ TRỢ

| Kênh | Mục đích |
|------|----------|
| GitHub Issues | Bug reports, feature requests |
| Slack/Discord | Team communication |
| Wiki | Documentation updates |

---

*Tài liệu được tạo cho đội ngũ phát triển NovaSaaS - Cập nhật: 06/02/2026*
