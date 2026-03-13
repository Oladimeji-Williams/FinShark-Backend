# FinShark Backend API

Enterprise-grade stock market financial application backend built with .NET 10, following clean architecture, CQRS pattern, and domain-driven design principles.

## 🎯 Project Overview

FinShark is a comprehensive backend API for stock management and financial data analysis. The architecture emphasizes:

- **Clean Architecture** - Separation of concerns across layers
- **CQRS Pattern** - Command Query Responsibility Segregation
- **Domain-Driven Design** - Business logic at the core
- **Type Safety** - Comprehensive typing with C# 12+ features
- **Testability** - Loosely coupled, highly testable code
- **Logging & Monitoring** - Structured logging throughout
- **Validation** - FluentValidation for comprehensive input validation

## 🏗️ Architecture Layers

### Domain Layer (`FinShark.Domain`)

Contains pure business logic with no external dependencies:
- **Entities** - Stock, BaseEntity
- **Interfaces** - Repository contracts (IRepository<T>, IStockRepository)
- **Business Rules** - Validation, invariants in constructors

### Application Layer (`FinShark.Application`)

CQRS implementation with use cases:
- **Commands** - State-changing operations (CreateStockCommand)
- **Queries** - Read-only operations (GetStocksQuery)
- **Handlers** - MediatR handlers for commands and queries
- **Validators** - FluentValidation rules
- **DTOs** - Data transfer objects for API contracts
- **Mappers** - Manual entity-to-DTO transformations

### Persistence Layer (`FinShark.Persistence`)

Data access and database concerns:
- **AppDbContext** - Entity Framework Core context
- **Repositories** - Concrete data access implementations
- **Migrations** - EF Core database migrations

### Infrastructure Layer (`FinShark.Infrastructure`)

External service integrations (email, cache, etc.)

### API Layer (`FinShark.API`)

HTTP endpoints and middleware:
- **Controllers** - REST API endpoints
- **Middleware** - Exception handling, request/response processing
- **Program.cs** - Application configuration and dependency injection

## 🚀 Quick Start

### Prerequisites

- .NET 10.0 SDK or later
- SQL Server 2019 or later
- Git

### Setup

```bash
# Clone repository
git clone <repository-url>
cd finshark-backend

# Restore packages
dotnet restore

# Configure connection string in appsettings.json
# Then create database and run migrations
cd src/FinShark.API
dotnet ef database update

# Run the application
dotnet run
```

The API will be available at:
- **HTTP**: `http://localhost:5192`
- **HTTPS**: `https://localhost:7235`

## 📚 Documentation

- **[SETUP.md](SETUP.md)** - Detailed setup instructions and troubleshooting
- **[IMPLEMENTATION.md](IMPLEMENTATION.md)** - Complete guide for implementing new features
- **[QUICK_REFERENCE.md](QUICK_REFERENCE.md)** - Quick lookup for commands and patterns

## 📁 Project Structure

```
finshark-backend/
├── src/
│   ├── FinShark.Domain/              # Business entities & interfaces
│   │   ├── Entities/
│   │   │   ├── BaseEntity.cs
│   │   │   └── Stock.cs
│   │   └── Repositories/
│   │       └── IStockRepository.cs
│   │
│   ├── FinShark.Application/         # CQRS & use cases
│   │   ├── Stocks/
│   │   │   ├── Commands/
│   │   │   │   └── CreateStock/
│   │   │   ├── Queries/
│   │   │   │   └── GetStocks/
│   │   │   └── Validators/
│   │   ├── Dtos/
│   │   │   ├── StockDto.cs
│   │   │   └── ApiResponse.cs
│   │   ├── Mappers/
│   │   │   └── StockMapper.cs
│   │   └── ServiceCollectionExtensions.cs
│   │
│   ├── FinShark.Persistence/        # Data access
│   │   ├── Repositories/
│   │   │   └── StockRepository.cs
│   │   ├── AppDbContext.cs
│   │   └── ServiceCollectionExtensions.cs
│   │
│   ├── FinShark.Infrastructure/     # External services
│   │   └── ServiceCollectionExtensions.cs
│   │
│   └── FinShark.API/                # REST API
│       ├── Controllers/
│       │   └── StocksController.cs
│       ├── Middleware/
│       │   └── ExceptionMiddleware.cs
│       ├── Program.cs
│       └── appsettings.json
│
├── tests/                           # Unit & integration tests
├── FinShark.slnx                    # Solution file
├── SETUP.md                         # Setup guide
├── IMPLEMENTATION.md                # Feature implementation guide
├── QUICK_REFERENCE.md              # Quick reference
└── README.md                        # This file
```

## 🔄 CQRS Pattern Overview

The project implements Command Query Responsibility Segregation:

**Commands** (State-Changing)
```csharp
public sealed record CreateStockCommand(string Symbol, string CompanyName, decimal Price) 
    : IRequest<int>;
```

**Queries** (Read-Only)
```csharp
public sealed record GetStocksQuery(StockQueryParameters QueryParameters) 
    : IRequest<PagedResult<GetStockResponseDto>>;
```

