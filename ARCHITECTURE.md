# FinShark Architecture Overview

Visual and detailed explanation of the clean architecture and system design.

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        CLIENT LAYER                             │
│              (Web Browser, Mobile App, Desktop)                  │
└─────────────────────────────────┬───────────────────────────────┘
                                  │
                    ┌─────────────▼─────────────┐
                    │   PRESENTATION LAYER      │
                    │   (Web/HTTP/REST)         │
                    │                           │
                    │  FinShark.API             │
                    │  ├── Controllers          │
                    │  ├── Middleware           │
                    │  └── Configuration        │
                    └─────────────┬─────────────┘
                                  │
                    ┌─────────────▼──────────────────┐
                    │   APPLICATION LAYER (CQRS)     │
                    │                                │
                    │  FinShark.Application          │
                    │  ├── Commands (Write)          │
                    │  ├── Queries (Read)            │
                    │  ├── Handlers (MediatR)        │
                    │  ├── DTOs (Data Transfer)      │
                    │  ├── Validators (FluentVal)    │
                    │  └── Mappers (Manual)          │
                    └─────────────┬──────────────────┘
                                  │
                    ┌─────────────▼──────────────┐
                    │   DOMAIN LAYER             │
                    │   (Pure Business Logic)    │
                    │                            │
                    │  FinShark.Domain           │
                    │  ├── Entities             │
                    │  ├── Interfaces           │
                    │  └── Business Rules       │
                    └─────────────┬──────────────┘
                                  │
        ┌─────────────────────────┼─────────────────────────┐
        │                         │                         │
┌───────▼────────────┐  ┌────────▼─────────┐  ┌──────────▼──────┐
│ PERSISTENCE LAYER  │  │INFRASTRUCTURE    │  │  EXTERNAL LAYER│
│                    │  │                  │  │                │
│FinShark.Persistence│  │FinShark.Infra    │  │  Services:     │
│ ├── DbContext      │  │ ├── Email        │  │  • PayPal API  │
│ ├── Repositories   │  │ ├── Cache        │  │  • Stock Feeds │
│ ├── Migrations     │  │ └── Logging      │  │  • Analytics   │
│ └── Configuration  │  │                  │  │                │
└───────┬────────────┘  └────────┬─────────┘  └──────────┬──────┘
        │                        │                       │
        └────────────────────────┼───────────────────────┘
                                 │
                    ┌────────────▼────────────┐
                    │  DATA LAYER             │
                    │                         │
                    │  • SQL Server Database  │
                    │  • Cache Store (Redis)  │
                    │  • File Storage         │
                    └─────────────────────────┘
```

## Layered Architecture Details

### 1. **Presentation Layer (API)**

**File**: `src/FinShark.API/`

Responsible for HTTP communication and client interaction.

**Components**:
- **Controllers** - Handle HTTP requests/responses
  - `StocksController` - Stock management endpoints
  
- **Middleware** - Cross-cutting concerns
  - `ExceptionMiddleware` - Global exception handling
  - Logging middleware (Serilog)
  - CORS middleware
  
- **Configuration** - Dependency injection & setup
  - `Program.cs` - Application startup
  - `AppConfiguration` - Register services
  - `MiddlewareConfiguration` - Setup middleware pipeline
  - `OpenApiConfiguration` - Swagger/OpenAPI setup

**Key Responsibilities**:
- Validate HTTP requests
- Transform DTOs to/from API contracts
- Return appropriate HTTP status codes
- Log all incoming/outgoing requests

---

### 2. **Application Layer (CQRS)**

**File**: `src/FinShark.Application/`

Implements business use cases using CQRS pattern with MediatR.

**Command Flow** (Write Operations):
```
HTTP POST Request
      ↓
StocksController
      ↓
CreateStockCommand (request)
      ↓
MediatR Pipeline
      ↓
CreateStockCommandValidator (FluentValidation)
      ↓
CreateStockCommandHandler
      ↓
IStockRepository.AddAsync()
      ↓
Database (Insert)
      ↓
