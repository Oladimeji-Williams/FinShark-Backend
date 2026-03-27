# Quick Reference

## Build and Run

```powershell
dotnet restore
dotnet build src/FinShark.API/FinShark.API.csproj
dotnet run --project src/FinShark.API/FinShark.API.csproj
```

## Tests

```powershell
dotnet test src/FinShark.Tests/FinShark.Tests.csproj
dotnet test src/FinShark.Tests/FinShark.Tests.csproj --filter "AuthApiIntegrationTests"
```

## Database

```powershell
dotnet ef database update --project src/FinShark.Persistence --startup-project src/FinShark.API
dotnet run --project src/FinShark.API/FinShark.API.csproj -- --seed
```

## In-Memory Mode

```powershell
$env:FINSHARK_USE_INMEMORY_DB='true'
dotnet run --project src/FinShark.API/FinShark.API.csproj
```

## Local URLs

- `http://localhost:5192`
- `https://localhost:7235`
- `https://localhost:7235/api/health`
- `https://localhost:7235/openapi/v1.json` in Development only

## Core Routes

- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/stocks`
- `GET /api/stocks/quote/{symbol}`
- `GET /api/comments`
- `GET /api/portfolio`

## Admin Routes

- `GET /api/auth/admin/users`
- `POST /api/auth/admin/assign-role`
- `GET /api/auth/smtp-test`

## Key Environment Variables

- `FINSHARK_DB_CONNECTION`
- `FINSHARK_USE_INMEMORY_DB`
- `Jwt__Key`
- `AppSettings__ClientUrl`
- `FMP__ApiKey`
- `Smtp__Host`
- `Smtp__FromEmail`

## Important Docs

- `README.md`
- `ARCHITECTURE.md`
- `API_ENDPOINTS.md`
- `ENVIRONMENT_CONFIGURATION.md`
- `TESTING.md`
