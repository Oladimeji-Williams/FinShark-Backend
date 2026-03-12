# FinShark Database Schema Documentation

Complete database design, entity relationships, and migrations guide.

## Database Overview

**Database Name**: FinSharkDb
**Type**: SQL Server 2019+
**Schema**: dbo (default)

---

## Entity Relationship Diagram

```
┌─────────────────────────────────────┐
│           Stocks                    │
├─────────────────────────────────────┤
│ Id (PK)                             │
│ Symbol (string, unique)             │
│ CompanyName (string)                │
│ CurrentPrice (decimal)              │
│ Industry (enum)                     │
│ MarketCap (long)                    │
│ CreatedAt (DateTime)                │
│ UpdatedAt (DateTime)                │
│ 1 ──────────────────< * │
│                         │
│                         │ (Comments)
│                         │
│                         ▼
│                      ┌──────────────────────┐
│                      │      Comments        │
│                      ├──────────────────────┤
│                      │ Id (PK)              │
│                      │ StockId (FK)         │
│                      │ Text (string)        │
│                      │ Rating (int, 1-5)    │
│                      │ CreatedAt (DateTime) │
│                      │ UpdatedAt (DateTime) │
│                      └──────────────────────┘
│
└─────────────────────────────────────┘
```

---

## Stocks Table

The central table for stock data.

### Schema

```sql
CREATE TABLE [dbo].[Stocks] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [Symbol] NVARCHAR(10) NOT NULL UNIQUE,
    [CompanyName] NVARCHAR(100) NOT NULL,
    [CurrentPrice] DECIMAL(10, 2) NOT NULL,
    [Industry] INT NOT NULL, -- Enum: 0=Technology, 1=Healthcare, etc.
    [MarketCap] BIGINT NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NULL,
    
    -- Constraints
    CHECK ([CurrentPrice] > 0),
    CHECK ([MarketCap] > 0),
    CHECK (LEN([Symbol]) > 0),
    CHECK (LEN([CompanyName]) > 0)
);

-- Indexes
CREATE INDEX [IX_Stocks_Symbol] ON [dbo].[Stocks]([Symbol]);
CREATE INDEX [IX_Stocks_Industry] ON [dbo].[Stocks]([Industry]);
CREATE INDEX [IX_Stocks_CreatedAt] ON [dbo].[Stocks]([CreatedAt]);
```

### Column Definitions

| Column | Type | Null | Default | Description |
|--------|------|------|---------|-------------|
| Id | int | NO | IDENTITY(1,1) | Primary key, auto-increment |
| Symbol | nvarchar(10) | NO | - | Stock ticker symbol (unique) |
| CompanyName | nvarchar(100) | NO | - | Company legal name |
| CurrentPrice | decimal(10,2) | NO | - | Current stock price |
| Industry | int | NO | - | Industry classification (enum) |
| MarketCap | bigint | NO | - | Market capitalization |
| CreatedAt | datetime2 | NO | GETUTCDATE() | Record creation timestamp |
| UpdatedAt | datetime2 | YES | NULL | Last modification timestamp |

### Sample Data

```sql
SELECT * FROM Stocks;

-- Output:
-- Id | Symbol | CompanyName | CurrentPrice | Industry | MarketCap | CreatedAt | UpdatedAt
-- 1  | AAPL   | Apple Inc. | 250.50       | 0        | 2500000000000 | 2026-03-10 10:30:00 | NULL
-- 2  | MSFT   | Microsoft  | 380.25       | 0        | 2800000000000 | 2026-03-10 10:35:00 | NULL
-- 3  | JPM    | JP Morgan  | 175.80       | 1        | 350000000000  | 2026-03-10 10:40:00 | NULL
```

---

## Comments Table

Contains user comments and ratings for stocks.

### Schema

```sql
CREATE TABLE [dbo].[Comments] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [StockId] INT NOT NULL,
    [Text] NVARCHAR(MAX) NOT NULL,
    [Rating] INT NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NULL,
    
    -- Foreign Key
    CONSTRAINT [FK_Comments_Stocks] 
        FOREIGN KEY ([StockId]) 
        REFERENCES [dbo].[Stocks]([Id]) 
        ON DELETE CASCADE,
    
    -- Constraints
    CHECK ([Rating] >= 1 AND [Rating] <= 5),
    CHECK (LEN([Text]) > 0)
);

-- Indexes
CREATE INDEX [IX_Comments_StockId] ON [dbo].[Comments]([StockId]);
CREATE INDEX [IX_Comments_CreatedAt] ON [dbo].[Comments]([CreatedAt]);
```

