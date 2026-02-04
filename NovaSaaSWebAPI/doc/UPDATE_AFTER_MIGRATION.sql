-- =============================================
-- Update script để fix Tenants sau migration AddMasterAdminPortal
-- Chạy sau khi apply migration
-- =============================================

-- 1. Kích hoạt pgvector extension (NẾU chưa có)
CREATE EXTENSION IF NOT EXISTS vector;

-- 2. Set Status = 1 (Active) cho các Tenants có Status = 0
UPDATE public."Tenants" 
SET 
    "Status" = 1,  -- TenantStatus.Active
    "SubscriptionStartDate" = NOW(),
    "SubscriptionEndDate" = NOW() + INTERVAL '1 year'
WHERE "Status" = 0;

-- 3. Update SubscriptionPlans với quota values
UPDATE public."SubscriptionPlans" 
SET 
    "MaxMonthlyAICalls" = 100,
    "MaxStorageMB" = 100,
    "MaxDocuments" = 10,
    "AIEnabled" = false,
    "GracePeriodDays" = 7,
    "Description" = 'Gói cơ bản cho doanh nghiệp nhỏ'
WHERE "Name" = 'Basic';

UPDATE public."SubscriptionPlans" 
SET 
    "MaxMonthlyAICalls" = 1000,
    "MaxStorageMB" = 1000,
    "MaxDocuments" = 100,
    "AIEnabled" = true,
    "GracePeriodDays" = 14,
    "Description" = 'Gói chuyên nghiệp với AI hỗ trợ'
WHERE "Name" = 'Pro';

UPDATE public."SubscriptionPlans" 
SET 
    "MaxMonthlyAICalls" = 10000,
    "MaxStorageMB" = 10000,
    "MaxDocuments" = 1000,
    "AIEnabled" = true,
    "GracePeriodDays" = 30,
    "AllowOverage" = true,
    "OveragePricePer1000Calls" = 0.50,
    "Description" = 'Gói doanh nghiệp với tính năng đầy đủ'
WHERE "Name" = 'Enterprise';

-- 4. Kiểm tra kết quả
SELECT "Id", "Name", "Status", "SubscriptionStartDate", "SubscriptionEndDate" FROM public."Tenants";
SELECT "Id", "Name", "MaxMonthlyAICalls", "MaxStorageMB", "AIEnabled", "GracePeriodDays" FROM public."SubscriptionPlans";

-- 5. Kiểm tra pgvector
SELECT extname, extversion FROM pg_extension WHERE extname = 'vector';
