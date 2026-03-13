# FinShark API Endpoints Reference

Complete guide to all available REST API endpoints with request/response examples.

## Base URL

```
Development: http://localhost:5000 or https://localhost:5001
Production: https://your-domain.com/api
```

## API Versions

- **Current Version**: v1
- **Base Path**: `/api`

---

## Stock Endpoints

### 1. Create Stock

Creates a new stock record.

**Endpoint**: `POST /api/stocks`

**Authentication**: None (can be added later with JWT)

**Request Headers**:
```
Content-Type: application/json
```

**Request Body**:
```json
{
  "symbol": "AAPL",
  "companyName": "Apple Inc.",
  "currentPrice": 250.50,
  "industry": "Technology",
  "marketCap": 2500000000000
}
```

**Response (201 Created)**:
```json
{
  "success": true,
  "data": 1,
  "message": "Stock created successfully",
  "errors": null
}
```

**Response (400 Bad Request)**:
```json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": [
    "Symbol is required",
    "Company Name is required",
    "Current Price must be greater than 0",
    "Industry is required",
    "Market Cap must be greater than 0"
  ]
}
```

**Response (409 Conflict)**:
```json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": ["A stock with symbol 'AAPL' already exists"]
}
```

**Example cURL**:
```bash
curl -X POST "https://localhost:5001/api/stocks" \
  -H "Content-Type: application/json" \
  -d '{
    "symbol": "AAPL",
    "companyName": "Apple Inc.",
    "currentPrice": 250.50,
    "industry": "Technology",
    "marketCap": 2500000000000
  }'
```

---

### 2. Get All Stocks

Retrieves all stocks with optional filtering, sorting, and pagination.

**Endpoint**: `GET /api/stocks`

**Query Parameters**:
- `pageNumber` (int, optional): Page number (1-based)
- `pageSize` (int, optional): Items per page (max: 100)
- `industry` (string, optional): Filter by industry
- `symbol` (string, optional): Search by symbol (partial match)
- `companyName` (string, optional): Search by company name (partial match)
- `minPrice` (decimal, optional): Minimum current price
- `maxPrice` (decimal, optional): Maximum current price
- `minMarketCap` (decimal, optional): Minimum market cap
- `maxMarketCap` (decimal, optional): Maximum market cap
- `sortBy` (string, optional): Sort field (`symbol`, `companyName`, `currentPrice`, `marketCap`, `created`)
- `sortDirection` (string, optional): `asc` or `desc`

**Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "symbol": "AAPL",
        "companyName": "Apple Inc.",
        "currentPrice": 250.50,
        "industry": "Technology",
        "marketCap": 2500000000000,
        "createdAt": "2026-03-10T10:30:00Z",
        "updatedAt": null
      },
      {
        "id": 2,
        "symbol": "MSFT",
        "companyName": "Microsoft Corporation",
        "currentPrice": 380.25,
        "industry": "Technology",
        "marketCap": 2800000000000,
        "createdAt": "2026-03-11T14:15:00Z",
        "updatedAt": null
      }
    ],
    "pagination": {
      "totalCount": 2,
      "pageNumber": 1,
      "pageSize": 2,
      "totalPages": 1,
      "hasNextPage": false,
      "hasPreviousPage": false
    }
  },
  "message": "Stocks retrieved successfully",
  "errors": null
}
```

**Example cURL**:
```bash
# Get all stocks
curl -X GET "https://localhost:5001/api/stocks" \
  -H "Content-Type: application/json"

# Get with pagination
curl -X GET "https://localhost:5001/api/stocks?pageNumber=1&pageSize=20" \
  -H "Content-Type: application/json"

# Filter by industry
curl -X GET "https://localhost:5001/api/stocks?industry=Technology" \
  -H "Content-Type: application/json"
