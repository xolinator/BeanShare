-- BeanShare Comprehensive Seed Data
-- Matches Keycloak users: testuser, alice, bob

-- Clear existing data (in dependency order)
DELETE FROM "UserPresetFavorites";
DELETE FROM "SpaceGlobalPresetConfigs";
DELETE FROM "SettlementLines";
DELETE FROM "Settlements";
DELETE FROM "Consumptions";
DELETE FROM "StockLevels";
DELETE FROM "Purchases";
DELETE FROM "CoffeeStocks";
DELETE FROM "BillingPeriods";
DELETE FROM "Notifications";
DELETE FROM "PresetRecipes";
DELETE FROM "GlobalPresets";
DELETE FROM "SpaceMemberships";
DELETE FROM "Spaces";
DELETE FROM "Users";

-- =============================================
-- USERS (matching Keycloak IDs)
-- =============================================
INSERT INTO "Users" ("Id", "Email", "Name", "PictureUrl", "Provider", "ProviderUserId", "PasswordHash", "CreatedAt", "LastLoginAt", "PreferredCurrencyCode", "DeactivatedAt", "IsActive", "SystemRole")
VALUES
  ('9ac64087-c765-4022-92b6-f740ba312566', 'test@beanshare.com', 'Test User', NULL, 3, '9ac64087-c765-4022-92b6-f740ba312566', NULL, NOW() - INTERVAL '60 days', NOW(), 'CZK', NULL, true, 1),
  ('23f2a7df-4f3e-43ff-8fff-8ff400816cad', 'alice@beanshare.com', 'Alice Johnson', NULL, 3, '23f2a7df-4f3e-43ff-8fff-8ff400816cad', NULL, NOW() - INTERVAL '45 days', NOW() - INTERVAL '1 day', 'EUR', NULL, true, 0),
  ('bf7a269c-3ad2-42c4-9904-25ba51702ef3', 'bob@beanshare.com', 'Bob Smith', NULL, 3, 'bf7a269c-3ad2-42c4-9904-25ba51702ef3', NULL, NOW() - INTERVAL '30 days', NOW() - INTERVAL '2 days', 'USD', NULL, true, 0);

-- =============================================
-- SPACES
-- =============================================
INSERT INTO "Spaces" ("Id", "Name", "InviteCode", "IsActive", "CreatedAt", "Currency")
VALUES
  ('a1000000-0000-0000-0000-000000000001', 'Office Coffee Club', 'FFCE2824', true, NOW() - INTERVAL '55 days', 'CZK'),
  ('a1000000-0000-0000-0000-000000000002', 'Home Brew Squad', 'HMBREW22', true, NOW() - INTERVAL '40 days', 'EUR'),
  ('a1000000-0000-0000-0000-000000000003', 'Weekend Baristas', 'WKND4FUN', true, NOW() - INTERVAL '20 days', 'USD');

-- =============================================
-- SPACE MEMBERSHIPS
-- =============================================
INSERT INTO "SpaceMemberships" ("UserId", "SpaceId", "Role", "JoinedAt")
VALUES
  ('9ac64087-c765-4022-92b6-f740ba312566', 'a1000000-0000-0000-0000-000000000001', 'Admin', NOW() - INTERVAL '55 days'),
  ('23f2a7df-4f3e-43ff-8fff-8ff400816cad', 'a1000000-0000-0000-0000-000000000001', 'Member', NOW() - INTERVAL '50 days'),
  ('bf7a269c-3ad2-42c4-9904-25ba51702ef3', 'a1000000-0000-0000-0000-000000000001', 'Member', NOW() - INTERVAL '48 days'),
  ('23f2a7df-4f3e-43ff-8fff-8ff400816cad', 'a1000000-0000-0000-0000-000000000002', 'Admin', NOW() - INTERVAL '40 days'),
  ('9ac64087-c765-4022-92b6-f740ba312566', 'a1000000-0000-0000-0000-000000000002', 'Member', NOW() - INTERVAL '38 days'),
  ('bf7a269c-3ad2-42c4-9904-25ba51702ef3', 'a1000000-0000-0000-0000-000000000003', 'Admin', NOW() - INTERVAL '20 days'),
  ('23f2a7df-4f3e-43ff-8fff-8ff400816cad', 'a1000000-0000-0000-0000-000000000003', 'Member', NOW() - INTERVAL '18 days');

-- =============================================
-- COFFEE STOCKS (per space)
-- =============================================
INSERT INTO "CoffeeStocks" ("Id", "SpaceId", "CreatedAt", "UpdatedAt")
VALUES
  ('b1000000-0000-0000-0000-000000000001', 'a1000000-0000-0000-0000-000000000001', NOW() - INTERVAL '55 days', NOW()),
  ('b1000000-0000-0000-0000-000000000002', 'a1000000-0000-0000-0000-000000000002', NOW() - INTERVAL '40 days', NOW()),
  ('b1000000-0000-0000-0000-000000000003', 'a1000000-0000-0000-0000-000000000003', NOW() - INTERVAL '20 days', NOW());

