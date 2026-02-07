# NOVASAAS ERP - ĐẶC TẢ MODULES VÀ CHỨC NĂNG

> **Phiên bản:** 1.0  
> **Ngày tạo:** 06/02/2026  
> **Mô tả:** Tài liệu mô tả chi tiết các module, chức năng và phân quyền theo actor

---

## 📋 MỤC LỤC

1. [Tổng quan hệ thống](#1-tổng-quan-hệ-thống)
2. [Danh sách Actors](#2-danh-sách-actors)
3. [Module ERP Core](#3-module-erp-core)
4. [Module HRM - Quản trị Nhân sự](#4-module-hrm---quản-trị-nhân-sự)
5. [Module CRM - Quan hệ Khách hàng](#5-module-crm---quan-hệ-khách-hàng)
6. [Module Inventory - Quản lý Kho](#6-module-inventory---quản-lý-kho)
7. [Module Accounting - Tài chính Kế toán](#7-module-accounting---tài-chính-kế-toán)
8. [Module SCM - Chuỗi Cung ứng](#8-module-scm---chuỗi-cung-ứng)
9. [Module PM - Quản lý Dự án](#9-module-pm---quản-lý-dự-án)
10. [Module DMS - Quản lý Tài liệu](#10-module-dms---quản-lý-tài-liệu)
11. [Ma trận tích hợp Module](#11-ma-trận-tích-hợp-module)
12. [Gói sản phẩm đề xuất](#12-gói-sản-phẩm-đề-xuất)

---

## 1. TỔNG QUAN HỆ THỐNG

### 1.1 Kiến trúc Module

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           MASTER ADMIN PORTAL                               │
│              (Quản lý toàn bộ hệ thống SaaS, Tenants, Billing)              │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
┌─────────────────────────────────────┴───────────────────────────────────────┐
│                           TENANT WORKSPACE                                  │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                      ERP CORE (Bắt buộc)                            │    │
│  │   Identity │ Settings │ Dashboard │ Notification │ AI Assistant    │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                    │                                        │
│  ┌──────────┬──────────┬──────────┬┴─────────┬──────────┬──────────┐        │
│  │   HRM    │   CRM    │   INV    │   ACC    │   SCM    │    PM    │        │
│  │ Nhân sự  │ Khách hàng│   Kho   │ Kế toán  │ Cung ứng │  Dự án   │        │
│  └────┬─────┴────┬─────┴────┬─────┴────┬─────┴────┬─────┴────┬─────┘        │
│       └──────────┴──────────┴──────────┴──────────┴──────────┘              │
│                                    │                                        │
│  ┌─────────────────────────────────┴───────────────────────────────────┐    │
│  │                     DMS - Document Management                       │    │
│  │            (Tích hợp vào tất cả modules khi được kích hoạt)         │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 1.2 Danh sách Modules

| Mã | Tên Module | Tên đầy đủ | Bắt buộc | Mô tả |
|----|-----------|-----------|----------|-------|
| CORE | ERP Core | Core Platform | ✅ Có | Nền tảng cơ bản, xác thực, phân quyền |
| HRM | Human Resource Management | Quản trị Nhân sự | ❌ Không | Quản lý nhân viên, chấm công, lương |
| CRM | Customer Relationship Management | Quan hệ Khách hàng | ❌ Không | Quản lý khách hàng, bán hàng |
| INV | Inventory Management | Quản lý Kho | ❌ Không | Quản lý tồn kho, xuất nhập |
| ACC | Accounting & Finance | Tài chính Kế toán | ❌ Không | Sổ sách, báo cáo tài chính |
| SCM | Supply Chain Management | Chuỗi Cung ứng | ❌ Không | Nhà cung cấp, mua hàng |
| PM | Project Management | Quản lý Dự án | ❌ Không | Dự án, task, tiến độ |
| DMS | Document Management System | Quản lý Tài liệu | ❌ Không | Lưu trữ, phân loại tài liệu |

---

## 2. DANH SÁCH ACTORS

### 2.1 Actors cấp Hệ thống (Master Level)

| Actor | Mô tả | Phạm vi |
|-------|-------|---------|
| **Master Admin** | Quản trị viên hệ thống SaaS | Toàn bộ hệ thống, tất cả tenants |
| **Support Staff** | Nhân viên hỗ trợ | Xem thông tin tenant, hỗ trợ kỹ thuật |

### 2.2 Actors cấp Tenant (Doanh nghiệp)

| Actor | Mô tả | Phạm vi |
|-------|-------|---------|
| **Tenant Admin** | Chủ doanh nghiệp / Quản trị viên | Toàn bộ tenant của mình |
| **Manager** | Quản lý | Quản lý phòng ban/module được giao |
| **HR Manager** | Quản lý Nhân sự | Module HRM |
| **Finance Manager** | Quản lý Tài chính | Module ACC |
| **Sales Manager** | Quản lý Bán hàng | Module CRM |
| **Warehouse Manager** | Quản lý Kho | Module INV |
| **Project Manager** | Quản lý Dự án | Module PM |
| **Accountant** | Kế toán viên | Bút toán, báo cáo |
| **HR Staff** | Nhân viên Nhân sự | Hỗ trợ quản lý nhân sự |
| **Sales Staff** | Nhân viên Bán hàng | Tạo đơn, chăm sóc khách |
| **Warehouse Staff** | Nhân viên Kho | Xuất nhập kho |
| **Employee** | Nhân viên | Xem thông tin cá nhân, chấm công |
| **Customer** | Khách hàng | Portal khách hàng (nếu có) |

---

## 3. MODULE ERP CORE

> **Trạng thái:** ✅ Bắt buộc cho mọi tenant  
> **Mô tả:** Nền tảng cơ bản bao gồm xác thực, phân quyền, cài đặt và dashboard

### 3.1 Sub-modules

#### 3.1.1 Identity & Access Management

| Chức năng | Mô tả | Master Admin | Tenant Admin | Manager | Staff | Employee |
|-----------|-------|:------------:|:------------:|:-------:|:-----:|:--------:|
| Đăng nhập/Đăng xuất | Xác thực người dùng | ✅ | ✅ | ✅ | ✅ | ✅ |
| Quản lý Profile | Xem/sửa thông tin cá nhân | ✅ | ✅ | ✅ | ✅ | ✅ |
| Đổi mật khẩu | Thay đổi mật khẩu | ✅ | ✅ | ✅ | ✅ | ✅ |
| Quản lý Users | CRUD người dùng | ❌ | ✅ | 👁️ | ❌ | ❌ |
| Quản lý Roles | CRUD vai trò | ❌ | ✅ | ❌ | ❌ | ❌ |
| Gán Permissions | Phân quyền cho roles | ❌ | ✅ | ❌ | ❌ | ❌ |
| Mời người dùng | Gửi invitation | ❌ | ✅ | ✅ | ❌ | ❌ |
| Xem Audit Logs | Lịch sử thao tác | ❌ | ✅ | 👁️ | ❌ | ❌ |

*Chú thích: ✅ Có quyền | ❌ Không có quyền | 👁️ Chỉ xem*

#### 3.1.2 Tenant Settings

| Chức năng | Mô tả | Tenant Admin | Manager |
|-----------|-------|:------------:|:-------:|
| Thông tin công ty | Logo, tên, địa chỉ, MST | ✅ | 👁️ |
| Cấu hình giao diện | Theme, màu sắc | ✅ | ❌ |
| Cài đặt thông báo | Email, push notification | ✅ | ✅ |
| Cấu hình khu vực | Ngôn ngữ, múi giờ, tiền tệ | ✅ | ❌ |
| Quản lý API Keys | Tạo/hủy API keys | ✅ | ❌ |
| Subscription | Xem gói, nâng cấp | ✅ | 👁️ |

#### 3.1.3 Dashboard

| Chức năng | Mô tả | Tenant Admin | Manager | Staff |
|-----------|-------|:------------:|:-------:|:-----:|
| Xem tổng quan | Widgets thống kê | ✅ Full | ✅ Theo module | 👁️ Hạn chế |
| Tùy chỉnh Dashboard | Sắp xếp widgets | ✅ | ✅ | ❌ |
| Quick Actions | Thao tác nhanh | ✅ | ✅ | ✅ |
| Alerts & Notifications | Cảnh báo hệ thống | ✅ | ✅ | ✅ |

#### 3.1.4 AI Assistant

| Chức năng | Mô tả | Tenant Admin | Manager | Staff |
|-----------|-------|:------------:|:-------:|:-----:|
| Chat AI cơ bản | Hỏi đáp thông tin | ✅ | ✅ | ✅ |
| Chat với RAG | Hỏi dựa trên tài liệu | ✅ | ✅ | ⚙️ |
| Function Calling | AI thực hiện truy vấn dữ liệu | ✅ | ✅ | ⚙️ |
| Upload tài liệu AI | Thêm knowledge base | ✅ | ✅ | ❌ |

*Chú thích: ⚙️ Tùy thuộc cấu hình*

#### 3.1.5 Notifications

| Chức năng | Mô tả | Tất cả Users |
|-----------|-------|:------------:|
| Xem thông báo | Danh sách notifications | ✅ |
| Đánh dấu đã đọc | Mark as read | ✅ |
| Cài đặt thông báo | Preferences | ✅ |
| Real-time alerts | Push notifications | ✅ |

---

## 4. MODULE HRM - QUẢN TRỊ NHÂN SỰ

> **Trạng thái:** Tùy chọn  
> **Phụ thuộc:** ERP Core  
> **Tích hợp:** Accounting (lương), PM (phân công), DMS (hồ sơ)

### 4.1 Entities (Bảng dữ liệu)

| Entity | Mô tả |
|--------|-------|
| Employee | Hồ sơ nhân viên (mở rộng từ User) |
| Department | Phòng ban |
| Position | Chức vụ |
| EmploymentContract | Hợp đồng lao động |
| Attendance | Chấm công |
| LeaveRequest | Đơn xin nghỉ phép |
| LeaveType | Loại nghỉ phép |
| Payroll | Bảng lương |
| PayrollItem | Chi tiết khoản lương |
| Bonus | Thưởng |
| Deduction | Khấu trừ |
| KPI | Chỉ tiêu đánh giá |
| PerformanceReview | Đánh giá hiệu suất |
| Training | Đào tạo |
| Recruitment | Tuyển dụng |
| Candidate | Ứng viên |

### 4.2 Chức năng theo Actor

#### 4.2.1 HR Manager

| Chức năng | Mô tả |
|-----------|-------|
| **Quản lý Cơ cấu tổ chức** | |
| CRUD Phòng ban | Tạo, sửa, xóa phòng ban |
| CRUD Chức vụ | Quản lý vị trí công việc |
| Sơ đồ tổ chức | Xem/in org chart |
| **Quản lý Nhân viên** | |
| CRUD Hồ sơ nhân viên | Thông tin đầy đủ |
| Quản lý Hợp đồng | Tạo, gia hạn, chấm dứt |
| Import/Export danh sách | Excel bulk operations |
| **Chấm công** | |
| Cấu hình ca làm việc | Shift management |
| Xem báo cáo chấm công | Attendance reports |
| Điều chỉnh chấm công | Sửa dữ liệu |
| **Nghỉ phép** | |
| Cấu hình loại nghỉ | Phép năm, ốm, không lương... |
| Duyệt đơn nghỉ | Approve/Reject |
| Báo cáo nghỉ phép | Leave reports |
| **Tính lương** | |
| Cấu hình công thức lương | Salary formulas |
| Tạo bảng lương | Generate payroll |
| Duyệt bảng lương | Approve payroll |
| Quản lý thưởng/phạt | Bonus/Deduction |
| **Đánh giá hiệu suất** | |
| Thiết lập KPI | KPI setup |
| Tạo đợt đánh giá | Performance review cycles |
| Xem kết quả đánh giá | Review results |
| **Tuyển dụng** | |
| Tạo tin tuyển dụng | Job postings |
| Quản lý ứng viên | Candidate pipeline |
| Lên lịch phỏng vấn | Interview scheduling |
| **Đào tạo** | |
| Tạo khóa đào tạo | Training programs |
| Đăng ký học viên | Enroll employees |
| Theo dõi tiến độ | Progress tracking |

#### 4.2.2 HR Staff

| Chức năng | Mô tả |
|-----------|-------|
| Xem hồ sơ nhân viên | Danh sách, tìm kiếm |
| Cập nhật thông tin cơ bản | Thông tin liên lạc |
| Xử lý đơn nghỉ phép | Nhận, forward cho manager |
| Nhập dữ liệu chấm công | Data entry |
| Hỗ trợ tuyển dụng | Lọc CV, liên hệ ứng viên |

#### 4.2.3 Manager (các phòng ban khác)

| Chức năng | Mô tả |
|-----------|-------|
| Xem nhân viên trong team | Danh sách team |
| Duyệt đơn nghỉ phép | Cấp 1 (trước HR) |
| Đánh giá hiệu suất | Đánh giá nhân viên dưới quyền |
| Xác nhận chấm công | Xác nhận OT của team |

#### 4.2.4 Employee (Nhân viên)

| Chức năng | Mô tả |
|-----------|-------|
| Xem thông tin cá nhân | Hồ sơ của mình |
| Chấm công | Check-in/Check-out |
| Xem lịch sử chấm công | Attendance history |
| Gửi đơn nghỉ phép | Submit leave request |
| Xem phiếu lương | Pay slip |
| Tự đánh giá | Self-assessment |
| Xem đào tạo | Tham gia training |

---

## 5. MODULE CRM - QUAN HỆ KHÁCH HÀNG

> **Trạng thái:** Tùy chọn  
> **Phụ thuộc:** ERP Core  
> **Tích hợp:** Inventory (tồn kho), Accounting (công nợ), SCM (đơn hàng)

### 5.1 Entities

| Entity | Mô tả |
|--------|-------|
| Customer | Khách hàng |
| CustomerGroup | Nhóm khách hàng (VIP, Regular...) |
| Contact | Người liên hệ |
| Lead | Khách hàng tiềm năng |
| Opportunity | Cơ hội bán hàng |
| Order | Đơn hàng bán |
| OrderItem | Chi tiết đơn hàng |
| Quotation | Báo giá |
| Invoice | Hóa đơn |
| Payment | Thanh toán |
| Coupon | Mã giảm giá |
| LoyaltyProgram | Chương trình tích điểm |
| LoyaltyTransaction | Giao dịch điểm thưởng |
| Campaign | Chiến dịch marketing |
| Activity | Hoạt động chăm sóc |

### 5.2 Chức năng theo Actor

#### 5.2.1 Sales Manager

| Chức năng | Mô tả |
|-----------|-------|
| **Quản lý Khách hàng** | |
| CRUD Khách hàng | Tạo, sửa, xóa khách hàng |
| Phân nhóm khách hàng | Customer segmentation |
| Thiết lập hạn mức tín dụng | Credit limit |
| Phân công Sales | Assign khách cho nhân viên |
| Import/Export | Bulk operations |
| **Pipeline Bán hàng** | |
| Quản lý Lead | Lead management |
| Quản lý Opportunity | Sales pipeline |
| Dự báo doanh thu | Sales forecast |
| **Đơn hàng & Hóa đơn** | |
| Xem tất cả đơn hàng | All orders |
| Duyệt đơn hàng lớn | Approve large orders |
| Quản lý báo giá | Quotation management |
| **Khuyến mãi** | |
| CRUD Coupon | Tạo mã giảm giá |
| Cấu hình Loyalty | Chương trình tích điểm |
| Tạo Campaign | Marketing campaigns |
| **Báo cáo** | |
| Báo cáo doanh thu | Revenue reports |
| Báo cáo khách hàng | Customer analytics |
| Hiệu suất Sales | Sales performance |
| Báo cáo pipeline | Pipeline reports |

#### 5.2.2 Sales Staff

| Chức năng | Mô tả |
|-----------|-------|
| **Khách hàng** | |
| Thêm khách hàng mới | Create customer |
| Cập nhật thông tin | Update customer info |
| Ghi nhận Activity | Log calls, emails, meetings |
| Xem lịch sử mua hàng | Purchase history |
| **Bán hàng** | |
| Nhập Lead | Add leads |
| Tạo Opportunity | Create opportunities |
| Tạo Báo giá | Create quotations |
| Tạo Đơn hàng | Create orders |
| Tạo Hóa đơn | Create invoices |
| In đơn/hóa đơn | Print documents |
| **Xử lý thanh toán** | |
| Nhận thanh toán | Receive payments |
| Áp dụng Coupon | Apply discounts |
| Cộng điểm Loyalty | Add loyalty points |

#### 5.2.3 Customer (Portal)

| Chức năng | Mô tả |
|-----------|-------|
| Xem tài khoản | Account info |
| Xem lịch sử đơn hàng | Order history |
| Xem hóa đơn | Invoice list |
| Theo dõi đơn hàng | Order tracking |
| Xem điểm tích lũy | Loyalty points |
| Đổi điểm thưởng | Redeem rewards |
| Gửi yêu cầu hỗ trợ | Support tickets |

---

## 6. MODULE INVENTORY - QUẢN LÝ KHO

> **Trạng thái:** Tùy chọn  
> **Phụ thuộc:** ERP Core  
> **Tích hợp:** CRM (bán hàng), SCM (mua hàng), Accounting (giá vốn)

### 6.1 Entities

| Entity | Mô tả |
|--------|-------|
| Product | Sản phẩm |
| ProductVariant | Biến thể (size, color...) |
| Category | Danh mục |
| Unit | Đơn vị tính |
| Warehouse | Kho hàng |
| Location | Vị trí trong kho |
| Stock | Tồn kho |
| StockMovement | Phiếu xuất/nhập |
| StockAdjustment | Điều chỉnh tồn kho |
| StockTransfer | Chuyển kho |
| InventoryCount | Kiểm kê |
| LotNumber | Số lô |
| SerialNumber | Số serial |

### 6.2 Chức năng theo Actor

#### 6.2.1 Warehouse Manager

| Chức năng | Mô tả |
|-----------|-------|
| **Sản phẩm** | |
| CRUD Sản phẩm | Quản lý catalog |
| CRUD Danh mục | Category tree |
| CRUD Đơn vị | Units of measure |
| Quản lý biến thể | Product variants |
| Cấu hình Lot/Serial | Tracking settings |
| **Kho hàng** | |
| CRUD Kho | Warehouse CRUD |
| Cấu hình vị trí | Location setup |
| Thiết lập kho mặc định | Default warehouse |
| **Tồn kho** | |
| Xem tồn kho tổng hợp | Stock overview |
| Thiết lập ngưỡng cảnh báo | Reorder levels |
| Điều chỉnh tồn kho | Stock adjustments |
| Duyệt phiếu xuất/nhập | Approve movements |
| Tạo lệnh kiểm kê | Inventory counting |
| Duyệt kết quả kiểm kê | Approve count results |
| **Chuyển kho** | |
| Tạo lệnh chuyển kho | Create transfers |
| Duyệt chuyển kho | Approve transfers |
| **Báo cáo** | |
| Báo cáo tồn kho | Stock reports |
| Báo cáo xuất/nhập | Movement reports |
| Báo cáo chênh lệch | Variance reports |
| ABC Analysis | Inventory analysis |

#### 6.2.2 Warehouse Staff

| Chức năng | Mô tả |
|-----------|-------|
| **Nhập kho** | |
| Nhận hàng | Receive goods |
| Tạo phiếu nhập | Create goods receipt |
| Đặt hàng vào vị trí | Put-away |
| **Xuất kho** | |
| Pick hàng | Pick goods |
| Đóng gói | Pack goods |
| Tạo phiếu xuất | Create goods issue |
| **Kiểm kê** | |
| Thực hiện đếm | Perform count |
| Ghi nhận chênh lệch | Report variances |
| **Chuyển kho** | |
| Thực hiện chuyển | Execute transfers |
| Xác nhận nhận hàng | Confirm receipt |
| **Tra cứu** | |
| Xem tồn kho | View stock |
| Tìm vị trí hàng | Locate items |

---

## 7. MODULE ACCOUNTING - TÀI CHÍNH KẾ TOÁN

> **Trạng thái:** Tùy chọn  
> **Phụ thuộc:** ERP Core  
> **Tích hợp:** CRM (doanh thu), INV (giá vốn), HRM (lương), SCM (chi phí mua)

### 7.1 Entities

| Entity | Mô tả |
|--------|-------|
| ChartOfAccount | Hệ thống tài khoản |
| AccountType | Loại tài khoản |
| JournalEntry | Bút toán |
| JournalLine | Chi tiết bút toán |
| FiscalYear | Năm tài chính |
| FiscalPeriod | Kỳ kế toán |
| Receivable | Công nợ phải thu |
| Payable | Công nợ phải trả |
| BankAccount | Tài khoản ngân hàng |
| BankTransaction | Giao dịch ngân hàng |
| BankReconciliation | Đối chiếu ngân hàng |
| Budget | Ngân sách |
| BudgetLine | Chi tiết ngân sách |
| TaxRate | Thuế suất |
| TaxTransaction | Giao dịch thuế |

### 7.2 Chức năng theo Actor

#### 7.2.1 Finance Manager

| Chức năng | Mô tả |
|-----------|-------|
| **Cấu hình** | |
| Thiết lập hệ thống tài khoản | Chart of accounts |
| Cấu hình năm/kỳ kế toán | Fiscal periods |
| Thiết lập thuế suất | Tax rates |
| Liên kết tài khoản ngân hàng | Bank accounts |
| **Bút toán** | |
| Duyệt bút toán | Approve entries |
| Đảo bút toán | Reverse entries |
| Khóa kỳ kế toán | Close periods |
| **Công nợ** | |
| Theo dõi công nợ phải thu | AR aging |
| Theo dõi công nợ phải trả | AP aging |
| Duyệt thanh toán | Approve payments |
| **Ngân sách** | |
| Tạo ngân sách | Create budgets |
| Theo dõi thực hiện | Budget vs Actual |
| **Báo cáo tài chính** | |
| Bảng cân đối kế toán | Balance sheet |
| Báo cáo kết quả kinh doanh | Income statement |
| Báo cáo lưu chuyển tiền tệ | Cash flow |
| Sổ cái | General ledger |
| **Thuế** | |
| Báo cáo thuế | Tax reports |
| Kê khai thuế | Tax filing |

#### 7.2.2 Accountant

| Chức năng | Mô tả |
|-----------|-------|
| **Bút toán** | |
| Tạo bút toán thủ công | Create entries |
| Import bút toán | Bulk import |
| Xem sổ sách | View ledgers |
| **Công nợ** | |
| Ghi nhận thanh toán | Record payments |
| Đối chiếu công nợ | Reconciliation |
| Gửi nhắc nợ | Send reminders |
| **Ngân hàng** | |
| Import sao kê | Import statements |
| Đối chiếu ngân hàng | Bank reconciliation |
| **Xử lý hàng ngày** | |
| Xác nhận doanh thu | Confirm revenue |
| Ghi nhận chi phí | Record expenses |
| Tính giá vốn | Cost calculation |

---

## 8. MODULE SCM - CHUỖI CUNG ỨNG

> **Trạng thái:** Tùy chọn  
> **Phụ thuộc:** ERP Core, Inventory  
> **Tích hợp:** Inventory (nhập kho), Accounting (công nợ NCC)

### 8.1 Entities

| Entity | Mô tả |
|--------|-------|
| Vendor | Nhà cung cấp |
| VendorContact | Liên hệ NCC |
| VendorPriceList | Bảng giá NCC |
| PurchaseRequisition | Yêu cầu mua hàng |
| PurchaseOrder | Đơn đặt hàng |
| PurchaseOrderLine | Chi tiết đơn mua |
| GoodsReceipt | Phiếu nhận hàng |
| PurchaseInvoice | Hóa đơn mua |
| VendorPayment | Thanh toán NCC |
| ReturnToVendor | Trả hàng NCC |

### 8.2 Chức năng theo Actor

#### 8.2.1 Purchase Manager

| Chức năng | Mô tả |
|-----------|-------|
| **Nhà cung cấp** | |
| CRUD Nhà cung cấp | Vendor management |
| Đánh giá NCC | Vendor rating |
| Quản lý bảng giá | Price lists |
| **Mua hàng** | |
| Duyệt yêu cầu mua | Approve PRs |
| Tạo đơn đặt hàng | Create POs |
| Duyệt đơn đặt hàng | Approve POs |
| So sánh giá | Price comparison |
| **Theo dõi** | |
| Theo dõi giao hàng | Delivery tracking |
| Xử lý trả hàng | Return handling |
| **Báo cáo** | |
| Phân tích mua hàng | Purchase analysis |
| Chi tiêu theo NCC | Spend by vendor |

#### 8.2.2 Purchase Staff

| Chức năng | Mô tả |
|-----------|-------|
| Tạo yêu cầu mua | Create requisitions |
| Tạo đơn đặt hàng | Create POs (nhỏ) |
| Liên hệ NCC | Contact vendors |
| Theo dõi đơn hàng | Track orders |

#### 8.2.3 Warehouse Staff (tích hợp từ INV)

| Chức năng | Mô tả |
|-----------|-------|
| Nhận hàng từ NCC | Receive goods |
| Tạo phiếu nhận | Goods receipt |
| Kiểm tra chất lượng | Quality check |
| Trả hàng lỗi | Process returns |

---

## 9. MODULE PM - QUẢN LÝ DỰ ÁN

> **Trạng thái:** Tùy chọn  
> **Phụ thuộc:** ERP Core  
> **Tích hợp:** HRM (nhân sự), Accounting (chi phí dự án)

### 9.1 Entities

| Entity | Mô tả |
|--------|-------|
| Project | Dự án |
| ProjectPhase | Giai đoạn |
| Task | Công việc |
| Subtask | Công việc con |
| Milestone | Cột mốc |
| ProjectMember | Thành viên dự án |
| Timesheet | Chấm công dự án |
| TimesheetEntry | Chi tiết timesheet |
| ProjectExpense | Chi phí dự án |
| ProjectDocument | Tài liệu dự án |
| ProjectComment | Bình luận |
| ProjectTag | Tag phân loại |

### 9.2 Chức năng theo Actor

#### 9.2.1 Project Manager / PMO

| Chức năng | Mô tả |
|-----------|-------|
| **Quản lý Dự án** | |
| CRUD Dự án | Tạo, sửa, xóa dự án |
| Thiết lập giai đoạn | Phase setup |
| Tạo Milestones | Milestone planning |
| Phân bổ ngân sách | Budget allocation |
| **Công việc** | |
| Tạo/phân công Tasks | Task assignment |
| Thiết lập dependencies | Task dependencies |
| Ước tính effort | Effort estimation |
| **Nhân sự dự án** | |
| Thêm thành viên | Add members |
| Phân vai trò | Assign roles |
| Xem allocation | Resource allocation |
| **Theo dõi** | |
| Gantt Chart | Timeline view |
| Kanban Board | Board view |
| Theo dõi tiến độ | Progress tracking |
| Duyệt Timesheet | Approve timesheets |
| **Báo cáo** | |
| Báo cáo tiến độ | Progress reports |
| Báo cáo chi phí | Cost reports |
| Báo cáo nhân sự | Resource reports |
| Burn-down chart | Agile reports |

#### 9.2.2 Team Member

| Chức năng | Mô tả |
|-----------|-------|
| Xem tasks được giao | My tasks |
| Cập nhật trạng thái | Update status |
| Log thời gian | Time logging |
| Thêm comments | Add comments |
| Đính kèm files | Attach files |
| Xem dự án | View project |

---

## 10. MODULE DMS - QUẢN LÝ TÀI LIỆU

> **Trạng thái:** Tùy chọn  
> **Phụ thuộc:** ERP Core  
> **Tích hợp:** Tất cả modules (đính kèm tài liệu)

### 10.1 Entities

| Entity | Mô tả |
|--------|-------|
| Folder | Thư mục |
| Document | Tài liệu |
| DocumentVersion | Phiên bản |
| DocumentShare | Chia sẻ |
| DocumentPermission | Phân quyền |
| DocumentTag | Tag |
| DocumentTemplate | Mẫu tài liệu |
| DocumentWorkflow | Workflow phê duyệt |
| WorkflowStep | Bước workflow |
| DocumentComment | Bình luận |
| DocumentCheckout | Lock file |

### 10.2 Chức năng theo Actor

#### 10.2.1 Admin / Document Manager

| Chức năng | Mô tả |
|-----------|-------|
| **Cấu trúc** | |
| CRUD Thư mục | Folder management |
| Thiết lập phân quyền | Permissions |
| Tạo mẫu tài liệu | Templates |
| **Workflow** | |
| Tạo workflow | Create workflows |
| Phân công reviewer | Assign reviewers |
| **Quản trị** | |
| Quản lý quota | Storage quota |
| Xem audit trail | Document history |
| Khôi phục file đã xóa | Recover deleted |

#### 10.2.2 Standard User

| Chức năng | Mô tả |
|-----------|-------|
| Upload tài liệu | Upload files |
| Tải xuống | Download |
| Xem online | Preview |
| Tạo version mới | Create version |
| Check-out/Check-in | Lock/Unlock |
| Chia sẻ | Share documents |
| Thêm tag | Add tags |
| Tìm kiếm | Search |
| Bình luận | Comment |

---

## 11. MA TRẬN TÍCH HỢP MODULE

> **Nguyên tắc:** Khi khách hàng mua nhiều modules, TẤT CẢ các modules đó sẽ tự động tích hợp với nhau. Dữ liệu chảy liền mạch, không cần cấu hình thêm.

---

### 11.1 SƠ ĐỒ TÍCH HỢP TỔNG QUAN

```
                                    ┌─────────────────┐
                                    │    ERP CORE     │
                                    │  (Bắt buộc)     │
                                    └────────┬────────┘
                                             │
        ┌────────────┬────────────┬──────────┼──────────┬────────────┬────────────┐
        │            │            │          │          │            │            │
        ▼            ▼            ▼          ▼          ▼            ▼            ▼
   ┌─────────┐  ┌─────────┐  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐
   │   HRM   │  │   CRM   │  │   INV   │ │   ACC   │ │   SCM   │ │   PM    │ │   DMS   │
   │ Nhân sự │  │ Bán hàng│  │   Kho   │ │ Kế toán │ │ Mua hàng│ │ Dự án   │ │Tài liệu │
   └────┬────┘  └────┬────┘  └────┬────┘ └────┬────┘ └────┬────┘ └────┬────┘ └────┬────┘
        │            │            │          │          │            │            │
        │            │            │          │          │            │            │
        └────────────┴──────┬─────┴──────────┴──────────┴────────────┘            │
                            │                                                      │
                            │  ◄──── Tích hợp 2 chiều giữa các modules ─────►      │
                            │                                                      │
                            └──────────────────────────────────────────────────────┘
                                                 ▲
                                                 │
                                    DMS tích hợp với TẤT CẢ modules
```

---

### 11.2 MA TRẬN TÍCH HỢP 2 MODULES

> ✅ = Có tích hợp | ❌ = Không liên quan trực tiếp

|         | **HRM** | **CRM** | **INV** | **ACC** | **SCM** | **PM** | **DMS** |
|---------|:-------:|:-------:|:-------:|:-------:|:-------:|:------:|:-------:|
| **HRM** | ─       | ⚪      | ⚪      | ✅      | ⚪      | ✅     | ✅      |
| **CRM** | ⚪      | ─       | ✅      | ✅      | ⚪      | ⚪     | ✅      |
| **INV** | ⚪      | ✅      | ─       | ✅      | ✅      | ⚪     | ✅      |
| **ACC** | ✅      | ✅      | ✅      | ─       | ✅      | ✅     | ✅      |
| **SCM** | ⚪      | ⚪      | ✅      | ✅      | ─       | ⚪     | ✅      |
| **PM**  | ✅      | ⚪      | ⚪      | ✅      | ⚪      | ─      | ✅      |
| **DMS** | ✅      | ✅      | ✅      | ✅      | ✅      | ✅     | ─       |

*⚪ = Không tích hợp trực tiếp, nhưng có thể liên kết qua module trung gian*

---

### 11.3 CHI TIẾT TÍCH HỢP TỪNG CẶP MODULE

#### 🔗 CRM ↔ INV (Bán hàng ↔ Kho)
| Chiều dữ liệu | Tích hợp |
|---------------|----------|
| CRM → INV | Đơn hàng tự động trừ tồn kho |
| CRM → INV | Đặt hàng kiểm tra tồn trước khi xác nhận |
| INV → CRM | Hiển thị tồn kho real-time khi bán |
| INV → CRM | Cảnh báo hết hàng khi tạo đơn |

#### 🔗 CRM ↔ ACC (Bán hàng ↔ Kế toán)
| Chiều dữ liệu | Tích hợp |
|---------------|----------|
| CRM → ACC | Hóa đơn bán → Bút toán doanh thu tự động |
| CRM → ACC | Thanh toán → Bút toán tiền mặt/ngân hàng |
| ACC → CRM | Công nợ phải thu → Hiển thị trên hồ sơ khách hàng |
| ACC → CRM | Hạn mức tín dụng → Chặn đặt hàng nếu vượt |

#### 🔗 INV ↔ ACC (Kho ↔ Kế toán)
| Chiều dữ liệu | Tích hợp |
|---------------|----------|
| INV → ACC | Nhập kho → Bút toán tăng tồn kho |
| INV → ACC | Xuất kho → Bút toán giá vốn hàng bán (COGS) |
| INV → ACC | Kiểm kê → Bút toán chênh lệch |
| ACC → INV | Định giá tồn kho → Báo cáo giá trị kho |

#### 🔗 INV ↔ SCM (Kho ↔ Mua hàng)
| Chiều dữ liệu | Tích hợp |
|---------------|----------|
| INV → SCM | Tồn kho thấp → Tự động tạo đề xuất mua hàng |
| SCM → INV | Nhận hàng từ NCC → Nhập kho tự động |
| SCM → INV | Trả hàng NCC → Xuất kho tự động |

#### 🔗 SCM ↔ ACC (Mua hàng ↔ Kế toán)
| Chiều dữ liệu | Tích hợp |
|---------------|----------|
| SCM → ACC | Hóa đơn mua → Bút toán chi phí |
| SCM → ACC | Thanh toán NCC → Bút toán tiền ra |
| ACC → SCM | Công nợ phải trả → Hiển thị trên NCC |

#### 🔗 HRM ↔ ACC (Nhân sự ↔ Kế toán)
| Chiều dữ liệu | Tích hợp |
|---------------|----------|
| HRM → ACC | Bảng lương → Bút toán chi phí lương |
| HRM → ACC | Thưởng/Phạt → Bút toán tương ứng |
| HRM → ACC | BHXH, thuế TNCN → Bút toán khấu trừ |

#### 🔗 HRM ↔ PM (Nhân sự ↔ Dự án)
| Chiều dữ liệu | Tích hợp |
|---------------|----------|
| HRM → PM | Danh sách nhân viên → Available resources |
| PM → HRM | Timesheet dự án → Tích hợp chấm công |
| PM → HRM | Phân bổ nhân sự → Xem tải công việc |

#### 🔗 PM ↔ ACC (Dự án ↔ Kế toán)
| Chiều dữ liệu | Tích hợp |
|---------------|----------|
| PM → ACC | Chi phí dự án → Bút toán chi phí |
| PM → ACC | Timesheet → Tính chi phí nhân công |
| ACC → PM | Ngân sách → Theo dõi Budget vs Actual |

---

### 11.4 TÍCH HỢP KHI MUA 3 MODULES

#### 📦 Combo: CRM + INV + ACC (Retail/Thương mại cơ bản)

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                         LUỒNG DỮ LIỆU HOÀN CHỈNH                              │
└──────────────────────────────────────────────────────────────────────────────┘

    Khách hàng                                                    Báo cáo
        │                                                            ▲
        ▼                                                            │
┌──────────────┐     Kiểm tra tồn     ┌──────────────┐              │
│     CRM      │ ──────────────────►  │     INV      │              │
│              │                      │              │              │
│  Tạo đơn    │     Trừ tồn kho      │  Cập nhật    │              │
│  Tạo invoice │ ◄────────────────── │  tồn kho     │              │
└──────┬───────┘                      └──────┬───────┘              │
       │                                     │                      │
       │ Doanh thu                           │ Giá vốn              │
       ▼                                     ▼                      │
┌──────────────────────────────────────────────────────────────┐    │
│                         ACCOUNTING                            │    │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────────┐   │    │
│  │ Doanh thu   │    │ Giá vốn     │    │ Lợi nhuận gộp   │───┼────┘
│  │ (Revenue)   │ ─  │ (COGS)      │ =  │ (Gross Profit)  │   │
│  └─────────────┘    └─────────────┘    └─────────────────┘   │
└──────────────────────────────────────────────────────────────┘
```

**Tính năng tích hợp khi có cả 3:**
| # | Tính năng | Mô tả |
|---|-----------|-------|
| 1 | Báo cáo Lợi nhuận theo đơn | Doanh thu - Giá vốn = Lợi nhuận từng đơn |
| 2 | Công nợ khách + Tồn kho | Dashboard tổng hợp |
| 3 | ABC Analysis + Margin | Sản phẩm nào lãi nhiều, bán chạy |
| 4 | Báo cáo tài chính tự động | Từ bán hàng → kho → kế toán |

---

#### 📦 Combo: INV + SCM + ACC (Chuỗi cung ứng hoàn chỉnh)

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                         CHUỖI CUNG ỨNG TÍCH HỢP                               │
└──────────────────────────────────────────────────────────────────────────────┘

┌────────────┐   Cảnh báo tồn thấp   ┌────────────┐   Đơn đặt hàng   ┌────────────┐
│    INV     │ ───────────────────►  │    SCM     │ ───────────────► │    NCC     │
│   (Kho)    │                       │ (Mua hàng) │                  │(Nhà CC)    │
└──────┬─────┘                       └──────┬─────┘                  └──────┬─────┘
       │                                    │                               │
       │ Nhập kho                           │ Hóa đơn mua                   │ Giao hàng
       │◄───────────────────────────────────┼───────────────────────────────┘
       │                                    │
       │                                    ▼
       │                            ┌──────────────┐
       │       Chi phí hàng         │     ACC      │
       └──────────────────────────► │  (Kế toán)   │
                                    │              │
              Công nợ NCC           │ ┌──────────┐ │
       ◄─────────────────────────── │ │ Payables │ │
                                    │ └──────────┘ │
                                    └──────────────┘
```

**Tính năng tích hợp khi có cả 3:**
| # | Tính năng | Mô tả |
|---|-----------|-------|
| 1 | Auto Reorder | Tồn thấp → Tự động tạo PO |
| 2 | Landed Cost | Tính đầy đủ chi phí nhập kho |
| 3 | Vendor Payables | Công nợ NCC tự động |
| 4 | Purchase Report | Chi tiêu, giá nhập, NCC tốt nhất |

---

#### 📦 Combo: HRM + PM + ACC (Dịch vụ / Tư vấn)

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                         QUẢN LÝ DỰ ÁN + NHÂN SỰ                               │
└──────────────────────────────────────────────────────────────────────────────┘

┌────────────┐    Nhân viên available    ┌────────────┐
│    HRM     │ ────────────────────────► │     PM     │
│ (Nhân sự)  │                           │  (Dự án)   │
│            │ ◄──────────────────────── │            │
└──────┬─────┘    Timesheet, Allocation  └──────┬─────┘
       │                                        │
       │ Lương                                  │ Chi phí dự án
       │                                        │
       ▼                                        ▼
┌──────────────────────────────────────────────────────────────┐
│                         ACCOUNTING                            │
│                                                              │
│  ┌────────────────┐         ┌────────────────────────────┐   │
│  │ Chi phí lương  │    +    │ Chi phí dự án (vật tư...) │   │
│  └────────────────┘         └────────────────────────────┘   │
│                    ↓                                         │
│           ┌────────────────────────┐                         │
│           │ Giá thành dự án        │                         │
│           │ (Project Cost)         │                         │
│           └────────────────────────┘                         │
└──────────────────────────────────────────────────────────────┘
```

**Tính năng tích hợp khi có cả 3:**
| # | Tính năng | Mô tả |
|---|-----------|-------|
| 1 | Project Costing | Chi phí nhân công = Timesheet × Hourly rate |
| 2 | Utilization Report | % thời gian billable của nhân viên |
| 3 | Budget Control | Cảnh báo vượt ngân sách real-time |
| 4 | Profitability | Lợi nhuận từng dự án |

---

### 11.5 TÍCH HỢP KHI MUA 4 MODULES

#### 📦 Combo: CRM + INV + SCM + ACC (Thương mại đầy đủ)

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                    VÒNG TUẦN HOÀN THƯƠNG MẠI ĐẦY ĐỦ                            │
└──────────────────────────────────────────────────────────────────────────────┘

                              ┌─────────────┐
                              │  KHÁCH HÀNG │
                              └──────┬──────┘
                                     │ Đặt hàng
                                     ▼
                              ┌─────────────┐
                        ┌────►│     CRM     │◄────┐
                        │     │  Bán hàng   │     │
                        │     └──────┬──────┘     │
                        │            │            │
            Công nợ KH  │            │ Xuất kho   │ Doanh thu
                        │            ▼            │
┌─────────────┐         │     ┌─────────────┐     │         ┌─────────────┐
│     ACC     │◄────────┴─────│     INV     │─────┴────────►│     ACC     │
│  (Kế toán)  │               │    (Kho)    │               │  (Kế toán)  │
└──────┬──────┘               └──────┬──────┘               └──────┬──────┘
       │                             │                             │
       │                             │ Tồn thấp                    │
       │ Thanh toán NCC              ▼                             │
       │                      ┌─────────────┐                      │
       └─────────────────────►│     SCM     │◄─────────────────────┘
                              │  Mua hàng   │      Chi phí mua
                              └──────┬──────┘
                                     │ Đặt hàng
                                     ▼
                              ┌─────────────┐
                              │     NCC     │
                              └─────────────┘
```

**Tính năng tích hợp khi có cả 4:**
| # | Tính năng | Mô tả |
|---|-----------|-------|
| 1 | End-to-End Tracking | Từ mua → kho → bán → kế toán |
| 2 | Margin Analysis | Lợi nhuận = Bán - Mua - Chi phí |
| 3 | Cash Flow Forecast | Dự báo dòng tiền từ AR & AP |
| 4 | Supplier vs Customer Balance | So sánh công nợ 2 chiều |
| 5 | Auto Purchase Suggestion | Dựa trên sales trend + tồn kho |

---

#### 📦 Combo: CRM + INV + HRM + ACC (Retail + Nhân sự)

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                      BÁN LẺ VỚI QUẢN LÝ NHÂN VIÊN                              │
└──────────────────────────────────────────────────────────────────────────────┘

┌─────────────┐         ┌─────────────┐         ┌─────────────┐
│     CRM     │◄───────►│     INV     │         │     HRM     │
│  Bán hàng   │         │    Kho      │         │  Nhân sự    │
└──────┬──────┘         └──────┬──────┘         └──────┬──────┘
       │                       │                       │
       │ Doanh thu             │ Giá vốn               │ Lương
       │ theo NV               │                       │
       ▼                       ▼                       ▼
┌──────────────────────────────────────────────────────────────┐
│                         ACCOUNTING                            │
│  ┌──────────┐   ┌──────────┐   ┌──────────┐   ┌──────────┐   │
│  │ Doanh thu│ - │ Giá vốn  │ - │ Chi phí  │ = │ Lợi nhuận│   │
│  │          │   │ (COGS)   │   │   lương  │   │   ròng   │   │
│  └──────────┘   └──────────┘   └──────────┘   └──────────┘   │
└──────────────────────────────────────────────────────────────┘
```

**Tính năng tích hợp khi có cả 4:**
| # | Tính năng | Mô tả |
|---|-----------|-------|
| 1 | Sales per Employee | Doanh thu từng nhân viên bán |
| 2 | Commission Calculation | Hoa hồng tự động từ doanh số |
| 3 | Full P&L | Doanh thu - Giá vốn - Lương = Lãi ròng |
| 4 | Staff Performance | Hiệu suất NV dựa trên sales |

---

### 11.6 TÍCH HỢP KHI MUA 5+ MODULES

#### 📦 Combo: CRM + INV + SCM + HRM + ACC (Doanh nghiệp vừa)

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                         MÔ HÌNH DOANH NGHIỆP ĐẦY ĐỦ                            │
└──────────────────────────────────────────────────────────────────────────────┘

                         ┌────────────────────────────┐
                         │       KHÁCH HÀNG           │
                         └─────────────┬──────────────┘
                                       │
                                       ▼
    ┌──────────────────────────────────────────────────────────────────────────┐
    │                              CRM                                         │
    │                         Bán hàng, Chăm sóc                               │
    └───────────────────────────────┬──────────────────────────────────────────┘
                                    │
                   ┌────────────────┼────────────────┐
                   ▼                ▼                ▼
    ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐
    │       INV        │  │       HRM        │  │       ACC        │
    │      (Kho)       │  │    (Nhân sự)     │  │    (Kế toán)     │
    │                  │  │                  │  │                  │
    │ • Xuất hàng      │  │ • NV bán hàng    │  │ • Doanh thu      │
    │ • Cập nhật tồn   │  │ • Hoa hồng       │  │ • Công nợ        │
    └────────┬─────────┘  └────────┬─────────┘  └────────┬─────────┘
             │                     │                     │
             │                     │                     │
             ▼                     │                     │
    ┌──────────────────┐           │                     │
    │       SCM        │           │                     │
    │    (Mua hàng)    │◄──────────┼─────────────────────┘
    │                  │           │
    │ • Đặt hàng NCC   │           │ Lương + BHXH
    │ • Nhận hàng      │           ▼
    └────────┬─────────┘  ┌──────────────────┐
             │            │       ACC        │
             │            │   (Chi phí)      │
             │            └──────────────────┘
             │
             ▼
    ┌──────────────────────────────────────────────────────────────────────────┐
    │                            NHÀ CUNG CẤP                                  │
    └──────────────────────────────────────────────────────────────────────────┘
```

**Báo cáo tích hợp khi có 5 modules:**
| Báo cáo | Nguồn dữ liệu |
|---------|---------------|
| **Báo cáo P&L đầy đủ** | CRM (Doanh thu) - INV (Giá vốn) - HRM (Lương) - SCM (Chi phí mua) |
| **Cash Flow Statement** | ACC (từ tất cả nguồn) |
| **Employee Productivity** | HRM + CRM (Sales per Employee) |
| **Vendor Performance** | SCM + INV (Chất lượng, giao hàng) |
| **Inventory Turnover** | INV + CRM + SCM (Vòng quay hàng tồn) |

---

#### 📦 Combo: Full ERP (Tất cả 7 modules)

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                        ENTERPRISE RESOURCE PLANNING                           │
│                           (Mô hình đầy đủ)                                    │
└──────────────────────────────────────────────────────────────────────────────┘

                              ┌─────────────────┐
                              │   ERP CORE      │
                              │   Dashboard     │
                              │   AI Assistant  │
                              └────────┬────────┘
                                       │
       ┌───────────┬───────────┬───────┴───────┬───────────┬───────────┐
       │           │           │               │           │           │
       ▼           ▼           ▼               ▼           ▼           ▼
┌───────────┐ ┌───────────┐ ┌───────────┐ ┌───────────┐ ┌───────────┐ ┌───────────┐
│    HRM    │ │    CRM    │ │    INV    │ │    SCM    │ │    PM     │ │    DMS    │
│  Nhân sự  │ │  Bán hàng │ │    Kho    │ │  Mua hàng │ │   Dự án   │ │  Tài liệu │
└─────┬─────┘ └─────┬─────┘ └─────┬─────┘ └─────┬─────┘ └─────┬─────┘ └─────┬─────┘
      │             │             │             │             │             │
      │   ┌─────────┴─────────────┴─────────────┴─────────────┘             │
      │   │                                                                 │
      │   │     ┌─────────────────────────────────────────────────┐         │
      │   │     │                 ACCOUNTING                       │         │
      │   └────►│                                                 │◄────────┘
      │         │  • Doanh thu (CRM)        • Giá vốn (INV)       │
      └────────►│  • Chi phí mua (SCM)      • Chi phí lương (HRM) │
                │  • Chi phí dự án (PM)     • Tài liệu (DMS)      │
                │                                                 │
                │     ══════════════════════════════════════      │
                │     Báo cáo Tài chính Tổng hợp (Full P&L)       │
                │     Balance Sheet, Cash Flow, Budget Report     │
                └─────────────────────────────────────────────────┘
```

---

### 11.7 BẢNG TỔNG HỢP TÍCH HỢP THEO GÓI

| Gói | Modules | Tích hợp chính | Use Case |
|-----|---------|----------------|----------|
| **2 modules** | CRM + INV | Bán hàng ↔ Kho | Cửa hàng nhỏ |
| **3 modules** | CRM + INV + ACC | Bán ↔ Kho ↔ Kế toán | Shop có sổ sách |
| **3 modules** | INV + SCM + ACC | Kho ↔ Mua ↔ Kế toán | Nhập khẩu, phân phối |
| **3 modules** | HRM + PM + ACC | Nhân sự ↔ Dự án ↔ Kế toán | Công ty dịch vụ |
| **4 modules** | CRM + INV + SCM + ACC | Mua ↔ Kho ↔ Bán ↔ Kế toán | Thương mại |
| **5 modules** | CRM + INV + SCM + HRM + ACC | Full supply chain + HR | DN vừa |
| **6 modules** | + PM | Thêm quản lý dự án | DN có dự án |
| **7 modules** | + DMS | Thêm quản lý tài liệu | Enterprise |

---

### 11.8 SƠ ĐỒ LUỒNG DỮ LIỆU ĐẦY ĐỦ

```
┌───────────────────────────────────────────────────────────────────────────────────────────┐
│                              LUỒNG DỮ LIỆU ERP TOÀN DIỆN                                  │
├───────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                           │
│    NGUỒN VÀO                     XỬ LÝ                           KẾT QUẢ                  │
│    ─────────                     ────                            ──────                   │
│                                                                                           │
│    ┌─────────┐                                                                            │
│    │ Ứng viên│───► HRM ───┬──► Nhân viên ───► PM (Phân công)                             │
│    └─────────┘            │                                                               │
│                           └──► Lương ───────────────────────────────────┐                 │
│                                                                         │                 │
│    ┌─────────┐                                                          │                 │
│    │   NCC   │───► SCM ───┬──► Đơn mua ───► INV (Nhập kho) ───┐         │                 │
│    └─────────┘            │                                    │         │                 │
│                           └──► Hóa đơn mua ───────────────────┼─────────┼──┐              │
│                                                               │         │  │              │
│    ┌─────────┐                                                │         │  │              │
│    │ Sản phẩm│───► INV ───┬──► Tồn kho ───► CRM (Xem tồn)     │         │  │              │
│    └─────────┘            │                                    │         │  │              │
│                           └──► Giá vốn ───────────────────────┼─────────┼──┼──┐           │
│                                                               │         │  │  │           │
│    ┌─────────┐                                                │         │  │  │           │
│    │  Khách  │───► CRM ───┬──► Đơn hàng ───► INV (Xuất kho)   │         │  │  │           │
│    └─────────┘            │                      │            │         │  │  │           │
│                           └──► Hóa đơn bán ──────┼────────────┼─────────┼──┼──┼──┐        │
│                                                  │            │         │  │  │  │        │
│    ┌─────────┐                                   │            │         │  │  │  │        │
│    │ Dự án   │───► PM ────┬──► Timesheet ────────┼────────────┼──► HRM  │  │  │  │        │
│    └─────────┘            │                      │            │         │  │  │  │        │
│                           └──► Chi phí DA ───────┼────────────┼─────────┼──┼──┼──┼──┐     │
│                                                  │            │         │  │  │  │  │     │
│    ┌─────────┐                                   │            │         │  │  │  │  │     │
│    │Tài liệu │───► DMS ───────► Đính kèm ────────┼────────────┼─────────┼──┼──┼──┼──┤     │
│    └─────────┘                  (Tất cả)         │            │         │  │  │  │  │     │
│                                                  │            │         │  │  │  │  │     │
│                                                  ▼            ▼         ▼  ▼  ▼  ▼  ▼     │
│                                        ┌─────────────────────────────────────────────┐    │
│                                        │              ACCOUNTING                     │    │
│                                        │                                             │    │
│                                        │   Doanh thu ─────────────┐                  │    │
│                                        │   Chi phí mua ───────────┤                  │    │
│                                        │   Chi phí lương ─────────┼─► BÁO CÁO       │    │
│                                        │   Chi phí dự án ─────────┤   TÀI CHÍNH     │    │
│                                        │   Giá vốn ───────────────┤   TỔNG HỢP      │    │
│                                        │   Công nợ ───────────────┘                  │    │
│                                        │                                             │    │
│                                        └─────────────────────────────────────────────┘    │
│                                                           │                               │
│                                                           ▼                               │
│                                                  ┌─────────────────┐                      │
│                                                  │   DASHBOARD     │                      │
│                                                  │   + AI Chat     │                      │
│                                                  └─────────────────┘                      │
│                                                                                           │
└───────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 12. GÓI SẢN PHẨM ĐỀ XUẤT

> **Chiến lược:** Cung cấp nhiều lựa chọn linh hoạt để phù hợp với mọi quy mô và ngành nghề của khách hàng.

---

### 12.1 MÔ HÌNH ĐỊNH GIÁ

#### 12.1.1 Các mô hình định giá khả dụng

| Mô hình | Mô tả | Ưu điểm | Nhược điểm |
|---------|-------|---------|------------|
| **Per-User** | Tính phí theo số user | Đơn giản, dễ hiểu | Giới hạn user = giới hạn tăng trưởng |
| **Per-Module** | Tính phí theo module mua | Linh hoạt, khách chọn được | Phức tạp khi tính toán |
| **Tiered** | Gói cố định theo cấp | Rõ ràng, dễ bán | Ít linh hoạt |
| **Usage-Based** | Theo lượng sử dụng | Công bằng | Khó dự đoán chi phí |
| **Hybrid** | Kết hợp nhiều mô hình | Tối ưu | Cần giải thích kỹ |

#### 12.1.2 Đề xuất: Mô hình Hybrid (Gói + Module + User)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         CÔNG THỨC TÍNH GIÁ                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│   Phí hàng tháng = [Phí gói cơ bản] + [Phí module bổ sung] + [Phí user]    │
│                                                                             │
│   Ví dụ:                                                                    │
│   ┌─────────────────────────────────────────────────────────────────────┐   │
│   │  Business Package (Core + CRM + INV + ACC)     = 2,000,000 VNĐ      │   │
│   │  + Module HRM (bổ sung)                        =   800,000 VNĐ      │   │
│   │  + 15 users (5 free + 10 × 100,000)            = 1,000,000 VNĐ      │   │
│   │  ─────────────────────────────────────────────────────────────      │   │
│   │  TỔNG CỘNG                                     = 3,800,000 VNĐ/tháng│   │
│   └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

### 12.2 GÓI SẢN PHẨM THEO QUY MÔ

#### 📦 STARTER - Khởi nghiệp

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              💡 STARTER                                     │
│                        "Khởi đầu kinh doanh số"                             │
├─────────────────────────────────────────────────────────────────────────────┤
│  Giá: 500,000 - 1,000,000 VNĐ/tháng                                        │
│  Users: 1-5 users (bao gồm)                                                 │
│  Extra user: 50,000 VNĐ/user/tháng                                         │
├─────────────────────────────────────────────────────────────────────────────┤
│  MODULES BAO GỒM:                                                           │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐                                      │
│  │  CORE   │  │   CRM   │  │   INV   │                                      │
│  │    ✓    │  │    ✓    │  │    ✓    │                                      │
│  └─────────┘  └─────────┘  └─────────┘                                      │
├─────────────────────────────────────────────────────────────────────────────┤
│  TÍNH NĂNG:                                                                 │
│  ✅ Quản lý khách hàng (500 khách)           ✅ Quản lý tồn kho (1,000 SP)  │
│  ✅ Tạo đơn hàng & hóa đơn                   ✅ Báo cáo cơ bản              │
│  ✅ 1 kho hàng                               ✅ AI Chat cơ bản              │
│  ✅ Dashboard real-time                      ✅ Hỗ trợ email                │
│                                                                             │
│  ❌ Kế toán        ❌ Nhân sự       ❌ Mua hàng       ❌ Dự án              │
├─────────────────────────────────────────────────────────────────────────────┤
│  ĐỐI TƯỢNG: Cửa hàng nhỏ, startup, freelancer bán online                   │
└─────────────────────────────────────────────────────────────────────────────┘
```

#### 📦 BUSINESS - Doanh nghiệp nhỏ

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              💼 BUSINESS                                    │
│                        "Quản lý chuyên nghiệp"                              │
├─────────────────────────────────────────────────────────────────────────────┤
│  Giá: 2,000,000 - 3,500,000 VNĐ/tháng                                      │
│  Users: 1-10 users (bao gồm)                                                │
│  Extra user: 80,000 VNĐ/user/tháng                                         │
├─────────────────────────────────────────────────────────────────────────────┤
│  MODULES BAO GỒM:                                                           │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐                         │
│  │  CORE   │  │   CRM   │  │   INV   │  │   ACC   │                         │
│  │    ✓    │  │    ✓    │  │    ✓    │  │    ✓    │                         │
│  └─────────┘  └─────────┘  └─────────┘  └─────────┘                         │
├─────────────────────────────────────────────────────────────────────────────┤
│  TÍNH NĂNG:                                                                 │
│  ✅ Quản lý khách hàng (2,000 khách)         ✅ Quản lý tồn kho (5,000 SP)  │
│  ✅ Hệ thống tài khoản kế toán               ✅ Báo cáo tài chính cơ bản   │
│  ✅ Công nợ phải thu/phải trả                ✅ 3 kho hàng                  │
│  ✅ AI Chat + RAG                            ✅ Hỗ trợ chat + email         │
│  ✅ Export Excel/PDF                         ✅ API access (giới hạn)       │
│                                                                             │
│  ❌ Nhân sự        ❌ Mua hàng       ❌ Dự án       ❌ Tài liệu              │
├─────────────────────────────────────────────────────────────────────────────┤
│  ĐỐI TƯỢNG: Shop nhiều nhân viên, SME cần sổ sách, chuỗi cửa hàng nhỏ      │
└─────────────────────────────────────────────────────────────────────────────┘
```

#### 📦 PROFESSIONAL - Doanh nghiệp vừa

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              🏢 PROFESSIONAL                                │
│                        "Giải pháp toàn diện"                                │
├─────────────────────────────────────────────────────────────────────────────┤
│  Giá: 5,000,000 - 10,000,000 VNĐ/tháng                                     │
│  Users: 1-30 users (bao gồm)                                                │
│  Extra user: 100,000 VNĐ/user/tháng                                        │
├─────────────────────────────────────────────────────────────────────────────┤
│  MODULES BAO GỒM:                                                           │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐│
│  │  CORE   │  │   CRM   │  │   INV   │  │   ACC   │  │   SCM   │  │   HRM   ││
│  │    ✓    │  │    ✓    │  │    ✓    │  │    ✓    │  │    ✓    │  │    ✓    ││
│  └─────────┘  └─────────┘  └─────────┘  └─────────┘  └─────────┘  └─────────┘│
├─────────────────────────────────────────────────────────────────────────────┤
│  TÍNH NĂNG:                                                                 │
│  ✅ Khách hàng không giới hạn                ✅ Sản phẩm không giới hạn    │
│  ✅ Quản lý nhà cung cấp                     ✅ Đơn mua hàng tự động       │
│  ✅ Quản lý nhân sự, chấm công               ✅ Tính lương cơ bản          │
│  ✅ Báo cáo tài chính đầy đủ                 ✅ 10 kho hàng                │
│  ✅ AI Chat + Functions                       ✅ Hỗ trợ ưu tiên            │
│  ✅ API full access                          ✅ Webhook integration        │
│                                                                             │
│  ❌ Dự án          ❌ Tài liệu                                              │
├─────────────────────────────────────────────────────────────────────────────┤
│  ĐỐI TƯỢNG: Công ty thương mại, phân phối, chuỗi cửa hàng vừa              │
└─────────────────────────────────────────────────────────────────────────────┘
```

#### 📦 ENTERPRISE - Doanh nghiệp lớn

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              🏛️ ENTERPRISE                                 │
│                        "Nền tảng doanh nghiệp"                              │
├─────────────────────────────────────────────────────────────────────────────┤
│  Giá: Thương lượng (từ 20,000,000 VNĐ/tháng)                               │
│  Users: Không giới hạn                                                      │
│  SLA: 99.9% uptime                                                          │
├─────────────────────────────────────────────────────────────────────────────┤
│  MODULES BAO GỒM: TẤT CẢ                                                    │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐                         │
│  │  CORE   │  │   CRM   │  │   INV   │  │   ACC   │                         │
│  │    ✓    │  │    ✓    │  │    ✓    │  │    ✓    │                         │
│  └─────────┘  └─────────┘  └─────────┘  └─────────┘                         │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐                         │
│  │   SCM   │  │   HRM   │  │   PM    │  │   DMS   │                         │
│  │    ✓    │  │    ✓    │  │    ✓    │  │    ✓    │                         │
│  └─────────┘  └─────────┘  └─────────┘  └─────────┘                         │
├─────────────────────────────────────────────────────────────────────────────┤
│  TÍNH NĂNG ĐẶC BIỆT:                                                        │
│  ✅ Mọi tính năng của Professional            ✅ Quản lý dự án đầy đủ      │
│  ✅ Quản lý tài liệu với workflow             ✅ Custom domain             │
│  ✅ White-label (logo, màu sắc riêng)         ✅ Dedicated support         │
│  ✅ On-premise option                         ✅ Data export full          │
│  ✅ Custom integrations                       ✅ Training & onboarding     │
│  ✅ Priority feature requests                 ✅ Account manager           │
├─────────────────────────────────────────────────────────────────────────────┤
│  ĐỐI TƯỢNG: Tập đoàn, công ty đa chi nhánh, doanh nghiệp 100+ nhân viên    │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

### 12.3 BẢNG SO SÁNH TÍNH NĂNG

| Tính năng | Starter | Business | Professional | Enterprise |
|-----------|:-------:|:--------:|:------------:|:----------:|
| **MODULES** |  |  |  |  |
| ERP Core | ✅ | ✅ | ✅ | ✅ |
| CRM - Bán hàng | ✅ | ✅ | ✅ | ✅ |
| INV - Kho | ✅ | ✅ | ✅ | ✅ |
| ACC - Kế toán | ❌ | ✅ | ✅ | ✅ |
| SCM - Mua hàng | ❌ | ❌ | ✅ | ✅ |
| HRM - Nhân sự | ❌ | ❌ | ✅ | ✅ |
| PM - Dự án | ❌ | ❌ | ❌ | ✅ |
| DMS - Tài liệu | ❌ | ❌ | ❌ | ✅ |
| **GIỚI HẠN** |  |  |  |  |
| Users bao gồm | 5 | 10 | 30 | Unlimited |
| Khách hàng | 500 | 2,000 | Unlimited | Unlimited |
| Sản phẩm | 1,000 | 5,000 | Unlimited | Unlimited |
| Kho hàng | 1 | 3 | 10 | Unlimited |
| Storage | 5 GB | 20 GB | 100 GB | Unlimited |
| **TÍNH NĂNG** |  |  |  |  |
| Dashboard | Basic | Full | Full | Custom |
| Báo cáo | Basic | Standard | Advanced | Custom |
| AI Chat | Basic | + RAG | + Functions | Custom AI |
| API Access | ❌ | Limited | Full | Full + SDKs |
| Webhooks | ❌ | ❌ | ✅ | ✅ |
| Custom fields | ❌ | Limited | ✅ | ✅ |
| Multi-currency | ❌ | ❌ | ✅ | ✅ |
| Multi-language | ❌ | ❌ | ✅ | ✅ |
| **HỖ TRỢ** |  |  |  |  |
| Email support | ✅ | ✅ | ✅ | ✅ |
| Chat support | ❌ | ✅ | ✅ | ✅ |
| Phone support | ❌ | ❌ | ✅ | ✅ |
| Dedicated support | ❌ | ❌ | ❌ | ✅ |
| Response time | 48h | 24h | 4h | 1h |
| Training | Self-serve | Videos | Live sessions | On-site |
| **GIÁ** | 500K-1M | 2M-3.5M | 5M-10M | 20M+ |

---

### 12.4 GÓI SẢN PHẨM THEO NGÀNH

#### 🛒 RETAIL - Bán lẻ

```
┌──────────────────────────────────────────────────────────────────┐
│                         🛒 RETAIL PACKAGE                        │
│                      "Giải pháp bán lẻ thông minh"               │
├──────────────────────────────────────────────────────────────────┤
│  Modules: CORE + CRM + INV + (ACC optional)                      │
│  Giá từ: 1,500,000 VNĐ/tháng                                     │
├──────────────────────────────────────────────────────────────────┤
│  TÍNH NĂNG ĐẶC BIỆT:                                             │
│  • POS-friendly interface (giao diện bán hàng nhanh)             │
│  • Barcode/QR scanning                                           │
│  • Customer loyalty program                                      │
│  • Quick checkout                                                │
│  • Sales analytics                                               │
│  • Low stock alerts                                              │
├──────────────────────────────────────────────────────────────────┤
│  PHÙ HỢP: Cửa hàng quần áo, mỹ phẩm, điện tử, tiện lợi          │
└──────────────────────────────────────────────────────────────────┘
```

#### 🔧 SERVICE - Dịch vụ

```
┌──────────────────────────────────────────────────────────────────┐
│                        🔧 SERVICE PACKAGE                        │
│                     "Quản lý dịch vụ chuyên nghiệp"              │
├──────────────────────────────────────────────────────────────────┤
│  Modules: CORE + CRM + PM + HRM + (ACC optional)                 │
│  Giá từ: 3,000,000 VNĐ/tháng                                     │
├──────────────────────────────────────────────────────────────────┤
│  TÍNH NĂNG ĐẶC BIỆT:                                             │
│  • Appointment booking (đặt lịch hẹn)                            │
│  • Service tickets management                                    │
│  • Project tracking                                              │
│  • Timesheet & billing                                           │
│  • Staff scheduling                                              │
│  • Customer feedback                                             │
├──────────────────────────────────────────────────────────────────┤
│  PHÙ HỢP: Spa, salon, sửa chữa, tư vấn, IT services, agency     │
└──────────────────────────────────────────────────────────────────┘
```

#### 📦 TRADING - Thương mại

```
┌──────────────────────────────────────────────────────────────────┐
│                       📦 TRADING PACKAGE                         │
│                    "Thương mại - Phân phối"                      │
├──────────────────────────────────────────────────────────────────┤
│  Modules: CORE + CRM + INV + SCM + ACC                           │
│  Giá từ: 5,000,000 VNĐ/tháng                                     │
├──────────────────────────────────────────────────────────────────┤
│  TÍNH NĂNG ĐẶC BIỆT:                                             │
│  • Vendor management (quản lý NCC)                               │
│  • Purchase orders                                               │
│  • Multi-warehouse                                               │
│  • Landed cost calculation                                       │
│  • AR/AP aging reports                                           │
│  • Profit margin analysis                                        │
├──────────────────────────────────────────────────────────────────┤
│  PHÙ HỢP: Nhập khẩu, phân phối, bán sỉ, đại lý                  │
└──────────────────────────────────────────────────────────────────┘
```

#### 🏭 MANUFACTURING - Sản xuất

```
┌──────────────────────────────────────────────────────────────────┐
│                    🏭 MANUFACTURING PACKAGE                      │
│                      "Sản xuất thông minh"                       │
├──────────────────────────────────────────────────────────────────┤
│  Modules: CORE + INV + SCM + ACC + PM + HRM                      │
│  Giá từ: 8,000,000 VNĐ/tháng                                     │
├──────────────────────────────────────────────────────────────────┤
│  TÍNH NĂNG ĐẶC BIỆT:                                             │
│  • BOM (Bill of Materials)                                       │
│  • Work orders                                                   │
│  • Production tracking                                           │
│  • Quality control                                               │
│  • Raw material management                                       │
│  • Cost accounting                                               │
├──────────────────────────────────────────────────────────────────┤
│  PHÙ HỢP: Gia công, lắp ráp, sản xuất quy mô nhỏ-vừa           │
└──────────────────────────────────────────────────────────────────┘
```

#### 🏥 HEALTHCARE - Y tế

```
┌──────────────────────────────────────────────────────────────────┐
│                      🏥 HEALTHCARE PACKAGE                       │
│                      "Chăm sóc sức khỏe"                         │
├──────────────────────────────────────────────────────────────────┤
│  Modules: CORE + CRM + INV + HRM + DMS                           │
│  Giá từ: 6,000,000 VNĐ/tháng                                     │
├──────────────────────────────────────────────────────────────────┤
│  TÍNH NĂNG ĐẶC BIỆT:                                             │
│  • Patient records (hồ sơ bệnh nhân) - CRM customized            │
│  • Appointment booking                                           │
│  • Medical inventory tracking                                    │
│  • Document management (CT, X-ray, etc.)                         │
│  • Staff rostering                                               │
│  • Treatment history                                             │
├──────────────────────────────────────────────────────────────────┤
│  PHÙ HỢP: Phòng khám, nha khoa, thú y, phòng lab                │
└──────────────────────────────────────────────────────────────────┘
```

#### 🏨 HOSPITALITY - Nhà hàng / Khách sạn

```
┌──────────────────────────────────────────────────────────────────┐
│                     🏨 HOSPITALITY PACKAGE                       │
│                   "Nhà hàng - Khách sạn"                         │
├──────────────────────────────────────────────────────────────────┤
│  Modules: CORE + CRM + INV + HRM + ACC                           │
│  Giá từ: 4,000,000 VNĐ/tháng                                     │
├──────────────────────────────────────────────────────────────────┤
│  TÍNH NĂNG ĐẶC BIỆT:                                             │
│  • Table/Room management                                         │
│  • Menu ordering                                                 │
│  • Kitchen display system integration                            │
│  • Shift scheduling                                              │
│  • Ingredient tracking                                           │
│  • Revenue per table/room                                        │
├──────────────────────────────────────────────────────────────────┤
│  PHÙ HỢP: Nhà hàng, cafe, homestay, mini hotel                  │
└──────────────────────────────────────────────────────────────────┘
```

---

### 12.5 ADD-ONS & TIỆN ÍCH BỔ SUNG

> Khách hàng có thể mua thêm các tiện ích bổ sung cho bất kỳ gói nào.

| Add-on | Mô tả | Giá/tháng |
|--------|-------|-----------|
| **Extra Users** | Thêm người dùng | 50K-100K/user |
| **Extra Storage** | Thêm dung lượng lưu trữ | 100K/10GB |
| **Extra Warehouse** | Thêm kho hàng | 200K/kho |
| **AI Advanced** | AI nâng cao, custom training | 500K-2M |
| **White Label** | Branding riêng (logo, màu, domain) | 1M-3M |
| **API Premium** | Không giới hạn API calls | 500K-1M |
| **Priority Support** | Hỗ trợ ưu tiên 24/7 | 1M-2M |
| **Custom Reports** | Báo cáo tùy chỉnh | 500K/report |
| **Data Migration** | Chuyển dữ liệu từ hệ thống cũ | Từ 2M (1 lần) |
| **Training** | Đào tạo nhân viên | 500K/buổi/người |
| **Custom Integration** | Tích hợp bên thứ 3 | Báo giá riêng |

---

### 12.6 UPGRADE PATH - LỘ TRÌNH NÂNG CẤP

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         LỘ TRÌNH PHÁT TRIỂN KHÁCH HÀNG                       │
└─────────────────────────────────────────────────────────────────────────────┘

      STARTER          →        BUSINESS         →       PROFESSIONAL      →      ENTERPRISE
    (Khởi nghiệp)              (Tăng trưởng)             (Mở rộng)               (Quy mô lớn)
         │                          │                         │                        │
         │                          │                         │                        │
    ┌────┴────┐               ┌─────┴─────┐             ┌─────┴─────┐           ┌──────┴──────┐
    │ 1-5 NV  │               │  5-15 NV  │             │ 15-50 NV  │           │   50+ NV    │
    │ 1 cửa   │               │ 1-3 cửa   │             │ 3-10 cửa  │           │  10+ chi    │
    │ hàng    │               │ hàng      │             │ hàng      │           │  nhánh      │
    └─────────┘               └───────────┘             └───────────┘           └─────────────┘
         │                          │                         │                        │
         │  + Kế toán               │  + Nhân sự              │  + Dự án               │
         │  + Báo cáo               │  + Mua hàng             │  + Tài liệu            │
         │                          │  + Multi-kho            │  + Custom              │
         │                          │                         │                        │
         └──────────────────────────┴─────────────────────────┴────────────────────────┘
                                           │
                                           ▼
                              ┌───────────────────────┐
                              │   NO DATA LOSS        │
                              │   Giữ nguyên dữ liệu  │
                              │   khi nâng cấp        │
                              └───────────────────────┘
```

#### Chính sách nâng cấp:
| Từ | Đến | Chính sách |
|----|-----|------------|
| Starter | Business | Miễn phí setup, tính giá mới từ tháng sau |
| Business | Professional | Giảm 20% tháng đầu, hỗ trợ migration |
| Professional | Enterprise | Tư vấn miễn phí, training on-site |
| Bất kỳ | Thêm module | Kích hoạt ngay, tính phí pro-rata |

---

### 12.7 FREE TRIAL & FREEMIUM

#### 12.7.1 Free Trial (Dùng thử)

| Gói trial | Thời gian | Giới hạn |
|-----------|-----------|----------|
| Starter | 14 ngày | Đầy đủ tính năng |
| Business | 14 ngày | Đầy đủ tính năng |
| Professional | 7 ngày | Đầy đủ tính năng |
| Enterprise | Demo call | Tùy chỉnh |

#### 12.7.2 Freemium Option (Miễn phí vĩnh viễn)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              🆓 FREE FOREVER                                │
│                        "Bắt đầu miễn phí, mãi mãi"                          │
├─────────────────────────────────────────────────────────────────────────────┤
│  Giá: 0 VNĐ                                                                 │
│  Users: 1 user                                                              │
├─────────────────────────────────────────────────────────────────────────────┤
│  GIỚI HẠN:                                                                  │
│  • 50 khách hàng                                                            │
│  • 100 sản phẩm                                                             │
│  • 100 đơn hàng/tháng                                                       │
│  • 500 MB storage                                                           │
│  • Branding NovaSaaS                                                        │
│  • Không AI                                                                 │
│  • Hỗ trợ community only                                                    │
├─────────────────────────────────────────────────────────────────────────────┤
│  MỤC ĐÍCH: Thu hút users, viral growth, convert lên Starter                │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

### 12.8 SO SÁNH VỚI ĐỐI THỦ

| Tính năng | NovaSaaS | KiotViet | Sapo | Odoo |
|-----------|:--------:|:--------:|:----:|:----:|
| Multi-tenant SaaS | ✅ | ✅ | ✅ | ❌ |
| Modular pricing | ✅ | ❌ | Partial | ✅ |
| AI Assistant | ✅ | ❌ | ❌ | Partial |
| RAG Chat | ✅ | ❌ | ❌ | ❌ |
| Real-time sync | ✅ | ✅ | ✅ | ✅ |
| Multi-warehouse | ✅ | ✅ | ✅ | ✅ |
| HR Management | ✅ | ❌ | ❌ | ✅ |
| Project Management | ✅ | ❌ | ❌ | ✅ |
| Document Management | ✅ | ❌ | ❌ | ✅ |
| White-label | ✅ | ❌ | ❌ | ✅ |
| On-premise option | ✅ | ❌ | ❌ | ✅ |
| API access | ✅ | Limited | Limited | ✅ |
| Custom integrations | ✅ | Limited | Limited | ✅ |
| **Giá khởi điểm** | 500K | 200K | 200K | Free |

---

### 12.9 CHIẾN LƯỢC BÁN HÀNG

#### 12.9.1 Kênh bán hàng

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           KÊNH BÁN HÀNG                                     │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│   ┌─────────────┐    ┌─────────────┐    ┌─────────────┐    ┌─────────────┐  │
│   │   DIRECT    │    │   PARTNER   │    │   ONLINE    │    │  REFERRAL   │  │
│   │             │    │             │    │             │    │             │  │
│   │ • Sales     │    │ • Reseller  │    │ • Website   │    │ • Affiliate │  │
│   │   team      │    │ • Agency    │    │ • Ads       │    │ • Customer  │  │
│   │ • Phone     │    │ • Consultant│    │ • SEO       │    │   referral  │  │
│   │ • Demo      │    │             │    │ • Content   │    │             │  │
│   └─────────────┘    └─────────────┘    └─────────────┘    └─────────────┘  │
│         │                  │                  │                  │          │
│         └──────────────────┴──────────────────┴──────────────────┘          │
│                                    │                                        │
│                                    ▼                                        │
│                          ┌─────────────────┐                                │
│                          │   CRM PIPELINE  │                                │
│                          │   Lead → Deal   │                                │
│                          └─────────────────┘                                │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

#### 12.9.2 Chiến lược pricing psychology

| Chiến lược | Áp dụng |
|------------|---------|
| **Anchoring** | Hiển thị Enterprise giá cao trước, làm các gói khác có vẻ rẻ hơn |
| **Decoy effect** | Business là gói "mồi", Professional là gói muốn bán |
| **Freemium → Paid** | Free trial → Convert % sang Starter → Upsell |
| **Annual discount** | Trả năm giảm 20% (giữ chân khách) |
| **Limited time offer** | Flash sale cho khách mới |

---

### 12.10 SLA & CAM KẾT DỊCH VỤ

| Gói | Uptime SLA | Response Time | Data Backup | Support Hours |
|-----|:----------:|:-------------:|:-----------:|:-------------:|
| Starter | 99% | 48h | Daily | 8x5 |
| Business | 99.5% | 24h | Daily | 8x5 |
| Professional | 99.9% | 4h | Hourly | 12x6 |
| Enterprise | 99.99% | 1h | Real-time | 24x7 |

---

*Tài liệu này là bản đặc tả sơ bộ. Chi tiết sẽ được bổ sung trong quá trình phát triển.*
