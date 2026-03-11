# Testing FinShark API

## 1. Database Setup & Migrations

First, ensure the database is created:

```powershell
# Navigate to project root
cd "c:\Users\OladimejiWilliams\Desktop\Software Engineering\FinShark\finshark-backend"

# Apply migrations (creates database and tables)
dotnet ef database update -p src/FinShark.Persistence -s src/FinShark.API
```

Expected output:
```
Build started...
Build succeeded.
Applying migration '20260310000000_InitialCreate'.
Done.
```

## 2. Run the Application

```powershell
cd src/FinShark.API
dotnet run
```

Expected output:
```
Building...
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

## 3. Test Individual Endpoints

### A. Create a Stock (POST)

```bash
curl -X POST "https://localhost:5001/api/stocks" \
  -H "Content-Type: application/json" \
  -d '{
    "symbol": "AAPL",
    "companyName": "Apple Inc.",
    "currentPrice": 250.50,
    "industry": "Technology",
    "marketCap": "2500000000000"
  }'
```

Expected Response (201 Created):
```json
{
  "success": true,
  "data": 1,
  "message": "Stock created successfully"
}
```

### B. Get All Stocks (GET)

```bash
curl -X GET "https://localhost:5001/api/stocks" \
  -H "Content-Type: application/json"
```

Expected Response (200 OK):
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
      "marketCap": "2500000000000",
      "createdAt": "2026-03-10T10:30:00Z",
      "updatedAt": null
    }
  ],
  "message": "Stocks retrieved successfully"
}
```

### C. Get Single Stock by ID (GET)

```bash
curl -X GET "https://localhost:5001/api/stocks/1" \
  -H "Content-Type: application/json"
```

Expected Response (200 OK):
```json
{
  "success": true,
  "data": {
    "id": 1,
    "symbol": "AAPL",
    "companyName": "Apple Inc.",
    "currentPrice": 250.50,
    "industry": "Technology",
    "marketCap": "2500000000000",
    "createdAt": "2026-03-10T10:30:00Z",
    "updatedAt": null
  },
  "message": "Stock retrieved successfully"
}
```

### D. Update a Stock (PUT)

```bash
curl -X PUT "https://localhost:5001/api/stocks/1" \
  -H "Content-Type: application/json" \
  -d '{
    "symbol": "AAPL",
    "companyName": "Apple Inc.",
    "currentPrice": 275.00,
    "industry": "Technology",
    "marketCap": "2750000000000"
  }'
```

Expected Response (200 OK):
```json
{
  "success": true,
  "data": true,
  "message": "Stock updated successfully"
}
```

### E. Delete a Stock (DELETE)

```bash
curl -X DELETE "https://localhost:5001/api/stocks/1" \
  -H "Content-Type: application/json"
```

Expected Response (200 OK):
```json
{
  "success": true,
  "data": true,
  "message": "Stock deleted successfully"
}
```

## 4. Test Error Scenarios

### Invalid Request (Missing Required Fields)

```bash
curl -X POST "https://localhost:5001/api/stocks" \
  -H "Content-Type: application/json" \
  -d '{
    "symbol": "",
    "companyName": "Apple Inc.",
    "currentPrice": 250.50
  }'
```

Expected Response (400 Bad Request):
```json
{
  "success": false,
  "data": null,
  "errors": [
    "Symbol is required",
    "Industry is required"
  ],
  "message": null
}
```

### Stock Not Found

```bash
curl -X GET "https://localhost:5001/api/stocks/999" \
  -H "Content-Type: application/json"
```

Expected Response (404 Not Found):
```json
{
  "success": false,
  "data": null,
  "errors": ["Stock with ID 999 not found."],
  "message": null
}
```

## 5. Test Using Visual Studio Code REST Client

Create a file `test-stocks.http` in the project root:

```http
### Variables
@baseUrl = https://localhost:5001
@contentType = application/json

### Create Stock
POST {{baseUrl}}/api/stocks
Content-Type: {{contentType}}

{
  "symbol": "MSFT",
  "companyName": "Microsoft Corporation",
  "currentPrice": 350.25,
  "industry": "Technology",
  "marketCap": "2600000000000"
}

### Get All Stocks
GET {{baseUrl}}/api/stocks
Content-Type: {{contentType}}

### Get Stock by ID
GET {{baseUrl}}/api/stocks/1
Content-Type: {{contentType}}

### Update Stock
PUT {{baseUrl}}/api/stocks/1
Content-Type: {{contentType}}

{
  "symbol": "MSFT",
  "companyName": "Microsoft Corporation",
  "currentPrice": 380.00,
  "industry": "Technology",
  "marketCap": "2800000000000"
}

### Delete Stock
DELETE {{baseUrl}}/api/stocks/1
Content-Type: {{contentType}}
```

Then use the "REST Client" extension to run these requests directly in VS Code.

## 6. Test Using Postman

1. Import this collection:

```json
{
  "info": {
    "name": "FinShark API",
    "description": "Test collection for FinShark Stock API"
  },
  "item": [
    {
      "name": "Create Stock",
      "request": {
        "method": "POST",
        "url": "https://localhost:5001/api/stocks",
        "header": [
          {"key": "Content-Type", "value": "application/json"}
        ],
        "body": {
          "mode": "raw",
          "raw": "{\"symbol\": \"AAPL\", \"companyName\": \"Apple Inc.\", \"currentPrice\": 250.50, \"industry\": \"Technology\", \"marketCap\": \"2500000000000\"}"
        }
      }
    }
  ]
}
```

## 7. Verify Logging Output

When running the application, check console logs for:

✅ **Configuration Loaded:**
```
Application environment: Development
```

✅ **Database Connection:**
```
Microsoft.EntityFrameworkCore.Database.Connection Information: Opened connection to database 'FinSharkDb'
```

✅ **Request Logging:**
```
GET /api/stocks - Retrieving all stocks
Retrieved 3 stocks
```

## 8. Check Database Directly

Open SQL Server Management Studio or use PowerShell:

```powershell
# Connect to LocalDB
sqlcmd -S "(localdb)\mssqllocaldb" -d "FinSharkDb"

# List tables
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES;

# View Stock data
SELECT * FROM dbo.Stocks;
```

## 9. Test Configuration Separation

✅ **CORS Configuration:**
- Should accept requests from allowed origins
- Should reject requests from blocked origins

✅ **Logging Configuration:**
- Development: Debug level logs
- Production: Warning level logs

✅ **Entity Configuration:**
- Symbol field: max 10 characters (enforced by DB constraint)
- CurrentPrice: precision 18,2 (decimal values)
- Audit fields: CreatedAt, UpdatedAt timestamps

## Troubleshooting

### SSL Certificate Error
```powershell
# If https has certificate issues, use http:
curl -X GET "http://localhost:5000/api/stocks"
```

### Database Not Found
```powershell
# Rerun migrations
dotnet ef database update -p src/FinShark.Persistence -s src/FinShark.API
```

### Port Already in Use
```powershell
# Check what's using port 5001
netstat -ano | findstr :5001

# Use different port in launchSettings.json
```