Dispatched via MediatR:
```csharp
var id = await _mediator.Send(new CreateStockCommand(...));
var stocks = await _mediator.Send(new GetStocksQuery(new StockQueryParameters()));
```

## 🗄️ Database

### Entity Relationship

```
Stock (1) ──────── (N) Dividend
├── Id (PK)         ├── Id (PK)
├── Symbol          ├── StockId (FK)
├── CompanyName     ├── Amount
├── CurrentPrice    ├── PaymentDate
├── Industry        ├── RecordDate
├── MarketCap       ├── ExDividendDate
├── CreatedAt       └── CreatedAt
└── UpdatedAt
```

### Connection String

```
Server=(local);Database=FinSharkDb;Trusted_Connection=true;TrustServerCertificate=true;
```

## 🔐 API Endpoints

### Stocks

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/stocks` | Get all stocks |
| POST | `/api/stocks` | Create new stock |
| GET | `/api/stocks/{id}` | Get stock by ID |
| PATCH | `/api/stocks/{id}` | Update stock |
| DELETE | `/api/stocks/{id}` | Delete stock |

### Request/Response Example

**POST /api/stocks**

Request:
```json
{
  "symbol": "AAPL",
  "companyName": "Apple Inc.",
  "currentPrice": 150.50,
  "industry": "Technology",
  "marketCap": "2.5T"
}
```

Response (201 Created):
```json
{
  "success": true,
  "data": 1,
  "message": "Stock created successfully",
  "errors": null
}
```

## 🧪 Testing

### Run Tests

```bash
dotnet test
```

### Test Structure

```
tests/
├── FinShark.Domain.Tests/
│   └── Entities/
├── FinShark.Application.Tests/
│   └── Handlers/
└── FinShark.API.Tests/
    └── Controllers/
```

## 🛠️ Development Workflow

### Add New Feature

1. Create entity in Domain layer
2. Define repository interface
3. Create DTOs and mapper
4. Create FluentValidation validator
5. Create CQRS command/query
6. Create handler
7. Implement repository
8. Create API controller
9. Register in DI container
10. Create database migration
11. Update database

See [IMPLEMENTATION.md](IMPLEMENTATION.md) for detailed guide.

### Build & Run

```bash
# Build
dotnet build

# Run
dotnet run --project src/FinShark.API

# Run with watch (auto-reload)
dotnet watch run --project src/FinShark.API
```

## 📦 Dependencies

### Core Frameworks
- `.NET 10.0` - Modern C# runtime
- `ASP.NET Core 10.0` - Web framework
- `Entity Framework Core 10.0.4` - ORM

### Libraries
- `MediatR 14.1.0` - CQRS implementation
- `FluentValidation 12.1.1` - Input validation
- `SQL Server Driver` - Database access

### Development
- `xUnit` - Unit testing
- `Moq` - Mocking library

## 🚨 Error Handling

Global exception middleware handles:
- Validation errors → 400 BadRequest
- Not found errors → 404 NotFound  
- General errors → 500 InternalServerError

All responses wrapped in `ApiResponse<T>`:
```json
{
  "success": false,
  "data": null,
  "message": "Validation failed",
  "errors": ["Symbol cannot be empty"]
}
```

## 📊 Logging

Structured logging with Microsoft.Extensions.Logging:

```
[INF] Creating new stock with symbol: AAPL, company: Apple Inc.
[INF] Stock created successfully with ID: 1
[ERR] Error creating stock with symbol: INVALID
```

View logs in:
- Console output
- Application Insights (if configured)
- Log files (if configured)

## 🔧 Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=FinSharkDb;..."
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

### Environment-Specific

- `appsettings.Development.json` - Development settings
- Environment variables - Production secrets

## 🎓 Learning Resources

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- [MediatR Documentation](https://github.com/jbogard/MediatR/wiki)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)

## 📝 Code Style Guidelines

- Use `sealed` classes for non-abstract types
- Use `required` for mandatory properties
- Use `private set;` for domain invariants
- Always validate in constructors
- Log at appropriate levels
- Use meaningful variable names
- XML documentation for public APIs

## 🤝 Contributing

1. Create feature branch: `git checkout -b feature/new-feature`
2. Commit changes: `git commit -am 'Add new feature'`
3. Push to branch: `git push origin feature/new-feature`
4. Submit pull request

Follow the implementation guide and code style guidelines.

## 📄 License

This project is proprietary and confidential.

## 👥 Team

- **Backend Architects**: Engineering Team
- **Database Design**: Data Engineering Team

## 📞 Support

For issues or questions:
1. Check [SETUP.md](SETUP.md) troubleshooting section
2. Review [IMPLEMENTATION.md](IMPLEMENTATION.md) for patterns
3. Consult [QUICK_REFERENCE.md](QUICK_REFERENCE.md) for commands

## 🎯 Roadmap

### Version 1.1
- [ ] Add authentication/authorization
- [ ] Implement caching layer
- [ ] Add API rate limiting
- [ ] Create unit tests

### Version 2.0
- [ ] Add real-time stock data
- [ ] Portfolio management
- [ ] Advanced analytics
- [ ] Mobile API support

---

**Last Updated**: March 2026  
**Version**: 1.0.0  
**Status**: Production Ready ✅