ApiResponse<int> (ID)
      ↓
HTTP 201 Created Response
```

**Query Flow** (Read Operations):
```
HTTP GET Request
      ↓
StocksController
      ↓
GetStocksQuery (request)
      ↓
MediatR Pipeline
      ↓
GetStocksQueryHandler
      ↓
IStockRepository.GetAllAsync()
      ↓
Database (Select)
      ↓
StockMapper.ToDto()
      ↓
ApiResponse<List<StockDto>>
      ↓
HTTP 200 OK Response
```

**Components**:

| Component | Purpose | Example |
|-----------|---------|---------|
| **Commands** | Write operations | `CreateStockCommand`, `UpdateStockCommand` |
| **Queries** | Read operations | `GetStocksQuery`, `GetStockByIdQuery` |
| **Handlers** | Execute commands/queries | `CreateStockCommandHandler` |
| **Validators** | Input validation | `CreateStockValidator` |
| **DTOs** | Data contracts | `CreateStockRequestDto`, `StockDto` |
| **Mappers** | Entity ↔ DTO conversion | `StockMapper.ToDto()` |

---

### 3. **Domain Layer**

**File**: `src/FinShark.Domain/`

The heart of the system - pure business logic with no external dependencies.

**Entity Lifecycle**:
```
Request Data
    ↓
Validated
    ↓
Entity Created (constructor enforces rules)
    ↓
Invariants Checked (business rules)
    ↓
If Valid → Persisted
If Invalid → Exception Thrown
```

**Key Rules**:
- No dependencies on other layers
- No database knowledge
- No HTTP knowledge
- Pure C# business logic
- Constructors enforce invariants

**Components**:

```
Domain Layer
├── Entities/
│   ├── BaseEntity (Id, CreatedAt, UpdatedAt)
│   ├── Stock (business logic)
│   └── Comment (related to Stock)
│
├── Repositories/
│   ├── IRepository<T> (generic interface)
│   └── IStockRepository (specialized)
│
└── Interfaces/
    └── Domain contracts
```

---

### 4. **Persistence Layer**

**File**: `src/FinShark.Persistence/`

Data access implementation using Entity Framework Core.

**Components**:

| Component | Purpose |
|-----------|---------|
| **AppDbContext** | EF Core context with DbSets |
| **Repositories** | Implement domain interfaces |
| **Migrations** | Database schema changes |
| **EntityConfigurations** | Fluent API configurations |

**Data Flow**:
```
Command/Query Handler
    ↓
IStockRepository (interface from Domain)
    ↓
StockRepository (concrete implementation)
    ↓
AppDbContext.Stocks (EF Core)
    ↓
SQL Server Database
```

---

### 5. **Infrastructure Layer**

**File**: `src/FinShark.Infrastructure/`

External integrations and services:
- Email services
- Caching (Redis)
- File storage
- Third-party APIs

---

## Dependency Injection

Follows the **Dependency Inversion Principle**:

```csharp
// Program.cs
services.AddScoped<IStockRepository, StockRepository>();
services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(...));
```

**Flow**:
1. Interface injected into constructor
2. Concrete implementation provided by DI container
3. Easy to mock for testing
4. Loose coupling between layers

---

## Request Lifecycle

### Complete HTTP Request Journey

```
1. CLIENT SENDS REQUEST
   POST /api/stocks
   {symbol: "AAPL", ...}
            ↓
2. PRESENTATION LAYER
   StocksController.CreateStock()
            ↓
3. APPLICATION LAYER - CQRS
   new CreateStockCommand(...) → MediatR pipeline
            ↓
4. VALIDATION
   CreateStockValidator checks all rules
   If invalid → throw ValidationException
            ↓
5. COMMAND HANDLER
   CreateStockCommandHandler.Handle()
            ↓
6. DOMAIN LOGIC
   Create Stock entity (enforces invariants)
            ↓
7. PERSISTENCE
   IStockRepository.AddAsync()
   StockRepository.AddAsync() → DbContext.SaveChangesAsync()
            ↓
