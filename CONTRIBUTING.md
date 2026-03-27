# Contributing

## Expectations

Contributions should preserve the current architectural rules:

- clean architecture boundaries
- CQRS for reads and writes
- manual mappers
- thin controllers
- repository contracts by bounded context

## Contribution Checklist

Before opening a PR or sharing a patch:

1. build the API project
2. run the test suite
3. update affected documentation
4. verify route and auth changes in `API_ENDPOINTS.md`
5. keep examples and `.http` requests current

## Coding Guidelines

- prefer explicit over implicit behavior
- keep business logic out of controllers
- keep infrastructure details out of the application layer
- add validators for new request shapes
- add tests when behavior changes

## Documentation Guidelines

If the implementation changes, the docs should change in the same work item.

At minimum, update any of the following that are affected:

- `README.md`
- `API_ENDPOINTS.md`
- `ARCHITECTURE.md`
- `ENVIRONMENT_CONFIGURATION.md`
- `TESTING.md`

## Pull Request Guidance

Include:

- what changed
- why it changed
- how it was validated
- any config or migration impact
