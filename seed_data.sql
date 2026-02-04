-- Enable UUID extension if not enabled (PostgreSQL specific)
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- =============================================
-- 1. Seed Subscription Plans (Master Schema 'public')
-- =============================================
INSERT INTO "public"."SubscriptionPlans" ("Id", "Name", "MonthlyPrice", "MaxUsers", "CreateAt")
VALUES 
('11111111-1111-1111-1111-111111111111', 'Basic', 10.00, 5, NOW()),
('22222222-2222-2222-2222-222222222222', 'Pro', 29.00, 20, NOW()),
('33333333-3333-3333-3333-333333333333', 'Enterprise', 99.00, 1000, NOW())
ON CONFLICT ("Id") DO NOTHING;

-- =============================================
-- 2. Seed Tenants (Master Schema 'public')
-- =============================================
INSERT INTO "public"."Tenants" ("Id", "Name", "Subdomain", "SchemaName", "PlanId", "IsActive", "CreateAt")
VALUES
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Demo Tenant', 'demo', 'tenant_demo', '33333333-3333-3333-3333-333333333333', true, NOW()),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Tech Solutions', 'tech', 'tenant_tech', '22222222-2222-2222-2222-222222222222', true, NOW()),
('cccccccc-cccc-cccc-cccc-cccccccccccc', 'Fashion House', 'fashion', 'tenant_fashion', '11111111-1111-1111-1111-111111111111', true, NOW())
ON CONFLICT ("Id") DO NOTHING;

-- =============================================
-- 3. Seed Roles
-- =============================================
-- Replaced mnemonic 'r','m','s','u' with valid hex '1','2','3','4'
INSERT INTO "Roles" ("Id", "Name", "CreateAt")
VALUES
('10000000-0000-0000-0000-000000000001', 'Admin', NOW()),
('10000000-0000-0000-0000-000000000002', 'Manager', NOW()),
('10000000-0000-0000-0000-000000000003', 'Staff', NOW()),
('10000000-0000-0000-0000-000000000004', 'User', NOW())
ON CONFLICT ("Id") DO NOTHING;

-- =============================================
-- 4. Seed Users
-- =============================================
-- Password placeholder: 'Password@123'
-- Replaced mnemonic 'e','f','g' (g is invalid) with valid hex '2' series
INSERT INTO "Users" ("Id", "FullName", "Email", "PasswordHash", "IsActive", "CreateAt")
VALUES
('20000000-0000-0000-0000-000000000001', 'System Admin', 'admin@novasaas.com', 'hashed_password_placeholder', true, NOW()),
('20000000-0000-0000-0000-000000000002', 'Tech Manager', 'manager@tech.com', 'hashed_password_placeholder', true, NOW()),
('20000000-0000-0000-0000-000000000003', 'Fashion Staff', 'staff@fashion.com', 'hashed_password_placeholder', true, NOW())
ON CONFLICT ("Id") DO NOTHING;

-- =============================================
-- 5. Seed UserRoles
-- =============================================
INSERT INTO "UserRoles" ("UserId", "RoleId")
VALUES
('20000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001'), -- Admin -> Admin
('20000000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000002'), -- Manager -> Manager
('20000000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000003')  -- Staff -> Staff
ON CONFLICT DO NOTHING;

-- =============================================
-- 6. Seed Inventory Master Data (Units)
-- =============================================
INSERT INTO "Units" ("Id", "Name", "ShortName", "CreateAt")
VALUES
('50000000-0000-0000-0000-000000000001', 'Piece', 'Pcs', NOW()),
('50000000-0000-0000-0000-000000000002', 'Box', 'Box', NOW()),
('50000000-0000-0000-0000-000000000003', 'Kilogram', 'Kg', NOW()),
('50000000-0000-0000-0000-000000000004', 'Liter', 'L', NOW()),
('50000000-0000-0000-0000-000000000005', 'Set', 'Set', NOW())
ON CONFLICT ("Id") DO NOTHING;

-- =============================================
-- 7. Seed Inventory Categories
-- =============================================
INSERT INTO "Categories" ("Id", "Name", "CreateAt")
VALUES
('c1000000-0000-0000-0000-000000000001', 'Electronics', NOW()), -- 'c' is valid hex
('c1000000-0000-0000-0000-000000000002', 'Clothing', NOW()),
('c1000000-0000-0000-0000-000000000003', 'Home & Garden', NOW()),
('c1000000-0000-0000-0000-000000000004', 'Office Supplies', NOW())
ON CONFLICT ("Id") DO NOTHING;

