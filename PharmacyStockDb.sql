-- 1. Create the Database
CREATE DATABASE PharmacyStockDb;
GO

-- 2. Select the Database to ensure subsequent scripts run inside it
USE PharmacyStockDb;
GO

-- Create Users table with explicit constraint names
CREATE TABLE Users (
    Id INT IDENTITY(1,1) NOT NULL,
    Username NVARCHAR(50) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL, -- e.g., 'Admin', 'Pharmacist'
    
    -- Base Fields
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(50) NULL, -- Stores Username of creator
    UpdatedAt DATETIME2 NULL,
    UpdatedBy NVARCHAR(50) NULL,
    
    -- Explicitly named constraints
    CONSTRAINT PK_Users PRIMARY KEY (Id),
    CONSTRAINT UQ_Users_Username UNIQUE (Username)
);

-- Create Categories table with explicit constraint names
CREATE TABLE Categories (
    Id INT IDENTITY(1,1) NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255) NULL,
    
    -- Base Fields
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(50) NULL,
    UpdatedAt DATETIME2 NULL,
    UpdatedBy NVARCHAR(50) NULL,
    
    -- Explicitly named constraints
    CONSTRAINT PK_Categories PRIMARY KEY (Id),
    CONSTRAINT UQ_Categories_Name UNIQUE (Name)
);

-- Create Suppliers table with explicit constraint names
CREATE TABLE Suppliers (
    Id INT IDENTITY(1,1) NOT NULL,
    SupplierCode NVARCHAR(50) NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    ContactInfo NVARCHAR(255) NULL, -- Email or Phone combined
    
    -- Base Fields
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(50) NULL,
    UpdatedAt DATETIME2 NULL,
    UpdatedBy NVARCHAR(50) NULL,
    
    -- Explicitly named constraints
    CONSTRAINT PK_Suppliers PRIMARY KEY (Id),
    CONSTRAINT UQ_Suppliers_SupplierCode UNIQUE (SupplierCode)
);

-- Create Medicines table with explicit constraint names
CREATE TABLE Medicines (
    Id INT IDENTITY(1,1) NOT NULL,
    CategoryId INT NOT NULL,
    MedicineCode NVARCHAR(50) NOT NULL,
    Name NVARCHAR(100) NOT NULL, -- Brand Name
    GenericName NVARCHAR(100) NULL, -- Chemical Name for substitution search
    Manufacturer NVARCHAR(100) NULL,
    StorageCondition NVARCHAR(100) NULL, -- e.g., 'Refrigerate below 5C'
    UnitOfMeasure NVARCHAR(20) NOT NULL, -- e.g., 'Box', 'Strip'
    
    -- Base Fields
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(50) NULL,
    UpdatedAt DATETIME2 NULL,
    UpdatedBy NVARCHAR(50) NULL,
    
    -- Explicitly named constraints
    CONSTRAINT PK_Medicines PRIMARY KEY (Id),
    CONSTRAINT UQ_Medicines_MedicineCode UNIQUE (MedicineCode),
    CONSTRAINT FK_Medicines_Categories FOREIGN KEY (CategoryId) 
        REFERENCES Categories(Id)
);

-- Create ExpiryRules table with explicit constraint names
CREATE TABLE ExpiryRules (
    Id INT IDENTITY(1,1) NOT NULL,
    CategoryId INT NULL, -- Nullable. If Null, this is the "Global Default" rule.
    WarningDays INT NOT NULL, -- Days before expiry to show Yellow alert
    CriticalDays INT NOT NULL, -- Days before expiry to show Red alert
    
    -- Base Fields
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(50) NULL,
    UpdatedAt DATETIME2 NULL,
    UpdatedBy NVARCHAR(50) NULL,
    
    -- Explicitly named constraints
    CONSTRAINT PK_ExpiryRules PRIMARY KEY (Id),
    CONSTRAINT FK_ExpiryRules_Categories FOREIGN KEY (CategoryId) 
        REFERENCES Categories(Id)
);

-- Create MedicineBatches table with explicit constraint names
CREATE TABLE MedicineBatches (
    Id INT IDENTITY(1,1) NOT NULL,
    MedicineId INT NOT NULL,
    SupplierId INT NOT NULL,
    BatchNumber NVARCHAR(50) NOT NULL,
    ExpiryDate DATE NOT NULL, -- Crucial for expiry logic
    ReceivedDate DATE NOT NULL DEFAULT GETDATE(),
    
    InitialQuantity INT NOT NULL, -- How much arrived
    CurrentQuantity INT NOT NULL, -- How much is left
    Status NVARCHAR(20) NOT NULL, -- 'Active', 'Expired', 'Depleted', 'OnHold'
    
    -- Concurrency Control
    -- Automatically updates on every edit. EF Core uses this to detect conflicts.
    RowVersion TIMESTAMP NOT NULL, 

    -- Base Fields
    IsActive BIT NOT NULL DEFAULT 1, -- Soft delete entire batch
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(50) NULL,
    UpdatedAt DATETIME2 NULL,
    UpdatedBy NVARCHAR(50) NULL,
    
    -- Explicitly named constraints
    CONSTRAINT PK_MedicineBatches PRIMARY KEY (Id),
    CONSTRAINT FK_MedicineBatches_Medicines FOREIGN KEY (MedicineId) 
        REFERENCES Medicines(Id),
    CONSTRAINT FK_MedicineBatches_Suppliers FOREIGN KEY (SupplierId) 
        REFERENCES Suppliers(Id)
);

-- Index for performance on the dashboard
CREATE INDEX IX_MedicineBatches_ExpiryDate_Status 
ON MedicineBatches(ExpiryDate, Status);

-- Create StockMovements table with explicit constraint names
CREATE TABLE StockMovements (
    Id INT IDENTITY(1,1) NOT NULL,
    MedicineBatchId INT NOT NULL,
    PerformedByUserId INT NOT NULL, -- Links to specific user who clicked the button
    
    MovementType NVARCHAR(20) NOT NULL, -- 'IN', 'OUT', 'ADJUSTMENT', 'EXPIRED'
    Quantity INT NOT NULL, -- Positive (IN) or Negative (OUT)
    Reason NVARCHAR(255) NULL, -- Mandatory for Adjustments
    ReferenceNo NVARCHAR(50) NULL, -- External Invoice or Order ID
    SnapshotQuantity INT NULL, -- The balance AFTER this move
    
    -- Immutable Log Fields (No Update fields here)
    PerformedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    
    -- Explicitly named constraints
    CONSTRAINT PK_StockMovements PRIMARY KEY (Id),
    CONSTRAINT FK_StockMovements_MedicineBatches FOREIGN KEY (MedicineBatchId) 
        REFERENCES MedicineBatches(Id),
    CONSTRAINT FK_StockMovements_Users FOREIGN KEY (PerformedByUserId) 
        REFERENCES Users(Id)
);