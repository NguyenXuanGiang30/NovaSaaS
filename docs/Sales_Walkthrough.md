# Sales & CRM Module Implementation Walkthrough

Hệ thống đã hoàn tất implement module Sales & CRM với các tính năng:
1. **Quản lý Khách hàng (CRM)**: Thông tin, xếp hạng, lịch sử mua hàng.
2. **Xử lý Đơn hàng (Sales)**: Quy trình tạo đơn hàng chuẩn ACID Transaction.
3. **Quản lý Kho (Inventory Integration)**: Tự động trừ kho và ghi log biến động.
4. **Hóa đơn (Invoicing)**: Tự động sinh hóa đơn khi có đơn hàng.

## 1. Updated Database Schema
- **Customers**: Thêm `Rank`, `Type`, `TotalSpending`, `Address`, `Email`.
- **Orders**: Thêm `Status` (Enum), `SubTotal`, `TaxAmount`, `OrderNumber` (logic fix).
- **Invoices**: Thêm `InvoiceNumber`, `Status`, `DueDate`.
- **StockMovements**: Hỗ trợ log `Type: Sale` với `ReferenceCode` là mã đơn hàng.

## 2. API Endpoints

### Customers API
| Method | Endpoint | Description |
|Args|---|---|
| POST | `/api/customers` | Tạo khách hàng mới |
| GET | `/api/customers` | Lấy danh sách khách hàng |
| GET | `/api/customers/{id}` | Lấy chi tiết khách hàng |
| GET | `/api/customers/{id}/history` | Lấy lịch sử mua hàng |

### Orders API
| Method | Endpoint | Description |
|Args|---|---|
| POST | `/api/orders` | **Tạo đơn hàng** (xem chi tiết bên dưới) |
| GET | `/api/orders/{id}` | Lấy chi tiết đơn hàng (kèm items & invoice) |

## 3. Order Processing Flow (Transaction)
Khi gọi `POST /api/orders`:
1. **Validation**: Kiểm tra khách hàng, sản phẩm tồn tại.
2. **Begin Transaction**: Mở giao dịch database.
3. **Create Order**: Tạo đơn hàng với trạng thái `Confirmed`.
4. **Stock Deduction**:
   - Kiểm tra tồn kho từng sản phẩm (Throw error nếu thiếu).
   - Trừ kho thực tế (`Stock.Quantity`).
   - Ghi log biến động kho (`StockMovement`) với Type=`Sale`, Reference=`OrderNumber`.
5. **Invoice Generation**: Tự động tạo Invoice trạng thái `Unpaid`.
6. **Commit Transaction**: Tất cả thay đổi được lưu đồng thời. Nếu có lỗi bất kỳ -> Rollback toàn bộ.

## 4. Example Payload (Create Order)
```json
{
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "note": "Giao hàng giờ hành chính",
  "items": [
    {
      "productId": "995a9757-96a8-4229-87c1-17937746487e",
      "quantity": 2,
      "warehouseId": "a1b2c3d4-e5f6-7890-1234-567890abcdef"
    }
  ]
}
```