```

---

### 3. Get Stock by ID

Retrieves a specific stock by its ID.

**Endpoint**: `GET /api/stocks/{id}`

**Path Parameters**:
- `id` (int, required): Stock ID

**Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "id": 1,
    "symbol": "AAPL",
    "companyName": "Apple Inc.",
    "currentPrice": 250.50,
    "industry": "Technology",
    "marketCap": 2500000000000,
    "createdAt": "2026-03-10T10:30:00Z",
    "updatedAt": null
  },
  "message": "Stock retrieved successfully",
  "errors": null
}
```

**Response (404 Not Found)**:
```json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": ["Stock with ID 999 not found"]
}
```

**Example cURL**:
```bash
curl -X GET "https://localhost:5001/api/stocks/1" \
  -H "Content-Type: application/json"
```

---

### 4. Update Stock

Updates an existing stock.

**Endpoint**: `PATCH /api/stocks/{id}`

**Path Parameters**:
- `id` (int, required): Stock ID

**Request Body**:
```json
{
  "symbol": "AAPL",
  "companyName": "Apple Inc.",
  "currentPrice": 275.00,
  "industry": "Technology",
  "marketCap": 2750000000000
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "data": true,
  "message": "Stock updated successfully",
  "errors": null
}
```

**Response (400 Bad Request)**:
```json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": [
    "Current Price must be greater than 0",
    "Market Cap must be greater than 0"
  ]
}
```

**Response (404 Not Found)**:
```json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": ["Stock with ID 999 not found"]
}
```

**Example cURL**:
```bash
curl -X PATCH "https://localhost:5001/api/stocks/1" \
  -H "Content-Type: application/json" \
  -d '{
    "symbol": "AAPL",
    "companyName": "Apple Inc.",
    "currentPrice": 275.00,
    "industry": "Technology",
    "marketCap": 2750000000000
  }'
```

---

### 5. Delete Stock

Deletes a stock and all associated records.

**Endpoint**: `DELETE /api/stocks/{id}`

**Path Parameters**:
- `id` (int, required): Stock ID

**Response (200 OK)**:
```json
{
  "success": true,
  "data": true,
  "message": "Stock deleted successfully",
  "errors": null
}
```

**Response (404 Not Found)**:
```json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": ["Stock with ID 999 not found"]
}
```

**Example cURL**:
```bash
curl -X DELETE "https://localhost:5001/api/stocks/1" \
  -H "Content-Type: application/json"
```

---

## Comment Endpoints

### 1. Create Comment

Creates a new comment on a stock.

**Endpoint**: `POST /api/stocks/{stockId}/comments`

**Request Headers**:
```
Content-Type: application/json
```

**Request Body**:
```json
{
  "title": "Great Investment",
  "content": "This stock has strong fundamentals and great growth potential",
  "rating": 5
}
```

**Response (201 Created)**:
```json
{
  "success": true,
  "data": 1,
  "message": "Comment created successfully",
  "errors": null
}
```

**Response (400 Bad Request)**:
```json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": [
    "Stock ID must be greater than 0",
    "Title is required and must be 3-200 characters",
    "Content must be 10-5000 characters",
    "Rating must be between 1 and 5"
  ]
}
```

**Response (404 Not Found)**:
```json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": ["Stock with ID 999 not found"]
}
```

**Example cURL**:
```bash
curl -X POST "https://localhost:5001/api/stocks/1/comments" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Great Investment",
    "content": "This stock has strong fundamentals and great growth potential",
    "rating": 5
  }'
```

---

### 2. Get All Comments

Retrieves all comments with optional pagination.

**Endpoint**: `GET /api/comments`

**Query Parameters**:
- `pageNumber` (int, optional): Page number (1-based)
- `pageSize` (int, optional): Items per page (max: 100)

**Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "stockId": 1,
        "title": "Great Investment",
        "content": "This stock has strong fundamentals and great growth potential",
        "rating": 5,
        "createdAt": "2026-03-11T14:20:00Z",
        "updatedAt": null
      }
    ],
    "pagination": {
      "totalCount": 1,
      "pageNumber": 1,
      "pageSize": 1,
      "totalPages": 1,
      "hasNextPage": false,
      "hasPreviousPage": false
    }
  },
  "message": "Comments retrieved successfully",
  "errors": null
}
```

**Example cURL**:
```bash
curl -X GET "https://localhost:5001/api/comments" \
  -H "Content-Type: application/json"