### Column Definitions

| Column | Type | Null | Default | Description |
|--------|------|------|---------|-------------|
| Id | int | NO | IDENTITY(1,1) | Primary key |
| StockId | int | NO | - | Foreign key to Stocks |
| Text | nvarchar(max) | NO | - | Comment text |
| Rating | int | NO | - | Rating (1-5) |
| CreatedAt | datetime2 | NO | GETUTCDATE() | Creation timestamp |
| UpdatedAt | datetime2 | YES | NULL | Update timestamp |

### Sample Data

```sql
SELECT * FROM Comments WHERE StockId = 1;

-- Output:
-- Id | StockId | Text | Rating | CreatedAt | UpdatedAt
-- 1  | 1       | Great company | 5 | 2026-03-11 14:20:00 | NULL
-- 2  | 1       | Strong fundamentals | 4 | 2026-03-11 14:25:00 | NULL
```

---

## Industry Enum Values

Stored as integer in database:

```csharp
public enum Industry
{
    Technology = 0,
    Healthcare = 1,
    Finance = 2,
    Energy = 3,
    Consumer = 4,
    Industrial = 5,
    Telecommunications = 6,
    Utilities = 7,
    RealEstate = 8,
    Materials = 9,
    Transportation = 10,
    Retail = 11
}
```

**Database Lookup**:
```sql
-- Get stocks by industry
SELECT * FROM Stocks WHERE Industry = 0; -- Technology stocks
SELECT * FROM Stocks WHERE Industry = 1; -- Healthcare stocks
SELECT * FROM Stocks WHERE Industry = 2; -- Finance stocks
```

---

## Entity Framework Core Configuration

### Stock Entity Configuration

```csharp
// FinShark.Persistence/EntityConfigurations/StockEntityConfiguration.cs
public sealed class StockEntityConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        // Table name
        builder.ToTable("Stocks");

        // Primary key
        builder.HasKey(s => s.Id);

        // Properties
        builder.Property(s => s.Id)
            .ValueGeneratedOnAdd();

        builder.Property(s => s.Symbol)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(s => s.CompanyName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.CurrentPrice)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(s => s.Industry)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(s => s.MarketCap)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(s => s.Symbol).IsUnique();
        builder.HasIndex(s => s.Industry);
        builder.HasIndex(s => s.CreatedAt);

        // Relationships
        builder.HasMany(s => s.Comments)
            .WithOne(c => c.Stock)
            .HasForeignKey(c => c.StockId)
            .OnDelete(DeleteBehavior.Cascade);

        // Check constraints
        builder.HasCheckConstraint("CK_Stocks_Price", "[CurrentPrice] > 0");
        builder.HasCheckConstraint("CK_Stocks_MarketCap", "[MarketCap] > 0");
    }
}
```

### Comment Entity Configuration

```csharp
public sealed class CommentEntityConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.StockId).IsRequired();
        builder.Property(c => c.Text).IsRequired();
        builder.Property(c => c.Rating).IsRequired();
        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .IsRequired();

        // Foreign key
        builder.HasOne(c => c.Stock)
            .WithMany(s => s.Comments)
            .HasForeignKey(c => c.StockId)
            .OnDelete(DeleteBehavior.Cascade);

        // Constraints
        builder.HasCheckConstraint("CK_Comments_Rating", "[Rating] >= 1 AND [Rating] <= 5");
    }
}
```

---

## Migrations

### Creating Migrations

```powershell
# Create a new migration
cd src/FinShark.Persistence
dotnet ef migrations add InitialCreate -o Migrations

# Apply migrations
cd ../FinShark.API
dotnet ef database update
```

### Migration Files

```
FinShark.Persistence/Migrations/
├── 20260310000000_InitialCreate.cs
├── 20260310000000_InitialCreate.Designer.cs
└── AppDbContextModelSnapshot.cs
```

### Example Migration

