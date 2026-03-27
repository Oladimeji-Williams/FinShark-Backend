# Implementation Guide

This guide explains how to implement features in a way that stays consistent with the current architecture.

## 1. Start With the Use Case

Ask first:

- is this a read or a write
- which layer owns the rule
- which bounded context owns the persistence behavior

## 2. Model the Request in Application

For a new API feature, add:

- request DTOs
- response DTOs
- command or query record
- validator

Examples already in the codebase:

- `RegisterCommand`
- `GetStocksQuery`
- `CommentQueryRequestDto`
- `StockQueryRequestDto`

## 3. Keep Controllers Thin

Controllers should:

- bind DTOs
- extract auth context when needed
- send commands or queries through MediatR
- return `ApiResponse<T>`

Controllers should not:

- query DbContext directly
- map complex objects inline
- own business rules

## 4. Use Manual Mappers

Add or update a mapper when:

- an endpoint shape changes
- a domain entity gains new response fields
- transport query objects must be translated to repository query parameters

Examples:

- `AuthMapper`
- `StockMapper`
- `CommentMapper`
- `StockQueryParametersMapper`
- `CommentQueryParametersMapper`

## 5. Put Business Logic in the Right Place

- entity invariants belong in domain entities or value objects
- use-case orchestration belongs in handlers
- data access belongs in repositories
- external HTTP and SMTP logic belongs in infrastructure

## 6. Use Existing Abstractions

Application handlers should prefer abstractions such as:

- `IAuthService`
- `IFmpService`
- `IEmailService`
- repository interfaces in the domain layer

Do not inject infrastructure implementations into the application layer.

## 7. Handle Authorization Intentionally

Use the current patterns:

- framework authorization attributes for route-level protection
- user ID extraction in API layer
- owner-or-admin checks in handlers for resource ownership decisions

## 8. Preserve Consistent Errors

If a feature introduces a new domain failure that should map consistently:

- add a domain exception
- let middleware map it
- avoid ad hoc controller exception handling unless there is a strong reason

## 9. Add Tests

Minimum expectation:

- unit tests for new validators or mappers
- unit tests for new handlers when logic is non-trivial
- integration tests when routes or end-to-end behavior changes

## 10. Update Documentation

If you change:

- routes
- auth requirements
- config keys
- startup behavior
- schema shape

then update the relevant documentation in the same change.