```

---

### 3. Get Comment by ID

Retrieves a specific comment by its ID.

**Endpoint**: `GET /api/comments/{id}`

**Path Parameters**:
- `id` (int, required): Comment ID

**Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "id": 1,
    "stockId": 1,
    "title": "Great Investment",
    "content": "This stock has strong fundamentals and great growth potential",
    "rating": 5,
    "createdAt": "2026-03-11T14:20:00Z",
    "updatedAt": null
  },
  "message": "Comment retrieved successfully",
  "errors": null
}
```

**Response (404 Not Found)**:
```json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": ["Comment with ID 999 not found"]
}
```

**Example cURL**:
```bash
curl -X GET "https://localhost:5001/api/comments/1" \
  -H "Content-Type: application/json"
```

---

### 4. Get Comments by Stock ID

Retrieves all comments for a specific stock.

**Endpoint**: `GET /api/stocks/{stockId}/comments`

**Path Parameters**:
- `stockId` (int, required): Stock ID

**Query Parameters**:
- `pageNumber` (int, optional): Page number (1-based)
- `pageSize` (int, optional): Items per page (max: 100)

**Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "stockId": 1,
        "title": "Great Investment",
        "content": "This stock has strong fundamentals and great growth potential",
        "rating": 5,
        "createdAt": "2026-03-11T14:20:00Z",
        "updatedAt": null
      },
      {
        "id": 2,
        "stockId": 1,
        "title": "Strong Performer",
        "content": "Consistent returns over the past 5 years",
        "rating": 4,
        "createdAt": "2026-03-11T14:25:00Z",
        "updatedAt": null
      }
    ],
    "pagination": {
      "totalCount": 2,
      "pageNumber": 1,
      "pageSize": 2,
      "totalPages": 1,
      "hasNextPage": false,
      "hasPreviousPage": false
    }
  },
  "message": "Comments retrieved successfully",
  "errors": null
}
```

**Response (404 Not Found)**:
```json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": ["Stock with ID 999 not found"]
}
```

**Example cURL**:
```bash
curl -X GET "https://localhost:5001/api/stocks/1/comments" \
  -H "Content-Type: application/json"
```

---

### 5. Update Comment

Updates an existing comment.

**Endpoint**: `PATCH /api/comments/{id}`

**Path Parameters**:
- `id` (int, required): Comment ID

**Request Body**:
```json
{
  "title": "Excellent Investment",
  "content": "Updated: This stock continues to show strong fundamentals",
  "rating": 5
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "data": true,
  "message": "Comment updated successfully",
  "errors": null
}
```

**Response (404 Not Found)**:
```json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": ["Comment with ID 999 not found"]
}
```

**Example cURL**:
```bash
curl -X PATCH "https://localhost:5001/api/comments/1" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Excellent Investment",
    "content": "Updated: This stock continues to show strong fundamentals",
    "rating": 5
  }'
```

---

### 6. Delete Comment

Deletes a comment.

**Endpoint**: `DELETE /api/comments/{id}`

**Path Parameters**:
- `id` (int, required): Comment ID

**Response (200 OK)**:
```json
{
  "success": true,
  "data": true,
  "message": "Comment deleted successfully",
  "errors": null
}
```

**Response (404 Not Found)**:
```json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": ["Comment with ID 999 not found"]
}
```

**Example cURL**:
```bash
curl -X DELETE "https://localhost:5001/api/comments/1" \
  -H "Content-Type: application/json"
