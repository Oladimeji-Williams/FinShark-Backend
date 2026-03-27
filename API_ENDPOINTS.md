# API Endpoints

This document is the canonical HTTP reference for the current implementation.

## Base URLs

Development launch profiles:

- `http://localhost:5192`
- `https://localhost:7235`

Canonical API prefix:

- `/api`

Auth compatibility alias:

- `/api/account`

Preferred auth prefix for new clients:

- `/api/auth`

## Response Envelope

All endpoints return `ApiResponse<T>`.

```json
{
  "success": true,
  "data": {},
  "message": "Human readable message",
  "fmpStatusCode": null,
  "suggestion": null,
  "errors": null
}
```

FMP failures may also populate:

- `fmpStatusCode`
- `suggestion`

## Health

### `GET /api/health`

Returns API health with SMTP configuration warnings.

Notes:

- always JSON
- currently checks SMTP configuration only
- does not verify database connectivity or FMP reachability

## Authentication

### `POST /api/auth/register`

Registers a user, assigns the default `User` role, generates a JWT, and attempts to send an email confirmation link.

Request:

```json
{
  "email": "trader@example.com",
  "password": "Password123!",
  "userName": "trader123",
  "firstName": "Ada",
  "lastName": "Lovelace"
}
```

Responses:

- `200 OK`
- `400 Bad Request`
- `409 Conflict`

### `POST /api/auth/login`

Logs a user in by email and password.

Important:

- login requires `EmailConfirmed = true`
- failed login attempts can lock the account out

Responses:

- `200 OK`
- `401 Unauthorized`

### `POST /api/auth/resend-confirmation`

Creates and attempts to send a new confirmation link.

Responses:

- `200 OK`
- `400 Bad Request`

### `GET /api/auth/confirm-email?userId={userId}&token={token}`

Confirms the user email.

Responses:

- `200 OK`
- `400 Bad Request`

### `GET /api/auth/profile`

Requires authentication.

Returns the current user's full profile.

Responses:

- `200 OK`
- `401 Unauthorized`
- `404 Not Found`

### `GET /api/auth/me`

Requires authentication.

Returns the current user's lightweight identity view.

Responses:

- `200 OK`
- `401 Unauthorized`
- `404 Not Found`

### `PATCH /api/auth/profile`

Requires authentication.

Updates any combination of:

- `userName`
- `firstName`
- `lastName`

Responses:

- `200 OK`
- `400 Bad Request`
- `401 Unauthorized`

### `POST /api/auth/change-password`

Requires authentication.

Request:

```json
{
  "currentPassword": "OldPassword123!",
  "newPassword": "NewPassword123!"
}
```

Responses:

- `200 OK`
- `400 Bad Request`
- `401 Unauthorized`

### `GET /api/auth/admin/users`

Requires `Admin`.

Responses:

- `200 OK`
- `401 Unauthorized`
- `403 Forbidden`

### `POST /api/auth/admin/assign-role`

Requires `Admin`.

Request:

```json
{
  "userId": "user-id",
  "role": "Admin"
}
```

Responses:

- `200 OK`
- `400 Bad Request`
- `401 Unauthorized`
- `403 Forbidden`

### `GET /api/auth/smtp-test`

Requires `Admin`.

Purpose:

- smoke-test SMTP delivery path from the API

Responses:

- `200 OK`
- `401 Unauthorized`
- `403 Forbidden`
- `500 Internal Server Error` if the configured SMTP provider throws

## Stocks

### `GET /api/stocks`

Public read endpoint.

Query parameters:

- `symbol`
- `companyName`
- `sector`
- `minPrice`
- `maxPrice`
- `minMarketCap`
- `maxMarketCap`
- `sortBy`: `symbol`, `companyName`, `currentPrice`, `marketCap`, `created`
- `sortDirection`: `asc`, `desc`
- `pageNumber`
- `pageSize`

Responses:

- `200 OK`
- `400 Bad Request`

### `POST /api/stocks`

Requires authentication.

Request:

```json
{
  "symbol": "AAPL",
  "companyName": "Apple Inc.",
  "currentPrice": 250.50,
  "sector": "Technology",
  "marketCap": 2500000000000
}
```