```csharp
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Stocks",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Symbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                CompanyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                CurrentPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                Industry = table.Column<int>(type: "int", nullable: false),
                MarketCap = table.Column<long>(type: "bigint", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Stocks", x => x.Id);
                table.UniqueConstraint("AK_Stocks_Symbol", x => x.Symbol);
                table.CheckConstraint("CK_Stocks_Price", "[CurrentPrice] > 0");
                table.CheckConstraint("CK_Stocks_MarketCap", "[MarketCap] > 0");
            });

        migrationBuilder.CreateIndex(
            name: "IX_Stocks_Industry",
            table: "Stocks",
            column: "Industry");

        migrationBuilder.CreateIndex(
            name: "IX_Stocks_CreatedAt",
            table: "Stocks",
            column: "CreatedAt");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Stocks");
    }
}
```

---

## Query Examples

### Insert Stock

```sql
INSERT INTO Stocks (Symbol, CompanyName, CurrentPrice, Industry, MarketCap)
VALUES ('AAPL', 'Apple Inc.', 250.50, 0, 2500000000000);
```

### Get All Stocks with Comments

```sql
SELECT 
    s.Id,
    s.Symbol,
    s.CompanyName,
    s.CurrentPrice,
    s.Industry,
    s.MarketCap,
    COUNT(c.Id) as CommentCount,
    AVG(CAST(c.Rating as float)) as AverageRating
FROM Stocks s
LEFT JOIN Comments c ON s.Id = c.StockId
GROUP BY s.Id, s.Symbol, s.CompanyName, s.CurrentPrice, s.Industry, s.MarketCap;
```

### Get Stocks by Industry

```sql
SELECT * FROM Stocks 
WHERE Industry = 0  -- Technology
ORDER BY MarketCap DESC;
```

### Get Recent Comments

```sql
SELECT TOP 10
    c.Id,
    c.StockId,
    c.Text,
    c.Rating,
    s.Symbol,
    c.CreatedAt
FROM Comments c
INNER JOIN Stocks s ON c.StockId = s.Id
ORDER BY c.CreatedAt DESC;
```

### Delete Stock and Comments

```sql
-- Cascade delete happens automatically
DELETE FROM Stocks WHERE Id = 1;
-- All comments for stock 1 are deleted automatically
```

---

## Performance Considerations

### Indexes

```sql
-- Already created indexes
CREATE INDEX IX_Stocks_Symbol ON Stocks(Symbol);
CREATE INDEX IX_Stocks_Industry ON Stocks(Industry);
CREATE INDEX IX_Stocks_CreatedAt ON Stocks(CreatedAt);
CREATE INDEX IX_Comments_StockId ON Comments(StockId);
CREATE INDEX IX_Comments_CreatedAt ON Comments(CreatedAt);
```

### Query Optimization

```csharp
// Use .Include() for eager loading
var stocks = await dbContext.Stocks
    .Include(s => s.Comments)
    .Where(s => s.Industry == Industry.Technology)
    .OrderByDescending(s => s.MarketCap)
    .Take(10)
    .ToListAsync();

// Use .AsNoTracking() for read-only queries
var stocks = await dbContext.Stocks
    .AsNoTracking()
    .ToListAsync();
```

---

## Backup & Recovery

### Backup

```powershell
# Full backup
sqlcmd -S localhost -E -Q "BACKUP DATABASE FinSharkDb TO DISK = 'C:\Backups\FinSharkDb.bak';"

# Transaction log backup
sqlcmd -S localhost -E -Q "BACKUP LOG FinSharkDb TO DISK = 'C:\Backups\FinSharkDb.trn';"
```

### Restore

```powershell
# Restore from backup
sqlcmd -S localhost -E << EOF
RESTORE DATABASE FinSharkDb FROM DISK = 'C:\Backups\FinSharkDb.bak'
WITH REPLACE;
GO
EOF
```

---

## Database Maintenance

```sql
-- Update statistics
EXEC sp_updatestats;

-- Rebuild fragmented indexes
ALTER INDEX IX_Stocks_Industry ON Stocks REBUILD;

-- Check database integrity
DBCC CHECKDB (FinSharkDb);
```

---

## Connection Strings

### Development
```
Server=(local);Database=FinSharkDb_Dev;Trusted_Connection=true;TrustServerCertificate=true;
```

### Staging
```
Server=staging-db.company.local;Database=FinSharkDb_Staging;User Id=sa;Password=***;TrustServerCertificate=true;
```

### Production
```
Server=prod-db.company.local;Database=FinSharkDb_Prod;User Id=prod_user;Password=***;Encrypt=true;TrustServerCertificate=false;
```
