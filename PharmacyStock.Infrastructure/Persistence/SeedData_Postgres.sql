-- =========================
-- Categories
-- =========================
INSERT INTO "Categories" ("Id", "Name", "Description", "IsActive", "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy")
VALUES
(1, 'Tablets', 'Solid dosage forms', true, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM'),
(2, 'Syrups', 'Liquid dosage forms', true, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM')
ON CONFLICT ("Id") DO NOTHING;

-- =========================
-- Suppliers
-- =========================
INSERT INTO "Suppliers" ("Id", "SupplierCode", "Name", "ContactInfo", "IsActive", "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy")
VALUES
(1, 'SUP001', 'MedSupply Co', 'medsupply@example.com | +1-555-0101', true, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM'),
(2, 'SUP002', 'HealthPlus Distributors', 'healthplus@example.com | +1-555-0102', true, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM'),
(3, 'SUP003', 'CareWell Pharma', 'carewell@example.com | +1-555-0103', true, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM'),
(4, 'SUP004', 'PrimeMed Suppliers', 'primemed@example.com | +1-555-0104', true, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM'),
(5, 'SUP005', 'LifeLine Medicals', 'lifeline@example.com | +1-555-0105', true, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM')
ON CONFLICT ("Id") DO NOTHING;

-- =========================
-- ExpiryRules
-- =========================
INSERT INTO "ExpiryRules" ("Id", "CategoryId", "WarningDays", "CriticalDays", "IsActive", "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy")
VALUES
(1, NULL, 90, 30, true, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM'),
(2, 1, 180, 60, true, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM'),
(3, 2, 60, 15, true, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM')
ON CONFLICT ("Id") DO NOTHING;

-- =========================
-- Medicines
-- =========================
INSERT INTO "Medicines" (
    "Id", "CategoryId", "MedicineCode", "Name", "GenericName", "Manufacturer",
    "StorageCondition", "UnitOfMeasure", "LowStockThreshold",
    "IsActive", "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy"
)
VALUES
(1, 1, 'TAB001', 'Paracetamol 500', 'Paracetamol', 'ABC Pharma', 'Store below 30°C', 'Strip', 50, true, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM'),
(2, 1, 'TAB002', 'Calpol 500', 'Paracetamol', 'GSK', 'Store below 30°C', 'Strip', 50, true, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM'),
(3, 2, 'SYP001', 'Paracetamol Syrup', 'Paracetamol', 'Cipla', 'Do not refrigerate', 'Bottle', 20, true, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM'),
(4, 1, 'TAB003', 'Amoxil 250', 'Amoxicillin', 'Pfizer', 'Store in a cool dry place', 'Strip', 50, true, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM'),
(5, 1, 'TAB004', 'Mox 250', 'Amoxicillin', 'Sun Pharma', 'Store below 25°C', 'Strip', 50, true, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM'),
(6, 1, 'TAB005', 'Cetzine 10', 'Cetirizine', 'GSK', 'Store below 25°C', 'Strip', 50, true, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM'),
(7, 2, 'SYP002', 'Cetirizine Syrup', 'Cetirizine', 'Dr Reddy''s', 'Store below 30°C', 'Bottle', 20, true, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM'),
(8, 1, 'TAB006', 'Brufen 400', 'Ibuprofen', 'Abbott', 'Store below 30°C', 'Strip', 50, true, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM'),
(9, 1, 'TAB007', 'Ibugesic 400', 'Ibuprofen', 'Cipla', 'Store below 30°C', 'Strip', 50, true, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM'),
(10, 2, 'SYP003', 'Ibuprofen Syrup', 'Ibuprofen', 'Abbott', 'Store below 25°C', 'Bottle', 20, true, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM'),
(11, 1, 'TAB008', 'Azee 500', 'Azithromycin', 'Cipla', 'Store below 30°C', 'Strip', 50, true, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM'),
(12, 1, 'TAB009', 'Zithro 500', 'Azithromycin', 'FDC Ltd', 'Store in a cool dry place', 'Strip', 50, true, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM'),
(13, 2, 'SYP004', 'Azithromycin Syrup', 'Azithromycin', 'Pfizer', 'Store below 25°C', 'Bottle', 20, true, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM')
ON CONFLICT ("Id") DO NOTHING;
