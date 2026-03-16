# ADR 0001: Single Repository Interface per Bounded Context

## Status
Accepted

## Context
FinShark follows layered clean architecture with Domain, Application, Persistence, and API layers. To preserve strict DDD and separation of concerns, each bounded context (aggregate root) should define a single repository contract in the Domain layer. This avoids interface fragmentation, reduces coupling, and keeps implementation details behind a single consistent contract.

## Decision
We adopt a single repository interface per bounded context:
- `IPortfolioRepository` for portfolio operations (get holdings, add/remove by user, etc.)
- `IStockRepository` for stock CRUD and query operations
- `ICommentRepository` for comment operations

All repository contracts are declared in `FinShark.Domain.Interfaces.Repositories` (or equivalent Domain contract namespace). Application handlers depend only on these domain contracts. Persistence implements them.

This decision is intentionally not using separate read/write repository interfaces for the same aggregate in this codebase to keep the model simple, consistent, and aligned with our current architecture conventions.

## Consequences
- ✅ Stronger aggregate boundary by keeping operations grouped under one clear contract.
- ✅ Easier to understand and navigate repository APIs.
- ✅ Application handlers remain decoupled from EF Core-specific details.
- ✅ Persistence implementations remain replaceable without changing application code.

- ⚠️ Larger repository interfaces can grow. We mitigate by keeping domain operations cohesive and aggregate-specific.

## Related Documents
- `ARCHITECTURE.md`
- `IMPLEMENTATION.md`
- `README.md`
- `QUICK_REFERENCE.md`
