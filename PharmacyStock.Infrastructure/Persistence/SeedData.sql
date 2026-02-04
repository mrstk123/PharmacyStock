SET IDENTITY_INSERT Categories ON;
GO
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Id = 1)
INSERT INTO Categories (Id, Name, Description, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy) VALUES (1, 'Tablets', 'Solid dosage forms', 1, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM');
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Id = 2)
INSERT INTO Categories (Id, Name, Description, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy) VALUES (2, 'Syrups', 'Liquid dosage forms', 1, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM');
GO
SET IDENTITY_INSERT Categories OFF;
GO

SET IDENTITY_INSERT Suppliers ON;
GO
IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE Id = 1)
INSERT INTO Suppliers (Id, SupplierCode, Name, ContactInfo, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy) VALUES (1, 'SUP001', 'MedSupply Co', 'medsupply@example.com | +1-555-0101', 1, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM');
IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE Id = 2)
INSERT INTO Suppliers (Id, SupplierCode, Name, ContactInfo, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy) VALUES (2, 'SUP002', 'HealthPlus Distributors', 'healthplus@example.com | +1-555-0102', 1, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM');
IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE Id = 3)
INSERT INTO Suppliers (Id, SupplierCode, Name, ContactInfo, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy) VALUES (3, 'SUP003', 'CareWell Pharma', 'carewell@example.com | +1-555-0103', 1, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM');
IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE Id = 4)
INSERT INTO Suppliers (Id, SupplierCode, Name, ContactInfo, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy) VALUES (4, 'SUP004', 'PrimeMed Suppliers', 'primemed@example.com | +1-555-0104', 1, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM');
IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE Id = 5)
INSERT INTO Suppliers (Id, SupplierCode, Name, ContactInfo, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy) VALUES (5, 'SUP005', 'LifeLine Medicals', 'lifeline@example.com | +1-555-0105', 1, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM');
GO
SET IDENTITY_INSERT Suppliers OFF;
GO

SET IDENTITY_INSERT ExpiryRules ON;
GO
IF NOT EXISTS (SELECT 1 FROM ExpiryRules WHERE Id = 1)
INSERT INTO ExpiryRules (Id, CategoryId, WarningDays, CriticalDays, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy) VALUES (1, NULL, 90, 30, 1, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM');
IF NOT EXISTS (SELECT 1 FROM ExpiryRules WHERE Id = 2)
INSERT INTO ExpiryRules (Id, CategoryId, WarningDays, CriticalDays, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy) VALUES (2, 1, 180, 60, 1, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM');
IF NOT EXISTS (SELECT 1 FROM ExpiryRules WHERE Id = 3)
INSERT INTO ExpiryRules (Id, CategoryId, WarningDays, CriticalDays, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy) VALUES (3, 2, 60, 15, 1, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM');
GO
SET IDENTITY_INSERT ExpiryRules OFF;
GO

SET IDENTITY_INSERT Medicines ON;
GO
IF NOT EXISTS (SELECT 1 FROM Medicines WHERE Id = 1)
INSERT INTO Medicines (Id, CategoryId, MedicineCode, Name, GenericName, Manufacturer, StorageCondition, UnitOfMeasure, LowStockThreshold, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy) VALUES (1, 1, 'TAB001', 'Paracetamol 500', 'Paracetamol', 'ABC Pharma', 'Store below 30°C', 'Strip', 50, 1, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM');
IF NOT EXISTS (SELECT 1 FROM Medicines WHERE Id = 2)
INSERT INTO Medicines (Id, CategoryId, MedicineCode, Name, GenericName, Manufacturer, StorageCondition, UnitOfMeasure, LowStockThreshold, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy) VALUES (2, 1, 'TAB002', 'Calpol 500', 'Paracetamol', 'GSK', 'Store below 30°C', 'Strip', 50, 1, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM');
IF NOT EXISTS (SELECT 1 FROM Medicines WHERE Id = 3)
INSERT INTO Medicines (Id, CategoryId, MedicineCode, Name, GenericName, Manufacturer, StorageCondition, UnitOfMeasure, LowStockThreshold, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy) VALUES (3, 2, 'SYP001', 'Paracetamol Syrup', 'Paracetamol', 'Cipla', 'Do not refrigerate', 'Bottle', 20, 1, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM');
IF NOT EXISTS (SELECT 1 FROM Medicines WHERE Id = 4)
INSERT INTO Medicines (Id, CategoryId, MedicineCode, Name, GenericName, Manufacturer, StorageCondition, UnitOfMeasure, LowStockThreshold, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy) VALUES (4, 1, 'TAB003', 'Amoxil 250', 'Amoxicillin', 'Pfizer', 'Store in a cool dry place', 'Strip', 50, 1, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM');
IF NOT EXISTS (SELECT 1 FROM Medicines WHERE Id = 5)
INSERT INTO Medicines (Id, CategoryId, MedicineCode, Name, GenericName, Manufacturer, StorageCondition, UnitOfMeasure, LowStockThreshold, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy) VALUES (5, 1, 'TAB004', 'Mox 250', 'Amoxicillin', 'Sun Pharma', 'Store below 25°C', 'Strip', 50, 1, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM');
IF NOT EXISTS (SELECT 1 FROM Medicines WHERE Id = 6)
INSERT INTO Medicines (Id, CategoryId, MedicineCode, Name, GenericName, Manufacturer, StorageCondition, UnitOfMeasure, LowStockThreshold, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy) VALUES (6, 1, 'TAB005', 'Cetzine 10', 'Cetirizine', 'GSK', 'Store below 25°C', 'Strip', 50, 1, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM');
IF NOT EXISTS (SELECT 1 FROM Medicines WHERE Id = 7)
INSERT INTO Medicines (Id, CategoryId, MedicineCode, Name, GenericName, Manufacturer, StorageCondition, UnitOfMeasure, LowStockThreshold, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy) VALUES (7, 2, 'SYP002', 'Cetirizine Syrup', 'Cetirizine', 'Dr Reddy''s', 'Store below 30°C', 'Bottle', 20, 1, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM');
IF NOT EXISTS (SELECT 1 FROM Medicines WHERE Id = 8)
INSERT INTO Medicines (Id, CategoryId, MedicineCode, Name, GenericName, Manufacturer, StorageCondition, UnitOfMeasure, LowStockThreshold, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy) VALUES (8, 1, 'TAB006', 'Brufen 400', 'Ibuprofen', 'Abbott', 'Store below 30°C', 'Strip', 50, 1, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM');
IF NOT EXISTS (SELECT 1 FROM Medicines WHERE Id = 9)
INSERT INTO Medicines (Id, CategoryId, MedicineCode, Name, GenericName, Manufacturer, StorageCondition, UnitOfMeasure, LowStockThreshold, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy) VALUES (9, 1, 'TAB007', 'Ibugesic 400', 'Ibuprofen', 'Cipla', 'Store below 30°C', 'Strip', 50, 1, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM');
IF NOT EXISTS (SELECT 1 FROM Medicines WHERE Id = 10)
INSERT INTO Medicines (Id, CategoryId, MedicineCode, Name, GenericName, Manufacturer, StorageCondition, UnitOfMeasure, LowStockThreshold, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy) VALUES (10, 2, 'SYP003', 'Ibuprofen Syrup', 'Ibuprofen', 'Abbott', 'Store below 25°C', 'Bottle', 20, 1, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM');
IF NOT EXISTS (SELECT 1 FROM Medicines WHERE Id = 11)
INSERT INTO Medicines (Id, CategoryId, MedicineCode, Name, GenericName, Manufacturer, StorageCondition, UnitOfMeasure, LowStockThreshold, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy) VALUES (11, 1, 'TAB008', 'Azee 500', 'Azithromycin', 'Cipla', 'Store below 30°C', 'Strip', 50, 1, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM');
IF NOT EXISTS (SELECT 1 FROM Medicines WHERE Id = 12)
INSERT INTO Medicines (Id, CategoryId, MedicineCode, Name, GenericName, Manufacturer, StorageCondition, UnitOfMeasure, LowStockThreshold, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy) VALUES (12, 1, 'TAB009', 'Zithro 500', 'Azithromycin', 'FDC Ltd', 'Store in a cool dry place', 'Strip', 50, 1, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM');
IF NOT EXISTS (SELECT 1 FROM Medicines WHERE Id = 13)
INSERT INTO Medicines (Id, CategoryId, MedicineCode, Name, GenericName, Manufacturer, StorageCondition, UnitOfMeasure, LowStockThreshold, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy) VALUES (13, 2, 'SYP004', 'Azithromycin Syrup', 'Azithromycin', 'Pfizer', 'Store below 25°C', 'Bottle', 20, 1, '2026-01-28', 'SYSTEM', '2026-01-28', 'SYSTEM');
GO
SET IDENTITY_INSERT Medicines OFF;
GO