Responses:

- `201 Created`
- `400 Bad Request`
- `401 Unauthorized`
- `409 Conflict`

### `GET /api/stocks/{id}`

Public read endpoint.

Responses:

- `200 OK`
- `404 Not Found`

### `GET /api/stocks/quote/{symbol}`

Public read endpoint backed by FMP.

Responses:

- `200 OK`
- `400 Bad Request`
- `404 Not Found`

### `GET /api/stocks/quote/full/{symbol}`

Public read endpoint backed by FMP company profile data.

Responses:

- `200 OK`
- `400 Bad Request`
- `404 Not Found`

### `PATCH /api/stocks/{id}`

Requires authentication.

Supports partial update with any combination of:

- `symbol`
- `companyName`
- `currentPrice`
- `sector`
- `marketCap`

Responses:

- `200 OK`
- `400 Bad Request`
- `401 Unauthorized`
- `404 Not Found`

### `DELETE /api/stocks/{id}?hardDelete={bool}`

Requires authentication.

Rules:

- soft delete is allowed for authenticated users
- `hardDelete=true` requires `Admin`

Responses:

- `200 OK`
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`

## Portfolio

All portfolio endpoints require authentication.

### `GET /api/portfolio`

Returns the current user's portfolio stocks.

Responses:

- `200 OK`
- `401 Unauthorized`

### `POST /api/portfolio/{stockId}`

Adds an existing local stock to the current user's portfolio.

Responses:

- `200 OK`
- `400 Bad Request`
- `401 Unauthorized`

### `POST /api/portfolio/symbol/{symbol}`

Adds a stock by symbol.

Behavior:

- if the symbol exists locally, it uses the local stock
- otherwise it queries FMP, stores the stock locally, then adds it to the portfolio

Responses:

- `200 OK`
- `400 Bad Request`
- `401 Unauthorized`
- `404 Not Found`

### `DELETE /api/portfolio/{stockId}?hardDelete={bool}`

Removes a portfolio item for the current user.

Rules:

- default behavior is soft delete
- `hardDelete=true` requires `Admin`

Responses:

- `200 OK`
- `400 Bad Request`
- `401 Unauthorized`
- `403 Forbidden`

## Comments

### `GET /api/comments`

Public read endpoint.

Query parameters:

- `pageNumber`
- `pageSize`
- `stockId`
- `stockSymbol`
- `minRating`
- `maxRating`
- `titleContains`
- `contentContains`
- `sortBy`: `created`, `rating`, `title`, `symbol`
- `sortDirection`: `asc`, `desc`

Responses:

- `200 OK`
- `400 Bad Request`

### `GET /api/comments/{id}`

Public read endpoint.

Responses:

- `200 OK`
- `404 Not Found`

### `GET /api/stocks/{stockId}/comments`

Public read endpoint.

Supports the same comment filtering options, with `stockId` fixed by route.

Responses:

- `200 OK`
- `400 Bad Request`
- `404 Not Found`

### `POST /api/stocks/{stockId}/comments`

Requires authentication.

Request:

```json
{
  "title": "Long-term upside",
  "content": "The balance sheet looks healthy and the valuation still leaves room for growth.",
  "rating": 5
}
```

Responses:

- `201 Created`
- `400 Bad Request`
- `401 Unauthorized`
- `404 Not Found`

### `PATCH /api/comments/{id}`

Requires authentication.

Rules:

- comment owner can update
- admins can update any comment

Responses:

- `200 OK`
- `400 Bad Request`
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`

### `DELETE /api/comments/{id}?hardDelete={bool}`

Requires authentication.

Rules:

- comment owner can soft delete their own comment
- admins can delete any comment
- `hardDelete=true` requires `Admin`

Responses:

- `200 OK`
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`

## Serialization Notes

- `SectorType` is serialized as a string value such as `"Technology"`
- the API also accepts legacy numeric sector codes on input
- `Rating` is serialized as an integer from `1` to `5`
- comment timestamps are exposed as `created` and `modified`

## OpenAPI

In Development only:

- `GET /openapi/v1.json`

Swagger UI is not currently configured.