-- =============================================
-- PURCHASES
-- =============================================
INSERT INTO "Purchases" ("Id", "ProductName", "ProductBrand", "ProductType", "QuantityGrams", "CostAmount", "CostCurrency", "Vendor", "PurchasedBy", "PurchasedAt", "CreatedAt", "CoffeeStockId")
VALUES
  ('c1000000-0000-0000-0000-000000000001', 'Ethiopia Yirgacheffe', 'Lavazza', 'Specialty', 1000, 450, 'CZK', 'Kava.cz', '9ac64087-c765-4022-92b6-f740ba312566', NOW() - INTERVAL '50 days', NOW() - INTERVAL '50 days', 'b1000000-0000-0000-0000-000000000001'),
  ('c1000000-0000-0000-0000-000000000002', 'Colombia Supremo', 'Illy', 'Espresso', 500, 299, 'CZK', 'Alza.cz', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', NOW() - INTERVAL '45 days', NOW() - INTERVAL '45 days', 'b1000000-0000-0000-0000-000000000001'),
  ('c1000000-0000-0000-0000-000000000003', 'Morning Blend', 'Tchibo', 'Filter', 750, 189, 'CZK', 'Albert', 'bf7a269c-3ad2-42c4-9904-25ba51702ef3', NOW() - INTERVAL '30 days', NOW() - INTERVAL '30 days', 'b1000000-0000-0000-0000-000000000001'),
  ('c1000000-0000-0000-0000-000000000004', 'Decaf Swiss Water', 'Starbucks', 'Decaf', 250, 159, 'CZK', 'Tesco', '9ac64087-c765-4022-92b6-f740ba312566', NOW() - INTERVAL '20 days', NOW() - INTERVAL '20 days', 'b1000000-0000-0000-0000-000000000001'),
  ('c1000000-0000-0000-0000-000000000005', 'Classic Instant', 'Nescafe', 'Instant', 200, 99, 'CZK', 'Lidl', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', NOW() - INTERVAL '15 days', NOW() - INTERVAL '15 days', 'b1000000-0000-0000-0000-000000000001'),
  ('c1000000-0000-0000-0000-000000000006', 'Kenya AA', 'Douwe Egberts', 'Specialty', 500, 15.50, 'EUR', 'CoffeeShop.eu', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', NOW() - INTERVAL '35 days', NOW() - INTERVAL '35 days', 'b1000000-0000-0000-0000-000000000002'),
  ('c1000000-0000-0000-0000-000000000007', 'Brazil Santos', 'Lavazza', 'Espresso', 1000, 22.00, 'EUR', 'Amazon.de', '9ac64087-c765-4022-92b6-f740ba312566', NOW() - INTERVAL '25 days', NOW() - INTERVAL '25 days', 'b1000000-0000-0000-0000-000000000002'),
  ('c1000000-0000-0000-0000-000000000008', 'Guatemala Antigua', 'illy', 'Filter', 250, 8.99, 'EUR', 'Local Roaster', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', NOW() - INTERVAL '10 days', NOW() - INTERVAL '10 days', 'b1000000-0000-0000-0000-000000000002'),
  ('c1000000-0000-0000-0000-000000000009', 'Sumatra Mandheling', 'Peets', 'Specialty', 750, 18.99, 'USD', 'Peets.com', 'bf7a269c-3ad2-42c4-9904-25ba51702ef3', NOW() - INTERVAL '18 days', NOW() - INTERVAL '18 days', 'b1000000-0000-0000-0000-000000000003'),
  ('c1000000-0000-0000-0000-000000000010', 'Dark Roast Blend', 'Starbucks', 'Espresso', 500, 14.50, 'USD', 'Target', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', NOW() - INTERVAL '12 days', NOW() - INTERVAL '12 days', 'b1000000-0000-0000-0000-000000000003');

-- =============================================
-- STOCK LEVELS
-- =============================================
INSERT INTO "StockLevels" ("Id", "ProductName", "ProductBrand", "ProductType", "TotalPurchasedGrams", "TotalConsumedGrams", "UpdatedAt", "CoffeeStockId")
VALUES
  ('d1000000-0000-0000-0000-000000000001', 'Ethiopia Yirgacheffe', 'Lavazza', 'Specialty', 1000, 720, NOW(), 'b1000000-0000-0000-0000-000000000001'),
  ('d1000000-0000-0000-0000-000000000002', 'Colombia Supremo', 'Illy', 'Espresso', 500, 380, NOW(), 'b1000000-0000-0000-0000-000000000001'),
  ('d1000000-0000-0000-0000-000000000003', 'Morning Blend', 'Tchibo', 'Filter', 750, 450, NOW(), 'b1000000-0000-0000-0000-000000000001'),
  ('d1000000-0000-0000-0000-000000000004', 'Decaf Swiss Water', 'Starbucks', 'Decaf', 250, 80, NOW(), 'b1000000-0000-0000-0000-000000000001'),
  ('d1000000-0000-0000-0000-000000000005', 'Classic Instant', 'Nescafe', 'Instant', 200, 60, NOW(), 'b1000000-0000-0000-0000-000000000001'),
  ('d1000000-0000-0000-0000-000000000006', 'Kenya AA', 'Douwe Egberts', 'Specialty', 500, 320, NOW(), 'b1000000-0000-0000-0000-000000000002'),
  ('d1000000-0000-0000-0000-000000000007', 'Brazil Santos', 'Lavazza', 'Espresso', 1000, 550, NOW(), 'b1000000-0000-0000-0000-000000000002'),
  ('d1000000-0000-0000-0000-000000000008', 'Guatemala Antigua', 'illy', 'Filter', 250, 90, NOW(), 'b1000000-0000-0000-0000-000000000002'),
  ('d1000000-0000-0000-0000-000000000009', 'Sumatra Mandheling', 'Peets', 'Specialty', 750, 280, NOW(), 'b1000000-0000-0000-0000-000000000003'),
  ('d1000000-0000-0000-0000-000000000010', 'Dark Roast Blend', 'Starbucks', 'Espresso', 500, 190, NOW(), 'b1000000-0000-0000-0000-000000000003');

-- =============================================
-- BILLING PERIODS
-- =============================================
INSERT INTO "BillingPeriods" ("Id", "SpaceId", "Name", "StartDate", "EndDate", "State", "CreatedAt", "ClosedAt", "SettledAt", "CreatedBy", "ClosedBy", "SettledBy")
VALUES
  ('e1000000-0000-0000-0000-000000000001', 'a1000000-0000-0000-0000-000000000001', 'January 2026', NOW() - INTERVAL '55 days', NOW() - INTERVAL '25 days', 'Settled', NOW() - INTERVAL '55 days', NOW() - INTERVAL '25 days', NOW() - INTERVAL '23 days', '9ac64087-c765-4022-92b6-f740ba312566', '9ac64087-c765-4022-92b6-f740ba312566', '9ac64087-c765-4022-92b6-f740ba312566'),
  ('e1000000-0000-0000-0000-000000000002', 'a1000000-0000-0000-0000-000000000001', 'February 2026 (Week 1-2)', NOW() - INTERVAL '25 days', NOW() - INTERVAL '5 days', 'Closed', NOW() - INTERVAL '25 days', NOW() - INTERVAL '5 days', NULL, '9ac64087-c765-4022-92b6-f740ba312566', '9ac64087-c765-4022-92b6-f740ba312566', NULL),
  ('e1000000-0000-0000-0000-000000000003', 'a1000000-0000-0000-0000-000000000001', 'February 2026 (Week 3+)', NOW() - INTERVAL '5 days', NOW() + INTERVAL '25 days', 'Open', NOW() - INTERVAL '5 days', NULL, NULL, '9ac64087-c765-4022-92b6-f740ba312566', NULL, NULL),
  ('e1000000-0000-0000-0000-000000000004', 'a1000000-0000-0000-0000-000000000002', 'January 2026', NOW() - INTERVAL '40 days', NOW() - INTERVAL '10 days', 'Closed', NOW() - INTERVAL '40 days', NOW() - INTERVAL '10 days', NULL, '23f2a7df-4f3e-43ff-8fff-8ff400816cad', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', NULL),
  ('e1000000-0000-0000-0000-000000000005', 'a1000000-0000-0000-0000-000000000002', 'February 2026', NOW() - INTERVAL '10 days', NOW() + INTERVAL '20 days', 'Open', NOW() - INTERVAL '10 days', NULL, NULL, '23f2a7df-4f3e-43ff-8fff-8ff400816cad', NULL, NULL),
  ('e1000000-0000-0000-0000-000000000006', 'a1000000-0000-0000-0000-000000000003', 'February 2026', NOW() - INTERVAL '18 days', NOW() + INTERVAL '12 days', 'Open', NOW() - INTERVAL '18 days', NULL, NULL, 'bf7a269c-3ad2-42c4-9904-25ba51702ef3', NULL, NULL);

-- =============================================
-- CONSUMPTIONS (55 records across all spaces/periods)
-- =============================================
INSERT INTO "Consumptions" ("Id", "SpaceId", "UserId", "ProductName", "ProductBrand", "ProductType", "QuantityGrams", "ConsumedAt", "CreatedAt", "PresetId", "PresetName", "BillingPeriodId")
VALUES
  -- Office Coffee Club - January (settled)
  ('f1000000-0000-0000-0000-000000000001', 'a1000000-0000-0000-0000-000000000001', '9ac64087-c765-4022-92b6-f740ba312566', 'Ethiopia Yirgacheffe', 'Lavazza', 'Specialty', 18, NOW() - INTERVAL '50 days', NOW() - INTERVAL '50 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000001'),
  ('f1000000-0000-0000-0000-000000000002', 'a1000000-0000-0000-0000-000000000001', '9ac64087-c765-4022-92b6-f740ba312566', 'Colombia Supremo', 'Illy', 'Espresso', 14, NOW() - INTERVAL '48 days', NOW() - INTERVAL '48 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000001'),
  ('f1000000-0000-0000-0000-000000000003', 'a1000000-0000-0000-0000-000000000001', '9ac64087-c765-4022-92b6-f740ba312566', 'Ethiopia Yirgacheffe', 'Lavazza', 'Specialty', 18, NOW() - INTERVAL '46 days', NOW() - INTERVAL '46 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000001'),
  ('f1000000-0000-0000-0000-000000000004', 'a1000000-0000-0000-0000-000000000001', '9ac64087-c765-4022-92b6-f740ba312566', 'Morning Blend', 'Tchibo', 'Filter', 12, NOW() - INTERVAL '44 days', NOW() - INTERVAL '44 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000001'),
  ('f1000000-0000-0000-0000-000000000005', 'a1000000-0000-0000-0000-000000000001', '9ac64087-c765-4022-92b6-f740ba312566', 'Colombia Supremo', 'Illy', 'Espresso', 14, NOW() - INTERVAL '42 days', NOW() - INTERVAL '42 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000001'),
  ('f1000000-0000-0000-0000-000000000006', 'a1000000-0000-0000-0000-000000000001', '9ac64087-c765-4022-92b6-f740ba312566', 'Ethiopia Yirgacheffe', 'Lavazza', 'Specialty', 18, NOW() - INTERVAL '40 days', NOW() - INTERVAL '40 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000001'),
  ('f1000000-0000-0000-0000-000000000007', 'a1000000-0000-0000-0000-000000000001', '9ac64087-c765-4022-92b6-f740ba312566', 'Morning Blend', 'Tchibo', 'Filter', 12, NOW() - INTERVAL '38 days', NOW() - INTERVAL '38 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000001'),
  ('f1000000-0000-0000-0000-000000000008', 'a1000000-0000-0000-0000-000000000001', '9ac64087-c765-4022-92b6-f740ba312566', 'Colombia Supremo', 'Illy', 'Espresso', 14, NOW() - INTERVAL '35 days', NOW() - INTERVAL '35 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000001'),
  ('f1000000-0000-0000-0000-000000000009', 'a1000000-0000-0000-0000-000000000001', '9ac64087-c765-4022-92b6-f740ba312566', 'Ethiopia Yirgacheffe', 'Lavazza', 'Specialty', 18, NOW() - INTERVAL '32 days', NOW() - INTERVAL '32 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000001'),
  ('f1000000-0000-0000-0000-000000000010', 'a1000000-0000-0000-0000-000000000001', '9ac64087-c765-4022-92b6-f740ba312566', 'Decaf Swiss Water', 'Starbucks', 'Decaf', 10, NOW() - INTERVAL '28 days', NOW() - INTERVAL '28 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000001'),
  ('f1000000-0000-0000-0000-000000000011', 'a1000000-0000-0000-0000-000000000001', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', 'Ethiopia Yirgacheffe', 'Lavazza', 'Specialty', 18, NOW() - INTERVAL '49 days', NOW() - INTERVAL '49 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000001'),
  ('f1000000-0000-0000-0000-000000000012', 'a1000000-0000-0000-0000-000000000001', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', 'Colombia Supremo', 'Illy', 'Espresso', 14, NOW() - INTERVAL '47 days', NOW() - INTERVAL '47 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000001'),
  ('f1000000-0000-0000-0000-000000000013', 'a1000000-0000-0000-0000-000000000001', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', 'Morning Blend', 'Tchibo', 'Filter', 12, NOW() - INTERVAL '45 days', NOW() - INTERVAL '45 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000001'),
  ('f1000000-0000-0000-0000-000000000014', 'a1000000-0000-0000-0000-000000000001', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', 'Ethiopia Yirgacheffe', 'Lavazza', 'Specialty', 18, NOW() - INTERVAL '43 days', NOW() - INTERVAL '43 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000001'),
  ('f1000000-0000-0000-0000-000000000015', 'a1000000-0000-0000-0000-000000000001', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', 'Colombia Supremo', 'Illy', 'Espresso', 14, NOW() - INTERVAL '41 days', NOW() - INTERVAL '41 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000001'),
  ('f1000000-0000-0000-0000-000000000016', 'a1000000-0000-0000-0000-000000000001', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', 'Morning Blend', 'Tchibo', 'Filter', 12, NOW() - INTERVAL '37 days', NOW() - INTERVAL '37 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000001'),
  ('f1000000-0000-0000-0000-000000000017', 'a1000000-0000-0000-0000-000000000001', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', 'Classic Instant', 'Nescafe', 'Instant', 8, NOW() - INTERVAL '33 days', NOW() - INTERVAL '33 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000001'),
  ('f1000000-0000-0000-0000-000000000018', 'a1000000-0000-0000-0000-000000000001', 'bf7a269c-3ad2-42c4-9904-25ba51702ef3', 'Ethiopia Yirgacheffe', 'Lavazza', 'Specialty', 18, NOW() - INTERVAL '48 days', NOW() - INTERVAL '48 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000001'),
  ('f1000000-0000-0000-0000-000000000019', 'a1000000-0000-0000-0000-000000000001', 'bf7a269c-3ad2-42c4-9904-25ba51702ef3', 'Morning Blend', 'Tchibo', 'Filter', 12, NOW() - INTERVAL '45 days', NOW() - INTERVAL '45 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000001'),
  ('f1000000-0000-0000-0000-000000000020', 'a1000000-0000-0000-0000-000000000001', 'bf7a269c-3ad2-42c4-9904-25ba51702ef3', 'Colombia Supremo', 'Illy', 'Espresso', 14, NOW() - INTERVAL '42 days', NOW() - INTERVAL '42 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000001'),
  ('f1000000-0000-0000-0000-000000000021', 'a1000000-0000-0000-0000-000000000001', 'bf7a269c-3ad2-42c4-9904-25ba51702ef3', 'Ethiopia Yirgacheffe', 'Lavazza', 'Specialty', 18, NOW() - INTERVAL '39 days', NOW() - INTERVAL '39 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000001'),
  ('f1000000-0000-0000-0000-000000000022', 'a1000000-0000-0000-0000-000000000001', 'bf7a269c-3ad2-42c4-9904-25ba51702ef3', 'Morning Blend', 'Tchibo', 'Filter', 12, NOW() - INTERVAL '36 days', NOW() - INTERVAL '36 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000001'),
  ('f1000000-0000-0000-0000-000000000023', 'a1000000-0000-0000-0000-000000000001', 'bf7a269c-3ad2-42c4-9904-25ba51702ef3', 'Decaf Swiss Water', 'Starbucks', 'Decaf', 10, NOW() - INTERVAL '30 days', NOW() - INTERVAL '30 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000001'),
  -- Office Coffee Club - February Week 1-2 (closed)
  ('f1000000-0000-0000-0000-000000000024', 'a1000000-0000-0000-0000-000000000001', '9ac64087-c765-4022-92b6-f740ba312566', 'Ethiopia Yirgacheffe', 'Lavazza', 'Specialty', 18, NOW() - INTERVAL '22 days', NOW() - INTERVAL '22 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000002'),
  ('f1000000-0000-0000-0000-000000000025', 'a1000000-0000-0000-0000-000000000001', '9ac64087-c765-4022-92b6-f740ba312566', 'Colombia Supremo', 'Illy', 'Espresso', 14, NOW() - INTERVAL '18 days', NOW() - INTERVAL '18 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000002'),
  ('f1000000-0000-0000-0000-000000000026', 'a1000000-0000-0000-0000-000000000001', '9ac64087-c765-4022-92b6-f740ba312566', 'Morning Blend', 'Tchibo', 'Filter', 12, NOW() - INTERVAL '14 days', NOW() - INTERVAL '14 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000002'),
  ('f1000000-0000-0000-0000-000000000027', 'a1000000-0000-0000-0000-000000000001', '9ac64087-c765-4022-92b6-f740ba312566', 'Classic Instant', 'Nescafe', 'Instant', 8, NOW() - INTERVAL '10 days', NOW() - INTERVAL '10 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000002'),
  ('f1000000-0000-0000-0000-000000000028', 'a1000000-0000-0000-0000-000000000001', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', 'Ethiopia Yirgacheffe', 'Lavazza', 'Specialty', 18, NOW() - INTERVAL '20 days', NOW() - INTERVAL '20 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000002'),
  ('f1000000-0000-0000-0000-000000000029', 'a1000000-0000-0000-0000-000000000001', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', 'Colombia Supremo', 'Illy', 'Espresso', 14, NOW() - INTERVAL '16 days', NOW() - INTERVAL '16 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000002'),
  ('f1000000-0000-0000-0000-000000000030', 'a1000000-0000-0000-0000-000000000001', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', 'Morning Blend', 'Tchibo', 'Filter', 12, NOW() - INTERVAL '12 days', NOW() - INTERVAL '12 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000002'),
  ('f1000000-0000-0000-0000-000000000031', 'a1000000-0000-0000-0000-000000000001', 'bf7a269c-3ad2-42c4-9904-25ba51702ef3', 'Ethiopia Yirgacheffe', 'Lavazza', 'Specialty', 18, NOW() - INTERVAL '21 days', NOW() - INTERVAL '21 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000002'),
  ('f1000000-0000-0000-0000-000000000032', 'a1000000-0000-0000-0000-000000000001', 'bf7a269c-3ad2-42c4-9904-25ba51702ef3', 'Morning Blend', 'Tchibo', 'Filter', 12, NOW() - INTERVAL '13 days', NOW() - INTERVAL '13 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000002'),
  ('f1000000-0000-0000-0000-000000000033', 'a1000000-0000-0000-0000-000000000001', 'bf7a269c-3ad2-42c4-9904-25ba51702ef3', 'Decaf Swiss Water', 'Starbucks', 'Decaf', 10, NOW() - INTERVAL '8 days', NOW() - INTERVAL '8 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000002'),
  -- Office Coffee Club - Current open period
  ('f1000000-0000-0000-0000-000000000034', 'a1000000-0000-0000-0000-000000000001', '9ac64087-c765-4022-92b6-f740ba312566', 'Ethiopia Yirgacheffe', 'Lavazza', 'Specialty', 18, NOW() - INTERVAL '4 days', NOW() - INTERVAL '4 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000003'),
  ('f1000000-0000-0000-0000-000000000035', 'a1000000-0000-0000-0000-000000000001', '9ac64087-c765-4022-92b6-f740ba312566', 'Colombia Supremo', 'Illy', 'Espresso', 14, NOW() - INTERVAL '2 days', NOW() - INTERVAL '2 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000003'),
  ('f1000000-0000-0000-0000-000000000036', 'a1000000-0000-0000-0000-000000000001', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', 'Morning Blend', 'Tchibo', 'Filter', 12, NOW() - INTERVAL '3 days', NOW() - INTERVAL '3 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000003'),
  ('f1000000-0000-0000-0000-000000000037', 'a1000000-0000-0000-0000-000000000001', 'bf7a269c-3ad2-42c4-9904-25ba51702ef3', 'Ethiopia Yirgacheffe', 'Lavazza', 'Specialty', 18, NOW() - INTERVAL '1 day', NOW() - INTERVAL '1 day', NULL, NULL, 'e1000000-0000-0000-0000-000000000003'),
  -- Home Brew Squad consumptions
  ('f1000000-0000-0000-0000-000000000038', 'a1000000-0000-0000-0000-000000000002', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', 'Kenya AA', 'Douwe Egberts', 'Specialty', 18, NOW() - INTERVAL '33 days', NOW() - INTERVAL '33 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000004'),
  ('f1000000-0000-0000-0000-000000000039', 'a1000000-0000-0000-0000-000000000002', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', 'Brazil Santos', 'Lavazza', 'Espresso', 14, NOW() - INTERVAL '28 days', NOW() - INTERVAL '28 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000004'),
  ('f1000000-0000-0000-0000-000000000040', 'a1000000-0000-0000-0000-000000000002', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', 'Kenya AA', 'Douwe Egberts', 'Specialty', 18, NOW() - INTERVAL '22 days', NOW() - INTERVAL '22 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000004'),
  ('f1000000-0000-0000-0000-000000000041', 'a1000000-0000-0000-0000-000000000002', '9ac64087-c765-4022-92b6-f740ba312566', 'Brazil Santos', 'Lavazza', 'Espresso', 14, NOW() - INTERVAL '30 days', NOW() - INTERVAL '30 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000004'),
  ('f1000000-0000-0000-0000-000000000042', 'a1000000-0000-0000-0000-000000000002', '9ac64087-c765-4022-92b6-f740ba312566', 'Kenya AA', 'Douwe Egberts', 'Specialty', 18, NOW() - INTERVAL '25 days', NOW() - INTERVAL '25 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000004'),
  ('f1000000-0000-0000-0000-000000000043', 'a1000000-0000-0000-0000-000000000002', '9ac64087-c765-4022-92b6-f740ba312566', 'Brazil Santos', 'Lavazza', 'Espresso', 14, NOW() - INTERVAL '18 days', NOW() - INTERVAL '18 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000004'),
  ('f1000000-0000-0000-0000-000000000044', 'a1000000-0000-0000-0000-000000000002', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', 'Guatemala Antigua', 'illy', 'Filter', 12, NOW() - INTERVAL '8 days', NOW() - INTERVAL '8 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000005'),
  ('f1000000-0000-0000-0000-000000000045', 'a1000000-0000-0000-0000-000000000002', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', 'Brazil Santos', 'Lavazza', 'Espresso', 14, NOW() - INTERVAL '5 days', NOW() - INTERVAL '5 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000005'),
  ('f1000000-0000-0000-0000-000000000046', 'a1000000-0000-0000-0000-000000000002', '9ac64087-c765-4022-92b6-f740ba312566', 'Kenya AA', 'Douwe Egberts', 'Specialty', 18, NOW() - INTERVAL '6 days', NOW() - INTERVAL '6 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000005'),
  ('f1000000-0000-0000-0000-000000000047', 'a1000000-0000-0000-0000-000000000002', '9ac64087-c765-4022-92b6-f740ba312566', 'Guatemala Antigua', 'illy', 'Filter', 12, NOW() - INTERVAL '2 days', NOW() - INTERVAL '2 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000005'),
  -- Weekend Baristas consumptions
  ('f1000000-0000-0000-0000-000000000048', 'a1000000-0000-0000-0000-000000000003', 'bf7a269c-3ad2-42c4-9904-25ba51702ef3', 'Sumatra Mandheling', 'Peets', 'Specialty', 18, NOW() - INTERVAL '16 days', NOW() - INTERVAL '16 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000006'),
  ('f1000000-0000-0000-0000-000000000049', 'a1000000-0000-0000-0000-000000000003', 'bf7a269c-3ad2-42c4-9904-25ba51702ef3', 'Dark Roast Blend', 'Starbucks', 'Espresso', 14, NOW() - INTERVAL '14 days', NOW() - INTERVAL '14 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000006'),
  ('f1000000-0000-0000-0000-000000000050', 'a1000000-0000-0000-0000-000000000003', 'bf7a269c-3ad2-42c4-9904-25ba51702ef3', 'Sumatra Mandheling', 'Peets', 'Specialty', 18, NOW() - INTERVAL '10 days', NOW() - INTERVAL '10 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000006'),
  ('f1000000-0000-0000-0000-000000000051', 'a1000000-0000-0000-0000-000000000003', 'bf7a269c-3ad2-42c4-9904-25ba51702ef3', 'Dark Roast Blend', 'Starbucks', 'Espresso', 14, NOW() - INTERVAL '7 days', NOW() - INTERVAL '7 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000006'),
  ('f1000000-0000-0000-0000-000000000052', 'a1000000-0000-0000-0000-000000000003', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', 'Sumatra Mandheling', 'Peets', 'Specialty', 18, NOW() - INTERVAL '15 days', NOW() - INTERVAL '15 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000006'),
  ('f1000000-0000-0000-0000-000000000053', 'a1000000-0000-0000-0000-000000000003', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', 'Dark Roast Blend', 'Starbucks', 'Espresso', 14, NOW() - INTERVAL '11 days', NOW() - INTERVAL '11 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000006'),
  ('f1000000-0000-0000-0000-000000000054', 'a1000000-0000-0000-0000-000000000003', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', 'Sumatra Mandheling', 'Peets', 'Specialty', 18, NOW() - INTERVAL '5 days', NOW() - INTERVAL '5 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000006'),
  ('f1000000-0000-0000-0000-000000000055', 'a1000000-0000-0000-0000-000000000003', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', 'Dark Roast Blend', 'Starbucks', 'Espresso', 14, NOW() - INTERVAL '2 days', NOW() - INTERVAL '2 days', NULL, NULL, 'e1000000-0000-0000-0000-000000000006');

-- =============================================
-- SETTLEMENTS
-- =============================================
INSERT INTO "Settlements" ("Id", "SpaceId", "BillingPeriodId", "GeneratedAt", "GeneratedBy", "Currency", "TotalAmount", "CompletedAt", "Status")
VALUES
  ('71000000-0000-0000-0000-000000000001', 'a1000000-0000-0000-0000-000000000001', 'e1000000-0000-0000-0000-000000000001', NOW() - INTERVAL '24 days', '9ac64087-c765-4022-92b6-f740ba312566', 'CZK', 938.00, NOW() - INTERVAL '23 days', 'Completed');

-- =============================================
-- SETTLEMENT LINES
-- =============================================
INSERT INTO "SettlementLines" ("Id", "TotalCoffeeGrams", "TotalMilkMl", "AmountDue", "Currency", "CreatedAt", "ConfirmedAt", "ConfirmedBy", "UserId", "SettlementId")
VALUES
  ('81000000-0000-0000-0000-000000000001', 148, 0, 385.50, 'CZK', NOW() - INTERVAL '24 days', NOW() - INTERVAL '23 days', '9ac64087-c765-4022-92b6-f740ba312566', '9ac64087-c765-4022-92b6-f740ba312566', '71000000-0000-0000-0000-000000000001'),
  ('81000000-0000-0000-0000-000000000002', 96, 0, 304.20, 'CZK', NOW() - INTERVAL '24 days', NOW() - INTERVAL '23 days', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', '71000000-0000-0000-0000-000000000001'),
  ('81000000-0000-0000-0000-000000000003', 84, 0, 248.30, 'CZK', NOW() - INTERVAL '24 days', NOW() - INTERVAL '23 days', 'bf7a269c-3ad2-42c4-9904-25ba51702ef3', 'bf7a269c-3ad2-42c4-9904-25ba51702ef3', '71000000-0000-0000-0000-000000000001');

-- =============================================
-- GLOBAL PRESETS
-- =============================================
INSERT INTO "GlobalPresets" ("Id", "Name", "DefaultCoffeeType", "DefaultPreparation", "DefaultGrams", "Description", "DisplayOrder", "IsActive", "CreatedAt")
VALUES
  ('91000000-0000-0000-0000-000000000001', 'Single Espresso', 'Espresso', 'Espresso Machine', 7, 'Classic single shot espresso', 1, true, NOW() - INTERVAL '60 days'),
  ('91000000-0000-0000-0000-000000000002', 'Double Espresso', 'Espresso', 'Espresso Machine', 14, 'Double shot espresso', 2, true, NOW() - INTERVAL '60 days'),
  ('91000000-0000-0000-0000-000000000003', 'Pour Over', 'Filter', 'Pour Over', 15, 'V60 or Chemex pour over', 3, true, NOW() - INTERVAL '60 days'),
  ('91000000-0000-0000-0000-000000000004', 'French Press', 'Filter', 'French Press', 20, 'Full immersion brewing', 4, true, NOW() - INTERVAL '60 days'),
  ('91000000-0000-0000-0000-000000000005', 'Cappuccino', 'Espresso', 'Espresso Machine', 14, 'Espresso with steamed milk foam', 5, true, NOW() - INTERVAL '60 days'),
  ('91000000-0000-0000-0000-000000000006', 'Latte', 'Espresso', 'Espresso Machine', 14, 'Espresso with steamed milk', 6, true, NOW() - INTERVAL '60 days'),
  ('91000000-0000-0000-0000-000000000007', 'Americano', 'Espresso', 'Espresso Machine', 14, 'Espresso diluted with hot water', 7, true, NOW() - INTERVAL '60 days'),
  ('91000000-0000-0000-0000-000000000008', 'Cold Brew', 'Specialty', 'Cold Brew', 25, 'Cold steeped for 12-24 hours', 8, true, NOW() - INTERVAL '60 days'),
  ('91000000-0000-0000-0000-000000000009', 'Turkish Coffee', 'Espresso', 'Turkish', 10, 'Finely ground, unfiltered', 9, true, NOW() - INTERVAL '60 days'),
  ('91000000-0000-0000-0000-000000000010', 'Instant Coffee', 'Instant', 'Instant', 2, 'Quick instant coffee', 10, true, NOW() - INTERVAL '60 days');

-- =============================================
-- SPACE GLOBAL PRESET CONFIGS
-- =============================================
INSERT INTO "SpaceGlobalPresetConfigs" ("Id", "SpaceId", "GlobalPresetId", "IsEnabled", "CreatedAt", "UpdatedAt")
VALUES
  ('a2000000-0000-0000-0000-000000000001', 'a1000000-0000-0000-0000-000000000001', '91000000-0000-0000-0000-000000000001', true, NOW() - INTERVAL '50 days', NULL),
  ('a2000000-0000-0000-0000-000000000002', 'a1000000-0000-0000-0000-000000000001', '91000000-0000-0000-0000-000000000002', true, NOW() - INTERVAL '50 days', NULL),
  ('a2000000-0000-0000-0000-000000000003', 'a1000000-0000-0000-0000-000000000001', '91000000-0000-0000-0000-000000000003', true, NOW() - INTERVAL '50 days', NULL),
  ('a2000000-0000-0000-0000-000000000004', 'a1000000-0000-0000-0000-000000000001', '91000000-0000-0000-0000-000000000004', true, NOW() - INTERVAL '50 days', NULL),
  ('a2000000-0000-0000-0000-000000000005', 'a1000000-0000-0000-0000-000000000001', '91000000-0000-0000-0000-000000000005', true, NOW() - INTERVAL '50 days', NULL),
  ('a2000000-0000-0000-0000-000000000006', 'a1000000-0000-0000-0000-000000000002', '91000000-0000-0000-0000-000000000001', true, NOW() - INTERVAL '35 days', NULL),
  ('a2000000-0000-0000-0000-000000000007', 'a1000000-0000-0000-0000-000000000002', '91000000-0000-0000-0000-000000000003', true, NOW() - INTERVAL '35 days', NULL),
  ('a2000000-0000-0000-0000-000000000008', 'a1000000-0000-0000-0000-000000000003', '91000000-0000-0000-0000-000000000002', true, NOW() - INTERVAL '18 days', NULL),
  ('a2000000-0000-0000-0000-000000000009', 'a1000000-0000-0000-0000-000000000003', '91000000-0000-0000-0000-000000000008', true, NOW() - INTERVAL '18 days', NULL);

-- =============================================
-- PRESET RECIPES (user custom presets)
-- =============================================
INSERT INTO "PresetRecipes" ("Id", "UserId", "SpaceId", "Name", "CoffeeType", "Preparation", "DefaultGrams", "Notes", "IsShared", "CreatedAt", "LastUsedAt", "UsageCount")
VALUES
  ('a3000000-0000-0000-0000-000000000001', '9ac64087-c765-4022-92b6-f740ba312566', 'a1000000-0000-0000-0000-000000000001', 'My Morning Ritual', 'Specialty', 'Pour Over', 18, 'V60 with Ethiopia beans, 93C water', true, NOW() - INTERVAL '40 days', NOW() - INTERVAL '2 days', 28),
  ('a3000000-0000-0000-0000-000000000002', '9ac64087-c765-4022-92b6-f740ba312566', 'a1000000-0000-0000-0000-000000000001', 'Quick Office Shot', 'Espresso', 'Espresso Machine', 14, 'Double shot, quick morning fix', false, NOW() - INTERVAL '35 days', NOW() - INTERVAL '4 days', 15),
  ('a3000000-0000-0000-0000-000000000003', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', 'a1000000-0000-0000-0000-000000000001', 'Alice Latte', 'Espresso', 'Espresso Machine', 14, 'Double shot with oat milk', true, NOW() - INTERVAL '30 days', NOW() - INTERVAL '3 days', 22),
  ('a3000000-0000-0000-0000-000000000004', 'bf7a269c-3ad2-42c4-9904-25ba51702ef3', 'a1000000-0000-0000-0000-000000000001', 'Bob Filter Brew', 'Filter', 'French Press', 20, 'Coarse grind, 4 min steep', true, NOW() - INTERVAL '25 days', NOW() - INTERVAL '1 day', 18),
  ('a3000000-0000-0000-0000-000000000005', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', 'a1000000-0000-0000-0000-000000000002', 'Weekend V60', 'Specialty', 'Pour Over', 15, 'Slow weekend brew with Kenya beans', false, NOW() - INTERVAL '20 days', NOW() - INTERVAL '6 days', 8);

-- =============================================
-- USER PRESET FAVORITES
-- =============================================
INSERT INTO "UserPresetFavorites" ("Id", "UserId", "SpaceId", "GlobalPresetId", "PresetRecipeId", "DisplayOrder", "CreatedAt")
VALUES
  ('a4000000-0000-0000-0000-000000000001', '9ac64087-c765-4022-92b6-f740ba312566', 'a1000000-0000-0000-0000-000000000001', NULL, 'a3000000-0000-0000-0000-000000000001', 1, NOW() - INTERVAL '38 days'),
  ('a4000000-0000-0000-0000-000000000002', '9ac64087-c765-4022-92b6-f740ba312566', 'a1000000-0000-0000-0000-000000000001', '91000000-0000-0000-0000-000000000002', NULL, 2, NOW() - INTERVAL '38 days'),
  ('a4000000-0000-0000-0000-000000000003', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', 'a1000000-0000-0000-0000-000000000001', NULL, 'a3000000-0000-0000-0000-000000000003', 1, NOW() - INTERVAL '28 days'),
  ('a4000000-0000-0000-0000-000000000004', 'bf7a269c-3ad2-42c4-9904-25ba51702ef3', 'a1000000-0000-0000-0000-000000000001', NULL, 'a3000000-0000-0000-0000-000000000004', 1, NOW() - INTERVAL '23 days');

-- =============================================
-- NOTIFICATIONS
-- =============================================
INSERT INTO "Notifications" ("Id", "UserId", "Type", "Title", "Message", "SpaceId", "IsRead", "CreatedAt", "ReadAt", "ActionUrl", "MetadataJson")
VALUES
  ('a5000000-0000-0000-0000-000000000001', '9ac64087-c765-4022-92b6-f740ba312566', 'MemberJoined', 'New Member!', 'Alice Johnson joined Office Coffee Club', 'a1000000-0000-0000-0000-000000000001', true, NOW() - INTERVAL '50 days', NOW() - INTERVAL '49 days', NULL, NULL),
  ('a5000000-0000-0000-0000-000000000002', '9ac64087-c765-4022-92b6-f740ba312566', 'MemberJoined', 'New Member!', 'Bob Smith joined Office Coffee Club', 'a1000000-0000-0000-0000-000000000001', true, NOW() - INTERVAL '48 days', NOW() - INTERVAL '47 days', NULL, NULL),
  ('a5000000-0000-0000-0000-000000000003', '9ac64087-c765-4022-92b6-f740ba312566', 'SettlementReady', 'Settlement Ready', 'January 2026 settlement generated for Office Coffee Club', 'a1000000-0000-0000-0000-000000000001', true, NOW() - INTERVAL '24 days', NOW() - INTERVAL '23 days', NULL, '{"settlementId": "71000000-0000-0000-0000-000000000001"}'),
  ('a5000000-0000-0000-0000-000000000004', '9ac64087-c765-4022-92b6-f740ba312566', 'SettlementReady', 'Settlement Completed', 'January 2026 settlement has been completed', 'a1000000-0000-0000-0000-000000000001', true, NOW() - INTERVAL '23 days', NOW() - INTERVAL '22 days', NULL, NULL),
  ('a5000000-0000-0000-0000-000000000005', '9ac64087-c765-4022-92b6-f740ba312566', 'BillingPeriodClosed', 'Period Closed', 'February 2026 (Week 1-2) billing period closed in Office Coffee Club', 'a1000000-0000-0000-0000-000000000001', false, NOW() - INTERVAL '5 days', NULL, NULL, NULL),
  ('a5000000-0000-0000-0000-000000000006', '9ac64087-c765-4022-92b6-f740ba312566', 'LowStock', 'Low Stock Alert', 'Colombia Supremo is running low (120g remaining) in Office Coffee Club', 'a1000000-0000-0000-0000-000000000001', false, NOW() - INTERVAL '3 days', NULL, NULL, '{"productName": "Colombia Supremo", "remainingGrams": 120}'),
  ('a5000000-0000-0000-0000-000000000007', '9ac64087-c765-4022-92b6-f740ba312566', 'ConsumptionRecorded', 'Coffee Logged', 'Bob Smith logged a coffee in Office Coffee Club', 'a1000000-0000-0000-0000-000000000001', false, NOW() - INTERVAL '1 day', NULL, NULL, NULL),
  ('a5000000-0000-0000-0000-000000000008', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', 'MemberJoined', 'Welcome!', 'You joined Office Coffee Club', 'a1000000-0000-0000-0000-000000000001', true, NOW() - INTERVAL '50 days', NOW() - INTERVAL '50 days', NULL, NULL),
  ('a5000000-0000-0000-0000-000000000009', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', 'SettlementReady', 'Settlement Ready', 'January 2026 settlement generated for Office Coffee Club', 'a1000000-0000-0000-0000-000000000001', true, NOW() - INTERVAL '24 days', NOW() - INTERVAL '24 days', NULL, NULL),
  ('a5000000-0000-0000-0000-000000000010', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', 'NewCoffeeAdded', 'New Coffee Added', 'Test User added Decaf Swiss Water to Office Coffee Club', 'a1000000-0000-0000-0000-000000000001', false, NOW() - INTERVAL '20 days', NULL, NULL, NULL),
  ('a5000000-0000-0000-0000-000000000011', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', 'BillingPeriodClosed', 'Period Closed', 'February billing period closed in Office Coffee Club', 'a1000000-0000-0000-0000-000000000001', false, NOW() - INTERVAL '5 days', NULL, NULL, NULL),
  ('a5000000-0000-0000-0000-000000000012', '23f2a7df-4f3e-43ff-8fff-8ff400816cad', 'ConsumptionRecorded', 'Coffee Logged', 'Test User logged a coffee in Home Brew Squad', 'a1000000-0000-0000-0000-000000000002', false, NOW() - INTERVAL '2 days', NULL, NULL, NULL),
  ('a5000000-0000-0000-0000-000000000013', 'bf7a269c-3ad2-42c4-9904-25ba51702ef3', 'MemberJoined', 'Welcome!', 'You joined Office Coffee Club', 'a1000000-0000-0000-0000-000000000001', true, NOW() - INTERVAL '48 days', NOW() - INTERVAL '48 days', NULL, NULL),
  ('a5000000-0000-0000-0000-000000000014', 'bf7a269c-3ad2-42c4-9904-25ba51702ef3', 'SettlementReady', 'Settlement Ready', 'January settlement generated', 'a1000000-0000-0000-0000-000000000001', false, NOW() - INTERVAL '24 days', NULL, NULL, NULL),
  ('a5000000-0000-0000-0000-000000000015', 'bf7a269c-3ad2-42c4-9904-25ba51702ef3', 'LowStock', 'Low Stock', 'Dark Roast Blend running low in Weekend Baristas', 'a1000000-0000-0000-0000-000000000003', false, NOW() - INTERVAL '4 days', NULL, NULL, '{"productName": "Dark Roast Blend", "remainingGrams": 310}');

-- =============================================
-- EXCHANGE RATES
-- =============================================
INSERT INTO "ExchangeRates" ("Id", "BaseCurrencyCode", "TargetCurrencyCode", "Rate", "FetchedAt")
VALUES
  ('a6000000-0000-0000-0000-000000000001', 'USD', 'CZK', 23.45, NOW() - INTERVAL '1 hour'),
  ('a6000000-0000-0000-0000-000000000002', 'USD', 'EUR', 0.92, NOW() - INTERVAL '1 hour'),
  ('a6000000-0000-0000-0000-000000000003', 'EUR', 'CZK', 25.48, NOW() - INTERVAL '1 hour'),
  ('a6000000-0000-0000-0000-000000000004', 'EUR', 'USD', 1.09, NOW() - INTERVAL '1 hour'),
  ('a6000000-0000-0000-0000-000000000005', 'CZK', 'USD', 0.043, NOW() - INTERVAL '1 hour'),
  ('a6000000-0000-0000-0000-000000000006', 'CZK', 'EUR', 0.039, NOW() - INTERVAL '1 hour');

-- Verify counts
SELECT 'Users' as entity, COUNT(*) as count FROM "Users"
UNION ALL SELECT 'Spaces', COUNT(*) FROM "Spaces"
UNION ALL SELECT 'SpaceMemberships', COUNT(*) FROM "SpaceMemberships"
UNION ALL SELECT 'CoffeeStocks', COUNT(*) FROM "CoffeeStocks"
UNION ALL SELECT 'Purchases', COUNT(*) FROM "Purchases"
UNION ALL SELECT 'StockLevels', COUNT(*) FROM "StockLevels"
UNION ALL SELECT 'Consumptions', COUNT(*) FROM "Consumptions"
UNION ALL SELECT 'BillingPeriods', COUNT(*) FROM "BillingPeriods"
UNION ALL SELECT 'Settlements', COUNT(*) FROM "Settlements"
UNION ALL SELECT 'SettlementLines', COUNT(*) FROM "SettlementLines"
UNION ALL SELECT 'GlobalPresets', COUNT(*) FROM "GlobalPresets"
UNION ALL SELECT 'PresetRecipes', COUNT(*) FROM "PresetRecipes"
UNION ALL SELECT 'Notifications', COUNT(*) FROM "Notifications"
UNION ALL SELECT 'ExchangeRates', COUNT(*) FROM "ExchangeRates"
ORDER BY entity;
