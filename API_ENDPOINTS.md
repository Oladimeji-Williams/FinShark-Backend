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

Retrieves all stocks with optional pagination and filtering.

**Endpoint**: `GET /api/stocks`

**Query Parameters**:
- `page` (int, optional): Page number (default: 1)
- `pageSize` (int, optional): Items per page (default: 10, max: 100)
- `industry` (string, optional): Filter by industry
- `symbol` (string, optional): Search by symbol (partial match)

**Response (200 OK)**:
```json
{
  "success": true,
  "data": [
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
curl -X GET "https://localhost:5001/api/stocks?page=1&pageSize=20" \
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

**Endpoint**: `PUT /api/stocks/{id}`

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
curl -X PUT "https://localhost:5001/api/stocks/1" \
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
| 200 | OK | Successful GET, PUT, DELETE |
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

# PUT - Update
$updateBody = @{
    symbol = "AAPL"
    companyName = "Apple Inc."
    currentPrice = 275.00
    industry = "Technology"
    marketCap = 2750000000000
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5001/api/stocks/1" `
  -Method PUT `
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
PUT {{baseUrl}}/stocks/{{stockId}}
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
GET /api/stocks?page=1&pageSize=10

# Get second page
GET /api/stocks?page=2&pageSize=10

# Change page size
GET /api/stocks?page=1&pageSize=50
```

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