8. DATABASE
   INSERT INTO Stocks (Symbol, CompanyName, ...) VALUES (...)
            ↓
9. RESPONSE PREPARATION
   StockMapper.ToDto() → ApiResponse<int>
            ↓
10. CLIENT RECEIVES RESPONSE
    201 Created
    {success: true, data: 1, ...}
```

---

## Data Flow Diagram

### Create Stock Flow

```
┌────────────────────┐
│  HTTP POST Request │
└────────┬───────────┘
         │
         ▼
┌────────────────────────────┐
│  StocksController          │
│  CreateStock(request)      │
└────────┬───────────────────┘
         │
         ▼
┌────────────────────────────┐
│  CreateStockCommand        │
│  {Symbol, CompanyName ...} │
└────────┬───────────────────┘
         │
         ▼
┌────────────────────────────┐
│  MediatR Pipeline          │
│  Dispatch to Handler       │
└────────┬───────────────────┘
         │
         ▼
┌────────────────────────────┐
│  CreateStockValidator      │
│  FluentValidation.Validate │
│  If invalid → Error        │
└────────┬───────────────────┘
         │
         ▼
┌────────────────────────────┐
│  CreateStockCommandHandler │
│  .Handle(command)          │
└────────┬───────────────────┘
         │
         ▼
┌────────────────────────────┐
│  Domain Layer              │
│  new Stock(...)            │
│  Enforce Invariants        │
└────────┬───────────────────┘
         │
         ▼
┌────────────────────────────┐
│  IStockRepository          │
│  .AddAsync(stock)          │
└────────┬───────────────────┘
         │
         ▼
┌────────────────────────────┐
│  StockRepository           │
│  DbContext.Stocks.Add()    │
│  DbContext.SaveChangesAsync│
└────────┬───────────────────┘
         │
         ▼
┌────────────────────────────┐
│  SQL Server Database       │
│  INSERT INTO Stocks        │
└────────┬───────────────────┘
         │
         ▼
┌────────────────────────────┐
│  Return Stock ID           │
│  Map to Response           │
└────────┬───────────────────┘
         │
         ▼
┌────────────────────────────┐
│  HTTP 201 Created Response │
│  {success: true, data: 1}  │
└────────────────────────────┘
```

---

## Design Principles Applied

### 1. **Separation of Concerns**
Each layer has a single responsibility - UI, business logic, data access are separate.

### 2. **Dependency Inversion**
High-level modules depend on abstractions, not low-level details.

### 3. **SOLID Principles**
- **S**ingle Responsibility - Each class does one thing
- **O**pen/Closed - Open for extension, closed for modification
- **L**iskov Substitution - Implementations can be swapped
- **I**nterface Segregation - Specific interfaces over general ones
- **D**ependency Inversion - Depend on abstractions

### 4. **CQRS Pattern**
Commands (write) and Queries (read) separated for scalability.

### 5. **Testability**
Loose coupling enables easy mocking and unit testing.

---

## Technology Stack Mapping

```
┌─────────────────────────────────────────┐
│      Layer          │    Technology      │
├─────────────────────────────────────────┤
│ Presentation        │ ASP.NET Core 10    │
│ CQRS Orchestration  │ MediatR            │
│ Validation          │ FluentValidation   │
│ Data Mapping        │ Manual Mappers     │
│ ORM                 │ Entity Framework   │
│ Database            │ SQL Server 2019+   │
│ Logging             │ Serilog            │
│ Testing             │ xUnit + Moq        │
└─────────────────────────────────────────┘
```

---

## Scalability Considerations

### Current Architecture
- Single database unit
- Synchronous operations
- In-process MediatR

### Future Enhancements Available
- **CQRS Read/Write Separation** - Separate read & write databases
- **Event Sourcing** - Event-driven architecture
- **Microservices** - Split into independent services
- **Async Messaging** - RabbitMQ, Azure Service Bus
- **Caching Layer** - Redis for frequently accessed data
- **API Gateway** - Kong, AWS API Gateway
- **Containerization** - Docker, Kubernetes
