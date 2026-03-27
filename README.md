# FinShark Backend

FinShark is a .NET 10 backend for stock discovery, user authentication, portfolio tracking, and comment management. The codebase is organized around clean architecture, CQRS, explicit manual mappers, and consistent API response envelopes.

## Current Scope

- JWT-based authentication and role-based authorization
- Email confirmation flow
- Stock CRUD and filtered stock queries
- External quote/profile lookup through Financial Modeling Prep (FMP)
- Portfolio management by stock ID or symbol
- Comment creation, filtering, update, and delete flows
- Global exception handling, validation, and structured logging

## Architecture Summary

- `FinShark.API`: HTTP endpoints, middleware, serialization, pipeline configuration
- `FinShark.Application`: CQRS handlers, validators, DTOs, manual mappers, service abstractions
- `FinShark.Domain`: entities, value objects, repository contracts, domain exceptions
- `FinShark.Persistence`: EF Core, ASP.NET Identity, repositories, migrations, seeding
- `FinShark.Infrastructure`: SMTP and FMP integrations
- `FinShark.Tests`: unit and integration tests

Controllers are intentionally thin. They delegate to MediatR commands and queries, and the application layer performs mapping through explicit mapper classes rather than AutoMapper.

## Quick Start

```powershell
dotnet restore
dotnet build src/FinShark.API/FinShark.API.csproj
dotnet run --project src/FinShark.API/FinShark.API.csproj
```

Default development URLs from `launchSettings.json`:

- `http://localhost:5192`
- `https://localhost:7235`

Useful local endpoints:

- `GET /api/health`
- `GET /openapi/v1.json` in Development only

## Required Configuration

At minimum, configure:

- `FINSHARK_DB_CONNECTION` or `ConnectionStrings:DefaultConnection`
- `Jwt__Key`
- `AppSettings__ClientUrl`
- `FMP__ApiKey` for external stock lookup
- `Smtp__Host` and `Smtp__FromEmail` for real email delivery

Development can also use:

- `FINSHARK_USE_INMEMORY_DB=true`

See [ENVIRONMENT_CONFIGURATION.md](ENVIRONMENT_CONFIGURATION.md) for the full matrix.

## Runtime Conventions

- All controllers produce JSON through [ApiControllerBase.cs](/c:/Users/OladimejiWilliams/Desktop/Software%20Engineering/FinShark/finshark-backend/src/FinShark.API/Controllers/ApiControllerBase.cs).
- All responses are wrapped in `ApiResponse<T>`.
- Validation failures return `400`.
- Not found errors return `404`.
- Forbidden operations return `403`.
- Stock conflicts return `409`.
- Unexpected failures return `500`.

## Authentication Model

Canonical auth route prefix:

- `/api/auth`

Compatibility alias:

- `/api/account`

Current authorization rules:

- Stock write endpoints require authentication.
- Comment create, update, and delete require authentication.
- Comment update and delete enforce owner-or-admin rules.
- Portfolio endpoints require authentication.
- Hard delete operations on comments, portfolio entries, and stocks require admin privileges.
- `GET /api/auth/admin/users`, `POST /api/auth/admin/assign-role`, and `GET /api/auth/smtp-test` require the `Admin` role.

## Database Model

Main business tables:

- `Stocks`
- `Comments`
- `PortfolioItems`
- ASP.NET Identity tables such as `AspNetUsers`, `AspNetRoles`, and `AspNetUserRoles`

Business entities use soft delete and audit metadata:

- CLR properties: `Created`, `Modified`
- Shadow properties: `CreatedBy`, `ModifiedBy`

See [DATABASE_SCHEMA.md](DATABASE_SCHEMA.md) for details.

## Operations

Run seed data:

```powershell
dotnet run --project src/FinShark.API/FinShark.API.csproj -- --seed
```

Run tests:

```powershell
dotnet test src/FinShark.Tests/FinShark.Tests.csproj
```

## Documentation Map

- [SETUP.md](SETUP.md): local setup and first run
- [ARCHITECTURE.md](ARCHITECTURE.md): layering, CQRS, and dependency rules
- [API_ENDPOINTS.md](API_ENDPOINTS.md): endpoint-by-endpoint API reference
- [DATABASE_SCHEMA.md](DATABASE_SCHEMA.md): schema, relationships, indexes, and audit behavior
- [ENVIRONMENT_CONFIGURATION.md](ENVIRONMENT_CONFIGURATION.md): configuration keys and precedence
- [ERROR_HANDLING.md](ERROR_HANDLING.md): exception-to-HTTP mapping
- [HEALTH_CHECKS.md](HEALTH_CHECKS.md): health endpoint behavior and limitations
- [LOGGING_MONITORING.md](LOGGING_MONITORING.md): logging behavior and operational guidance
- [TESTING.md](TESTING.md): unit and integration testing workflow
- [DEVELOPMENT.md](DEVELOPMENT.md): everyday development workflow
- [IMPLEMENTATION.md](IMPLEMENTATION.md): how to add new features correctly
- [VALIDATION_RULES.md](VALIDATION_RULES.md): validation rules by feature
- [QUICK_REFERENCE.md](QUICK_REFERENCE.md): short command and endpoint cheatsheet
- [TROUBLESHOOTING.md](TROUBLESHOOTING.md): common failure modes and fixes
- [CONTRIBUTING.md](CONTRIBUTING.md): contribution expectations

## Production Notes

The codebase is organized for production use, but the sample configuration files are intentionally development-friendly. Before production deployment, override at least the following:

- Restrict `Cors:AllowedOrigins`
- Provide a strong `Jwt__Key`
- Use a real SQL Server connection string
- Configure SMTP credentials
- Configure `FMP__ApiKey`
- Set `AppSettings__ClientUrl` to the deployed client URL
- Use HTTPS and environment-based secret management

## Status

- Build: passing
- Tests: passing
- Documentation: aligned with the current CQRS/manual-mapping implementation
