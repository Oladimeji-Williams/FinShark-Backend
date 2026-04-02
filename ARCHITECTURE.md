# Architecture Overview

FinShark follows a layered clean architecture with explicit CQRS and manual mappers.

## Layers

### API

`src/FinShark.API`

Responsibilities:

- expose HTTP endpoints
- bind request DTOs
- call MediatorFlow
- return `ApiResponse<T>`
- apply middleware, auth, CORS, and serialization rules

Key rules:

- controllers inherit `ApiControllerBase`
- controllers should stay thin
- controller methods should not contain business logic

### Application

`src/FinShark.Application`

Responsibilities:

- define commands and queries
- validate requests
- coordinate use cases
- map between DTOs and domain models with manual mappers
- depend on abstractions, not infrastructure implementations

Current application-facing abstractions include:

- `IAuthService`
- `IFmpService`
- `IEmailService`
- `ICurrentUserService`

### Domain

`src/FinShark.Domain`

Responsibilities:

- define entities and value objects
- express repository contracts
- hold domain-specific exceptions
- remain free of API, EF Core, and infrastructure knowledge

Core business entities:

- `ApplicationUser`
- `Stock`
- `Comment`
- `PortfolioItem`

### Persistence

`src/FinShark.Persistence`

Responsibilities:

- EF Core database access
- ASP.NET Identity persistence
- repository implementations
- entity configuration and migrations
- audit interception and seeding

Important details:

- `AppDbContext` inherits `IdentityDbContext<ApplicationUser>`
- soft delete filters apply to stocks, comments, and portfolio items
- `Created` and `Modified` are CLR properties on `BaseEntity`
- `CreatedBy` and `ModifiedBy` are EF Core shadow properties populated by `AuditSaveChangesInterceptor`

### Infrastructure

`src/FinShark.Infrastructure`

Responsibilities:

- SMTP email delivery
- FMP HTTP integration
- external service configuration

## CQRS Conventions

Write operations use commands:

- `CreateStockCommand`
- `UpdateCommentCommand`
- `AssignRoleCommand`

Read operations use queries:

- `GetStocksQuery`
- `GetCommentsByStockIdQuery`
- `GetCurrentUserIdentityQuery`

Rules:

- commands mutate state
- queries do not mutate state
- handlers depend on repository or service abstractions
- controllers talk to MediatorFlow, not directly to repositories

## Manual Mapping Conventions

The codebase intentionally uses manual mappers instead of AutoMapper.

Current mapper examples:

- `StockMapper`
- `CommentMapper`
- `AuthMapper`
- `StockQueryParametersMapper`
- `CommentQueryParametersMapper`

Why:

- explicit mapping logic
- easier code review
- predictable behavior across DTO evolution

## Repository Boundaries

Repository contracts are split by bounded context, not by transport layer.

Current repository interfaces:

- `IStockRepository`
- `ICommentRepository`
- `IPortfolioRepository`

This keeps stock CRUD, comment behavior, and portfolio behavior separate.

## Request Flow

Typical write flow:

1. Controller receives HTTP request
2. Controller creates command or query
3. MediatorFlow dispatches to handler
4. FluentValidation pipeline validates the request
5. Handler calls repositories or service abstractions
6. Manual mapper shapes response DTOs
7. Controller returns `ApiResponse<T>`

## Cross-Cutting Concerns

### Validation

- implemented with FluentValidation in the application layer
- executed through MediatorFlow pipeline behavior

### Error Handling

- implemented by `ExceptionMiddleware`
- returns consistent JSON responses

### Authorization

- JWT bearer authentication
- role checks for admin-only routes
- ownership checks for comment mutation flows

### Logging

- Serilog is the host logger
- console and rolling file sinks are enabled

## Production-Relevant Design Choices

- consistent JSON response envelope
- soft delete for user-managed business records
- audit metadata on persisted business entities
- SQL Server retry behavior enabled in persistence registration
- environment-driven configuration with `.env` support outside Production
- development-only OpenAPI endpoint exposure

## Things The Architecture Does Not Claim

To keep the docs accurate, this project does not currently implement:

- distributed caching
- background workers
- event sourcing
- message bus integration
- Swagger UI
- database or FMP deep health probes

Those can be added later, but they are not current behavior.
