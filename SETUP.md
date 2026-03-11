# FinShark Backend - Setup Guide

## Prerequisites

- **.NET 10.0 SDK** or later - [Download](https://dotnet.microsoft.com/en-us/download/dotnet)
- **SQL Server 2019** or later (or SQL Server Express)
- **Git** for version control
- **Visual Studio Code** or **Visual Studio 2022** (recommended)

## Initial Setup

### 1. Clone the Repository

```bash
git clone <repository-url>
cd finshark-backend
```

### 2. Install Dependencies

```bash
dotnet restore
```

This automatically downloads all NuGet packages defined in the `.csproj` files.

### 3. Configure Database Connection

Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=FinSharkDb;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

**Local SQL Server Examples:**
- **Local Machine**: `Server=(local);Database=FinSharkDb;Trusted_Connection=true;TrustServerCertificate=true;`
- **SQL Server Express**: `Server=.\\SQLEXPRESS;Database=FinSharkDb;Trusted_Connection=true;TrustServerCertificate=true;`
- **Remote Server**: `Server=your-server-address;Database=FinSharkDb;User Id=sa;Password=your-password;`

### 4. Create the Database

Navigate to the API project and run:

```bash
cd src/FinShark.API
dotnet ef database update
```

This creates the database and applies all migrations.

### 5. Verify Setup

Build the project to ensure everything compiles:

```bash
cd ../..
dotnet build
```

Expected output: `Build succeeded`

### 6. Run the Application

```bash
cd src/FinShark.API
dotnet run
```

The API will start at:
- HTTP: `http://localhost:5192`
- HTTPS: `https://localhost:7235`

Visit `https://localhost:7235/openapi/v1.json` to view the OpenAPI schema.

## Project Structure

```
finshark-backend/
├── src/
│   ├── FinShark.Domain/              # Business entities & interfaces
│   ├── FinShark.Application/         # Use cases, validators, mappers, DTOs
│   ├── FinShark.Persistence/         # Database context, repositories
│   ├── FinShark.Infrastructure/      # External services
│   └── FinShark.API/                 # Controllers, middleware, configuration
├── tests/                            # Unit & integration tests
├── FinShark.slnx                     # Solution file
├── appsettings.json                  # Configuration
└── README.md
```

## Dependency Tree

```
FinShark.API (depends on)
  ├── FinShark.Application
  ├── FinShark.Persistence
  └── FinShark.Infrastructure

FinShark.Application (depends on)
  ├── FinShark.Domain
  └── External: MediatR, FluentValidation

FinShark.Persistence (depends on)
  ├── FinShark.Domain
  └── External: EF Core, SQL Server

FinShark.Domain (depends on)
  └── None (pure business logic)

FinShark.Infrastructure (depends on)
  ├── FinShark.Application
  └── FinShark.Domain
```

## Common Tasks

### Add a New Package

```bash
cd src/FinShark.Application
dotnet add package PackageName --version 1.0.0
```

### Create a Database Migration

```bash
cd src/FinShark.API
dotnet ef migrations add MigrationName -p ../FinShark.Persistence
```

### Update Database to Latest Migration

```bash
cd src/FinShark.API
dotnet ef database update
```

### Run Tests

```bash
dotnet test
```

### Clean Build

```bash
dotnet clean
dotnet build
```

## Environment Configuration

### Development (`appsettings.Development.json`)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(local);Database=FinSharkDb_Dev;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

### Production

Set via environment variables or Azure Key Vault:

```bash
export ConnectionStrings__DefaultConnection="your-production-connection-string"
export ASPNETCORE_ENVIRONMENT=Production
```

## Troubleshooting

### Issue: "Connection string 'DefaultConnection' not found"

**Solution**: Ensure `appsettings.json` has the correct connection string configured.

### Issue: Database migration fails

**Solution**: 
```bash
# Check for unapplied migrations
dotnet ef migrations list

# Revert to previous migration if needed
dotnet ef database update [PreviousMigrationName]
```

### Issue: Port already in use (5192 or 7235)

**Solution**: Either stop the application using the port or modify `Properties/launchSettings.json`:

```json
{
  "applicationUrl": "https://localhost:7236;http://localhost:5193"
}
```

### Issue: NuGet package restore fails

**Solution**:
```bash
dotnet nuget locals all --clear
dotnet restore
```

## IDE Setup

### Visual Studio Code

1. Install extensions:
   - C# Dev Kit
   - REST Client
   - SQL Server (mssql)

2. Open workspace:
   ```bash
   code .
   ```

### Visual Studio 2022

1. Open `FinShark.slnx` solution file
2. Wait for initial project loading
3. Set `FinShark.API` as startup project (right-click → Set as Startup Project)
4. Press `F5` to run

## Next Steps

- Read [IMPLEMENTATION.md](./IMPLEMENTATION.md) to learn how to add features
- Review [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) for common patterns
- Check [README.md](./README.md) for project overview
