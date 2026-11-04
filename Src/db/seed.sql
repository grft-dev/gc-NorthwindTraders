IF DB_ID(N'Northwind') IS NULL
BEGIN
    CREATE DATABASE [Northwind];
END
GO

USE [Northwind];
GO

IF OBJECT_ID(N'dbo.Customers', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Customers]
    (
        [CustomerID]   NVARCHAR(5)   NOT NULL,
        [CompanyName]  NVARCHAR(40)  NOT NULL,
        [ContactName]  NVARCHAR(30)  NULL,
        [ContactTitle] NVARCHAR(30)  NULL,
        [Address]      NVARCHAR(60)  NULL,
        [City]         NVARCHAR(15)  NULL,
        [Region]       NVARCHAR(15)  NULL,
        [PostalCode]   NVARCHAR(10)  NULL,
        [Country]      NVARCHAR(15)  NULL,
        [Phone]        NVARCHAR(24)  NULL,
        [Fax]          NVARCHAR(24)  NULL,
        CONSTRAINT [PK_Customers] PRIMARY KEY CLUSTERED ([CustomerID] ASC)
    );
END
GO

MERGE [dbo].[Customers] AS target
USING
(
    SELECT
        N'ALFKI' AS [CustomerID],
        N'Alfreds Futterkiste' AS [CompanyName],
        N'Maria Anders' AS [ContactName],
        N'Sales Representative' AS [ContactTitle],
        N'Obere Str. 57' AS [Address],
        N'Berlin' AS [City],
        CAST(NULL AS NVARCHAR(15)) AS [Region],
        N'12209' AS [PostalCode],
        N'Germany' AS [Country],
        N'030-0074321' AS [Phone],
        N'030-0076545' AS [Fax]
    UNION ALL
    SELECT
        N'ANATR',
        N'Ana Trujillo Emparedados y helados',
        N'Ana Trujillo',
        N'Owner',
        N'Avda. de la Constitucion 2222',
        N'Mexico D.F.',
        CAST(NULL AS NVARCHAR(15)),
        N'05021',
        N'Mexico',
        N'(5) 555-4729',
        N'(5) 555-3745'
) AS source
ON target.[CustomerID] = source.[CustomerID]
WHEN MATCHED THEN
    UPDATE SET
        [CompanyName] = source.[CompanyName],
        [ContactName] = source.[ContactName],
        [ContactTitle] = source.[ContactTitle],
        [Address] = source.[Address],
        [City] = source.[City],
        [Region] = source.[Region],
        [PostalCode] = source.[PostalCode],
        [Country] = source.[Country],
        [Phone] = source.[Phone],
        [Fax] = source.[Fax]
WHEN NOT MATCHED THEN
    INSERT
    (
        [CustomerID],
        [CompanyName],
        [ContactName],
        [ContactTitle],
        [Address],
        [City],
        [Region],
        [PostalCode],
        [Country],
        [Phone],
        [Fax]
    )
    VALUES
    (
        source.[CustomerID],
        source.[CompanyName],
        source.[ContactName],
        source.[ContactTitle],
        source.[Address],
        source.[City],
        source.[Region],
        source.[PostalCode],
        source.[Country],
        source.[Phone],
        source.[Fax]
    );
GO
