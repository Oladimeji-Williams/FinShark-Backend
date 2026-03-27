# Database Schema

FinShark uses SQL Server through EF Core and ASP.NET Identity.

## Main Tables

### `AspNetUsers`

Backed by `ApplicationUser`.

Important fields:

- `Id`
- `UserName`
- `NormalizedUserName`
- `Email`
- `NormalizedEmail`
- `EmailConfirmed`
- `FirstName`
- `LastName`
- `Created`
- `Modified`

Important notes:

- normalized email is indexed uniquely
- role membership is managed through ASP.NET Identity tables

### `AspNetRoles`

Stores role names such as:

- `User`
- `Admin`

### `Stocks`

Backed by `Stock`.

Important fields:

- `Id`
- `Symbol`
- `CompanyName`
- `CurrentPrice`
- `Purchase`
- `LastDiv`
- `Sector`
- `MarketCap`
- `Created`
- `Modified`
- `IsDeleted`
- `CreatedBy` shadow property
- `ModifiedBy` shadow property

Important constraints:

- unique index on `Symbol`
- `Symbol` max length 10
- `CompanyName` max length 255
- `CurrentPrice` precision `(18,2)`
- `MarketCap` precision `(18,2)`

### `Comments`

Backed by `Comment`.

Important fields:

- `Id`
- `UserId`
- `StockId`
- `Title`
- `Content`
- `Rating`
- `Created`
- `Modified`
- `IsDeleted`
- `CreatedBy` shadow property
- `ModifiedBy` shadow property

Important constraints:

- rating check constraint: `1 <= Rating <= 5`
- index on `StockId`
- index on `UserId`
- index on `Created`
- `Title` max length 200

### `PortfolioItems`

Backed by `PortfolioItem`.

Important fields:

- `Id`
- `UserId`
- `StockId`
- `Created`
- `Modified`
- `IsDeleted`
- `CreatedBy` shadow property
- `ModifiedBy` shadow property

Important constraints:

- unique composite index on `(UserId, StockId)`

## Relationships

- one `ApplicationUser` to many `Comments`
- one `ApplicationUser` to many `PortfolioItems`
- one `Stock` to many `Comments`
- one `Stock` to many `PortfolioItems`

Delete behavior:

- comment relationships cascade at the database mapping level
- portfolio relationships cascade at the database mapping level
- soft delete filters still hide business rows from normal reads

## Audit Model

Business entities derived from `BaseEntity` use:

- CLR properties: `Created`, `Modified`, `IsDeleted`
- shadow properties: `CreatedBy`, `ModifiedBy`

Population behavior:

- `AuditSaveChangesInterceptor` stamps `Created` and `CreatedBy` on insert
- `AuditSaveChangesInterceptor` stamps `Modified` and `ModifiedBy` on update

## Soft Delete Behavior

Soft delete applies to:

- `Stocks`
- `Comments`
- `PortfolioItems`

Global query filters hide rows where `IsDeleted = true`.

Repository implementations can bypass query filters when required for administrative or restore-style operations.

## Migrations

Migrations live in:

- `src/FinShark.Persistence/Migrations`

Apply them with:

```powershell
dotnet ef database update --project src/FinShark.Persistence --startup-project src/FinShark.API
```

## Seed Data

The seed flow creates:

- default roles: `Admin`, `User`
- a default seed user
- sample stock rows
- sample comment rows

Run:

```powershell
dotnet run --project src/FinShark.API/FinShark.API.csproj -- --seed
```
