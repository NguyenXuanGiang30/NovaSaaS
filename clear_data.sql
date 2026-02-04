-- =============================================
-- Script to DELETE ALL DATA from NovaSaaS Database
-- WARNING: This will remove ALL records from ALL tables!
-- =============================================

-- Disable foreign key checks temporarily by using TRUNCATE with CASCADE
-- This automatically handles dependent rows

-- 1. Clear Business & Inventory Data (Child tables first)
TRUNCATE TABLE "OrderItems" CASCADE;
TRUNCATE TABLE "Orders" CASCADE;
TRUNCATE TABLE "Invoices" CASCADE;
TRUNCATE TABLE "StockMovements" CASCADE;
TRUNCATE TABLE "Products" CASCADE;
TRUNCATE TABLE "Customers" CASCADE;
TRUNCATE TABLE "Coupons" CASCADE;

-- 2. Clear Inventory Master Data
TRUNCATE TABLE "Categories" CASCADE;
TRUNCATE TABLE "Units" CASCADE;
TRUNCATE TABLE "Warehouses" CASCADE;

-- 3. Clear Identity Data
TRUNCATE TABLE "UserRoles" CASCADE;
TRUNCATE TABLE "RolePermissions" CASCADE;
TRUNCATE TABLE "Users" CASCADE;
TRUNCATE TABLE "Roles" CASCADE;
TRUNCATE TABLE "Permissions" CASCADE;

-- 4. Clear AI & Logs
TRUNCATE TABLE "DocumentSegments" CASCADE;
TRUNCATE TABLE "KnowledgeDocuments" CASCADE;
TRUNCATE TABLE "AuditLogs" CASCADE;

-- 5. Clear Master Data (Public Schema)
TRUNCATE TABLE "public"."Payments" CASCADE;
TRUNCATE TABLE "public"."Tenants" CASCADE;
TRUNCATE TABLE "public"."PlanFeatures" CASCADE;
TRUNCATE TABLE "public"."SubscriptionPlans" CASCADE;
TRUNCATE TABLE "public"."GlobalAuditLogs" CASCADE;
TRUNCATE TABLE "public"."MasterAdmins" CASCADE;

-- Done!
-- All data has been removed. You can now run seed_data.sql to repopulate.
