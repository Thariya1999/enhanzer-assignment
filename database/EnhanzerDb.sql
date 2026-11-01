/* Enhanzer assignment - SQL Server script
   Run in SSMS / Azure Data Studio / VS Code (SQL Server extension).
   Optional: the API also creates this schema automatically on first start. */

IF DB_ID(N'EnhanzerDb') IS NULL
    CREATE DATABASE EnhanzerDb;
GO

USE EnhanzerDb;
GO

IF OBJECT_ID(N'dbo.Location_Details', N'U') IS NULL
CREATE TABLE dbo.Location_Details (
    Id             INT IDENTITY(1,1) CONSTRAINT PK_Location_Details PRIMARY KEY,
    Location_Code  NVARCHAR(100) NOT NULL,
    Location_Name  NVARCHAR(250) NOT NULL,
    UpdatedAt      DATETIME2     NOT NULL CONSTRAINT DF_Location_Details_UpdatedAt DEFAULT SYSUTCDATETIME()
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Location_Details_Location_Code')
    CREATE UNIQUE INDEX IX_Location_Details_Location_Code ON dbo.Location_Details (Location_Code);
GO

IF OBJECT_ID(N'dbo.Purchase_Bills', N'U') IS NULL
CREATE TABLE dbo.Purchase_Bills (
    Id            INT IDENTITY(1,1) CONSTRAINT PK_Purchase_Bills PRIMARY KEY,
    CreatedBy     NVARCHAR(256)  NOT NULL,
    CreatedAt     DATETIME2      NOT NULL,
    TotalItems    INT            NOT NULL,
    TotalQty      INT            NOT NULL,
    TotalCost     DECIMAL(18,2)  NOT NULL,
    TotalSelling  DECIMAL(18,2)  NOT NULL
);
GO

IF OBJECT_ID(N'dbo.Purchase_Bill_Items', N'U') IS NULL
CREATE TABLE dbo.Purchase_Bill_Items (
    Id               INT IDENTITY(1,1) CONSTRAINT PK_Purchase_Bill_Items PRIMARY KEY,
    PurchaseBillId   INT            NOT NULL
        CONSTRAINT FK_Purchase_Bill_Items_Bills REFERENCES dbo.Purchase_Bills (Id) ON DELETE CASCADE,
    Item             NVARCHAR(100)  NOT NULL,
    Batch            NVARCHAR(250)  NOT NULL,
    StandardCost     DECIMAL(18,2)  NOT NULL,
    StandardPrice    DECIMAL(18,2)  NOT NULL,
    Margin           DECIMAL(18,2)  NOT NULL,
    Qty              INT            NOT NULL,
    FreeQty          INT            NOT NULL,
    DiscountPercent  DECIMAL(5,2)   NOT NULL,
    TotalCost        DECIMAL(18,2)  NOT NULL,
    TotalSelling     DECIMAL(18,2)  NOT NULL
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Purchase_Bill_Items_PurchaseBillId')
    CREATE INDEX IX_Purchase_Bill_Items_PurchaseBillId ON dbo.Purchase_Bill_Items (PurchaseBillId);
GO
