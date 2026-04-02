# Error Handling

FinShark uses centralized exception handling through `ExceptionMiddleware` plus a few explicit controller-level authorization checks.

## Response Shape

Errors use `ApiResponse<object>`.

```json
{
  "success": false,
  "data": null,
  "message": "Validation failed",
  "fmpStatusCode": null,
  "suggestion": null,
  "errors": [
    "Current price must be greater than zero."
  ]
}
```

## Exception Mapping

Current middleware behavior:

- `ValidationException` -> `400 Bad Request`
- `StockAlreadyExistsException` -> `409 Conflict`
- `StockNotFoundException` -> `404 Not Found`
- `CommentNotFoundException` -> `404 Not Found`
- `KeyNotFoundException` -> `404 Not Found`
- `FMPServiceException` -> `400 Bad Request`
- `ForbiddenOperationException` -> `403 Forbidden`
- `UnauthorizedAccessException` -> `403 Forbidden`
- `FinSharkException` -> `400 Bad Request`
- all other exceptions -> `500 Internal Server Error`

## Authorization Failures

Authorization failures can come from two places:

- ASP.NET Core auth middleware
- controller-level checks for missing user identity claims or admin-only hard delete operations

Typical outcomes:

- `401 Unauthorized` when authentication is missing or invalid
- `403 Forbidden` when the user is authenticated but lacks permission

## Validation Failures

Validation enters through:

- model binding and JSON conversion
- FluentValidation MediatorFlow pipeline behavior

Examples:

- invalid `sector` string
- invalid `rating`
- missing required registration fields
- invalid pagination bounds

## FMP Failures

FMP-related failures may include:

- `message`
- `fmpStatusCode`
- `suggestion`

This makes external lookup failures distinguishable from generic API failures.

## Unexpected Errors

Unhandled exceptions return:

- `500 Internal Server Error`
- message: `An unexpected error occurred. Please contact support.`

Implementation note:

- stack traces are logged server-side
- they are not returned to clients

## Logging

The middleware logs:

- validation and domain exceptions at warning level
- unexpected exceptions at error level

See [LOGGING_MONITORING.md](LOGGING_MONITORING.md) for runtime logging behavior.
