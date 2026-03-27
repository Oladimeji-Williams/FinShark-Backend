# ADR 0001: Repository Contracts by Bounded Context

## Status

Accepted

## Context

The codebase follows clean architecture and CQRS, but persistence still needs stable seams for application handlers. Repository interfaces should stay cohesive and reflect business ownership rather than transport or ORM concerns.

## Decision

FinShark keeps one repository contract per bounded context:

- `IStockRepository`
- `ICommentRepository`
- `IPortfolioRepository`

These contracts live in the domain layer, and persistence implements them.

## Consequences

- clearer aggregate ownership
- better separation between stock CRUD and portfolio behavior
- application handlers stay decoupled from EF Core details
- repository growth must still be monitored so interfaces remain cohesive

## Notes

This ADR does not mean read and write logic are merged at the application level. CQRS is still enforced through separate commands and queries.