```

---

## Response Format

All API responses follow a standardized format:

```typescript
interface ApiResponse<T> {
  success: boolean;           // Whether the request was successful
  data: T | null;             // Response data (null on error)
  message: string | null;     // User-friendly message
  errors: string[] | null;    // Array of error messages
}
```

---

## HTTP Status Codes

| Code | Meaning | Example |
|------|---------|---------|
| 200 | OK | Successful GET, PATCH, DELETE |
| 201 | Created | Successful POST |
| 400 | Bad Request | Validation errors, malformed data |
| 404 | Not Found | Resource doesn't exist |
| 409 | Conflict | Duplicate record, business rule violation |
| 500 | Internal Server Error | Unhandled exception |

---

## Error Responses

### Validation Error (400)
```json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": [
    "Symbol is required",
    "Current Price must be greater than 0"
  ]
}
```

### Not Found Error (404)
```json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": ["Stock with ID 999 not found"]
}
```

### Business Logic Error (409)
```json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": ["A stock with symbol 'AAPL' already exists"]
}
```

### Server Error (500)
```json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": ["An unexpected error occurred. Please try again later."]
}
```

---

## Request Examples by Tool

### PowerShell

```powershell
# POST - Create stock
$body = @{
    symbol = "AAPL"
    companyName = "Apple Inc."
    currentPrice = 250.50
    industry = "Technology"
    marketCap = 2500000000000
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5001/api/stocks" `
  -Method POST `
  -ContentType "application/json" `
  -Body $body `
  -SkipCertificateCheck

# GET - All stocks
Invoke-RestMethod -Uri "https://localhost:5001/api/stocks" `
  -Method GET `
  -SkipCertificateCheck

# GET - By ID
Invoke-RestMethod -Uri "https://localhost:5001/api/stocks/1" `
  -Method GET `
  -SkipCertificateCheck

# PATCH - Update
$updateBody = @{
    symbol = "AAPL"
    companyName = "Apple Inc."
    currentPrice = 275.00
    industry = "Technology"
    marketCap = 2750000000000
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5001/api/stocks/1" `
  -Method PATCH `
  -ContentType "application/json" `
  -Body $updateBody `
  -SkipCertificateCheck

# DELETE
Invoke-RestMethod -Uri "https://localhost:5001/api/stocks/1" `
  -Method DELETE `
  -SkipCertificateCheck
```

### VS Code REST Client (.http files)

```http
### Variables
@baseUrl = https://localhost:5001/api
@stockId = 1

### Create Stock
POST {{baseUrl}}/stocks
Content-Type: application/json

{
  "symbol": "AAPL",
  "companyName": "Apple Inc.",
  "currentPrice": 250.50,
  "industry": "Technology",
  "marketCap": 2500000000000
}

### Get All Stocks
GET {{baseUrl}}/stocks

### Get Stock by ID
GET {{baseUrl}}/stocks/{{stockId}}

### Update Stock
PATCH {{baseUrl}}/stocks/{{stockId}}
Content-Type: application/json

{
  "symbol": "AAPL",
  "companyName": "Apple Inc.",
  "currentPrice": 275.00,
  "industry": "Technology",
  "marketCap": 2750000000000
}

### Delete Stock
DELETE {{baseUrl}}/stocks/{{stockId}}
```

---

## Industry Values

Valid industry enum values:

```
Technology
Healthcare
Finance
Energy
Consumer
Industrial
Telecommunications
Utilities
RealEstate
Materials
Transportation
Retail
```

---

## Pagination

For large datasets, use pagination:

```bash
# Get first page (10 items)
GET /api/stocks?pageNumber=1&pageSize=10

# Get second page
GET /api/stocks?pageNumber=2&pageSize=10

# Change page size
GET /api/stocks?pageNumber=1&pageSize=50
```

Pagination metadata is included in `data.pagination` for `GET /api/stocks`, `GET /api/comments`, and `GET /api/stocks/{stockId}/comments`.

---

## Rate Limiting

Currently no rate limiting is implemented. Consider adding in production:
- 100 requests per minute per IP
- 1000 requests per hour per API key

---

## API Versioning

Future versions will be accessible via:
- `GET /api/v2/stocks` (example)
- Version headers: `Accept: application/vnd.finshark.v2+json`

---

## OpenAPI/Swagger

Access interactive API documentation:
- **Swagger UI**: `https://localhost:5001/swagger/index.html`
- **OpenAPI Schema**: `https://localhost:5001/openapi/v1.json`