-- =============================================
-- 8. Seed Warehouses
-- =============================================
INSERT INTO "Warehouses" ("Id", "Name", "Adress", "CreateAt")
VALUES
('d1000000-0000-0000-0000-000000000001', 'Central Hub', '123 Main St, New York', NOW()), -- 'd' is valid hex
('d1000000-0000-0000-0000-000000000002', 'West Coast Warehouse', '456 Pacific Blvd, LA', NOW())
ON CONFLICT ("Id") DO NOTHING;

-- =============================================
-- 9. Seed Products
-- =============================================
INSERT INTO "Products" ("Id", "Name", "SKU", "Price", "StockQuantity", "CategoryID", "UnitId", "CreateAt")
VALUES
-- Electronics (Category: c1...1, Unit: Piece 5...1)
(uuid_generate_v4(), 'Laptop Dell XPS 15', 'DELL-XPS-15', 1899.00, 25, 'c1000000-0000-0000-0000-000000000001', '50000000-0000-0000-0000-000000000001', NOW()),
(uuid_generate_v4(), 'iPhone 15 Pro', 'IPHONE-15-PRO', 1199.00, 50, 'c1000000-0000-0000-0000-000000000001', '50000000-0000-0000-0000-000000000001', NOW()),
(uuid_generate_v4(), 'Samsung Galaxy S24', 'SAMS-S24', 999.00, 60, 'c1000000-0000-0000-0000-000000000001', '50000000-0000-0000-0000-000000000001', NOW()),
(uuid_generate_v4(), 'Sony WH-1000XM5 Headphones', 'SONY-XM5', 349.00, 100, 'c1000000-0000-0000-0000-000000000001', '50000000-0000-0000-0000-000000000001', NOW()),

-- Clothing (Category: c1...2, Unit: Piece 5...1)
(uuid_generate_v4(), 'Men T-Shirt Black', 'TSHIRT-M-BLK', 19.99, 200, 'c1000000-0000-0000-0000-000000000002', '50000000-0000-0000-0000-000000000001', NOW()),
(uuid_generate_v4(), 'Women Jeans Blue', 'JEANS-W-BLU', 49.99, 150, 'c1000000-0000-0000-0000-000000000002', '50000000-0000-0000-0000-000000000001', NOW()),
(uuid_generate_v4(), 'Running Shoes', 'NIKE-RUN-001', 89.99, 80, 'c1000000-0000-0000-0000-000000000002', '50000000-0000-0000-0000-000000000005', NOW()), -- Unit: Set 5...5

-- Home & Garden (Category: c1...3, Unit: Piece 5...1)
(uuid_generate_v4(), 'Office Chair Ergonomic', 'CHAIR-ERGO', 299.00, 30, 'c1000000-0000-0000-0000-000000000003', '50000000-0000-0000-0000-000000000001', NOW()),
(uuid_generate_v4(), 'Desk Lamp LED', 'LAMP-LED-01', 35.00, 120, 'c1000000-0000-0000-0000-000000000003', '50000000-0000-0000-0000-000000000001', NOW()),
(uuid_generate_v4(), 'Garden Hose 50ft', 'HOSE-50FT', 25.00, 45, 'c1000000-0000-0000-0000-000000000003', '50000000-0000-0000-0000-000000000001', NOW()),

-- Office Supplies (Category: c1...4, Unit: Box 5...2)
(uuid_generate_v4(), 'Printer Paper A4', 'PAPER-A4-500', 5.99, 500, 'c1000000-0000-0000-0000-000000000004', '50000000-0000-0000-0000-000000000002', NOW()), 
(uuid_generate_v4(), 'Ballpoint Pens Blue', 'PEN-BIC-BLU', 2.50, 1000, 'c1000000-0000-0000-0000-000000000004', '50000000-0000-0000-0000-000000000005', NOW()); -- Unit: Set 5...5

-- =============================================
-- 10. Seed Customers
-- =============================================
INSERT INTO "Customers" ("Id", "Name", "Phone", "TaxCode", "CreateAt")
VALUES
(uuid_generate_v4(), 'John Doe', '555-0101', 'TAX001', NOW()),
(uuid_generate_v4(), 'Jane Smith', '555-0102', 'TAX002', NOW()),
(uuid_generate_v4(), 'Acme Corp', '555-0999', 'TAX-CORP-99', NOW());
