# Setup Guide

This guide covers the fastest path to running FinShark locally with either SQL Server or the in-memory database.

## Prerequisites

- .NET 10 SDK
- SQL Server or LocalDB if you want a persistent database
- PowerShell

## 1. Restore Dependencies

```powershell
dotnet restore
```

## 2. Configure Environment

Copy `.env.example` to `.env` and fill in real values where needed.

Minimum recommended local configuration:

```env
ASPNETCORE_ENVIRONMENT=Development
Jwt__Key=replace-with-a-real-dev-secret-of-at-least-32-characters
AppSettings__ClientUrl=https://localhost:7235
FINSHARK_DB_CONNECTION=Server=(localdb)\mssqllocaldb;Database=FinSharkDb;Trusted_Connection=True;MultipleActiveResultSets=true
FMP__ApiKey=your-fmp-key
```

Notes:

- `.env` is loaded automatically outside Production.
- `FINSHARK_DB_CONNECTION` overrides `ConnectionStrings:DefaultConnection`.
- `FINSHARK_USE_INMEMORY_DB=true` skips SQL Server entirely.

## 3. Apply Database Migrations

```powershell
dotnet ef database update --project src/FinShark.Persistence --startup-project src/FinShark.API
```

## 4. Run the API

```powershell
dotnet run --project src/FinShark.API/FinShark.API.csproj
```

Local profiles:

- HTTP: `http://localhost:5192`
- HTTPS: `https://localhost:7235`

## 5. Verify the Application

```powershell
Invoke-RestMethod -Uri "https://localhost:7235/api/health" -SkipCertificateCheck
```

In Development, OpenAPI JSON is also available at:

```text
https://localhost:7235/openapi/v1.json
```

## Optional: Run With In-Memory Database

Useful for quick local validation when SQL Server is unavailable.

```powershell
$env:FINSHARK_USE_INMEMORY_DB='true'
dotnet run --project src/FinShark.API/FinShark.API.csproj
```

## Optional: Seed Sample Data

```powershell
dotnet run --project src/FinShark.API/FinShark.API.csproj -- --seed
```

Seed behavior:

- Creates `Admin` and `User` roles if missing
- Creates `seeduser@example.com`
- Inserts sample stocks
- Inserts two sample comments per stock

## Optional: Run the API Request Collection

The REST client file is [FinShark.API.http](/c:/Users/OladimejiWilliams/Desktop/Software%20Engineering/FinShark/finshark-backend/src/FinShark.API/FinShark.API.http).

You can use it from VS Code REST Client or copy requests into Postman/Insomnia.

## Common Next Steps

- Read [API_ENDPOINTS.md](API_ENDPOINTS.md) for route details
- Read [ARCHITECTURE.md](ARCHITECTURE.md) before changing layer boundaries
- Read [TROUBLESHOOTING.md](TROUBLESHOOTING.md) if startup fails
