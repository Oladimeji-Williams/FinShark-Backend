# Development Workflow

This document describes the expected day-to-day workflow for changing FinShark safely.

## Core Rules

- keep controllers thin
- use CQRS for all new use cases
- keep command and query handlers separate
- use manual mappers only
- keep domain and application free of API and EF Core implementation details
- keep repository contracts scoped to bounded contexts

## Recommended Local Loop

```powershell
dotnet restore
dotnet build src/FinShark.API/FinShark.API.csproj
dotnet test src/FinShark.Tests/FinShark.Tests.csproj
```

## Running the API

```powershell
dotnet run --project src/FinShark.API/FinShark.API.csproj
```

Useful variants:

```powershell
dotnet run --project src/FinShark.API/FinShark.API.csproj -- --seed
$env:FINSHARK_USE_INMEMORY_DB='true'; dotnet run --project src/FinShark.API/FinShark.API.csproj
```

## Adding a New Feature

Use this sequence unless the feature is purely internal refactoring:

1. define or update the domain model
2. update repository contracts if persistence behavior changes
3. add DTOs in the application layer
4. add command or query types
5. add FluentValidation validators
6. add handlers
7. add or update manual mappers
8. implement persistence or infrastructure behavior
9. expose the endpoint in API
10. add or update tests
11. update documentation

## Authorization Expectations

When adding mutation endpoints, decide explicitly:

- public
- authenticated
- admin-only
- owner-or-admin

Do not leave write endpoints unaudited by accident.

## Mapping Expectations

Current convention:

- DTO-to-domain mapping is explicit
- domain-to-DTO mapping is explicit
- query DTOs are mapped to repository query objects explicitly

If you add a new transport shape, add a mapper rather than hiding conversion in controllers or repositories.

## Persistence Expectations

- use EF Core configuration classes for schema rules
- keep business queries inside repositories
- keep external service calls out of repositories
- use `IgnoreQueryFilters()` only when bypassing soft delete is intentional

## Documentation Expectations

After changing routes, config, or behavior, update at least:

- `README.md`
- `API_ENDPOINTS.md`
- any specialized doc that describes the changed area

## Before Merging

- build passes
- tests pass
- docs match the implementation
- no obsolete examples remain in `.http`, `.md`, or `.env.example`
