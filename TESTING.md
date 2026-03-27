# Testing

FinShark uses xUnit for tests and Moq for mocking.

## Test Project

- `src/FinShark.Tests`

The suite contains both:

- unit tests
- integration tests

## Run All Tests

```powershell
dotnet test src/FinShark.Tests/FinShark.Tests.csproj
```

## Run a Filtered Subset

```powershell
dotnet test src/FinShark.Tests/FinShark.Tests.csproj --filter "AuthApiIntegrationTests"
dotnet test src/FinShark.Tests/FinShark.Tests.csproj --filter "CreateStockValidatorTests"
```

## Test Infrastructure

Integration tests use a custom `WebApplicationFactory<Program>` and force:

```text
FINSHARK_USE_INMEMORY_DB=true
```

That means integration tests run without a real SQL Server dependency by default.

## What Is Covered

Current suite coverage includes:

- auth registration and admin flows
- stock queries and stock creation/update logic
- comment query validation
- portfolio by-symbol behavior
- FMP-backed query behavior
- mapper behavior

## Local Verification Workflow

Recommended sequence before committing:

```powershell
dotnet restore
dotnet build src/FinShark.API/FinShark.API.csproj
dotnet test src/FinShark.Tests/FinShark.Tests.csproj
```

## Notes

- tests target `.NET 10`
- integration tests rely on the real application startup path
- if you change configuration or route behavior, update the relevant integration tests and the documentation
