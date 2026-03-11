# FinShark Backend - Quick Reference Guide

Quick lookup guide for common commands, patterns, and snippets.

## Table of Contents

1. [CLI Commands](#cli-commands)
2. [Project Structure](#project-structure)
3. [Code Snippets](#code-snippets)
4. [Common Tasks](#common-tasks)
5. [Architecture Quick Guide](#architecture-quick-guide)

---

## CLI Commands

### Build & Run

```bash
# Build the solution
dotnet build

# Build specific project
dotnet build src/FinShark.API

# Run the application
dotnet run --project src/FinShark.API

# Run with production settings
dotnet run --project src/FinShark.API --configuration Release

# Clean everything
dotnet clean
```

### Database Management

```bash
# Create a new migration
dotnet ef migrations add MigrationName -p src/FinShark.Persistence

# Update database to latest migration
dotnet ef database update

# Update to specific migration
dotnet ef database update MigrationName

# Revert last migration
dotnet ef database update [PreviousMigrationName]

# List all migrations
dotnet ef migrations list

# Remove last migration (if not applied)
dotnet ef migrations remove
```

### Package Management

```bash
# Add package to project
dotnet add src/FinShark.Application package MediatR

# Remove package
dotnet remove src/FinShark.Application package MediatR

# Update all packages
dotnet package update

# Restore packages
dotnet restore
```

### Testing

```bash
# Run all tests
dotnet test

# Run tests with logging
dotnet test --verbosity detailed

# Run specific test
dotnet test --filter "ClassName.MethodName"
```

### Solution Management

```bash
# Add project to solution
dotnet sln add src/FinShark.Domain/FinShark.Domain.csproj

# List solution projects
dotnet sln list

# Remove project from solution
dotnet sln remove src/FinShark.API/FinShark.API.csproj
```

---

## Project Structure

```
src/
├── FinShark.Domain/
│   ├── Entities/              # Business entities (Stock, BaseEntity)
│   ├── Repositories/          # Repository interfaces (IStockRepository)
│   └── Interfaces/            # Other interfaces (IRepository<T>)
│
├── FinShark.Application/
│   ├── Stocks/
│   │   ├── Commands/          # Command definitions (CreateStockCommand)
│   │   ├── Queries/           # Query definitions (GetStocksQuery)
│   │   └── Validators/        # FluentValidation rules
│   ├── Dtos/                  # Data transfer objects
│   ├── Mappers/               # Manual mappers
│   └── ServiceCollectionExtensions.cs
│
├── FinShark.Persistence/
│   ├── Repositories/          # Repository implementations
│   ├── AppDbContext.cs        # EF Core context
│   └── ServiceCollectionExtensions.cs
│
├── FinShark.Infrastructure/
│   └── ServiceCollectionExtensions.cs
│
└── FinShark.API/
    ├── Controllers/           # API endpoints
    ├── Middleware/            # Middleware components
    ├── Program.cs            # Application startup
    └── Properties/
        └── launchSettings.json
```

---

## Code Snippets

### Entity Definition Template

```csharp
namespace FinShark.Domain.Entities;

public class MyEntity : BaseEntity
{
    public string Name { get; private set; }

    private MyEntity() { } // EF Core

    public MyEntity(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        
        Name = name;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
            Name = name;
        
        UpdatedAt = DateTime.UtcNow;
    }
}
```

### Command Definition Template

```csharp
using MediatR;

namespace FinShark.Application.Stocks.Commands.CreateMyEntity;

public sealed record CreateMyEntityCommand(
    string Name,
    string Description
) : IRequest<int>;
```

### Query Definition Template

```csharp
using FinShark.Application.Dtos;
using MediatR;

namespace FinShark.Application.Stocks.Queries.GetMyEntities;

public sealed record GetMyEntitiesQuery() : IRequest<IEnumerable<MyEntityDto>>;
```

### Command Handler Template

```csharp
using FinShark.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinShark.Application.Stocks.Commands.CreateMyEntity;

public sealed class CreateMyEntityCommandHandler : IRequestHandler<CreateMyEntityCommand, int>
{
    private readonly IMyEntityRepository _repository;
    private readonly ILogger<CreateMyEntityCommandHandler> _logger;

    public CreateMyEntityCommandHandler(
        IMyEntityRepository repository,
        ILogger<CreateMyEntityCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<int> Handle(CreateMyEntityCommand request, CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        _logger.LogInformation("Creating entity: {Name}", request.Name);

        try
        {
            var entity = new Domain.Entities.MyEntity(request.Name);
            await _repository.AddAsync(entity);
            
            _logger.LogInformation("Entity created with ID: {Id}", entity.Id);
            return entity.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating entity: {Name}", request.Name);
            throw;
        }
    }
}
```

### Query Handler Template

```csharp
using FinShark.Application.Dtos;
using FinShark.Application.Mappers;
using FinShark.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinShark.Application.Stocks.Queries.GetMyEntities;

public sealed class GetMyEntitiesQueryHandler : IRequestHandler<GetMyEntitiesQuery, IEnumerable<MyEntityDto>>
{
    private readonly IMyEntityRepository _repository;
    private readonly ILogger<GetMyEntitiesQueryHandler> _logger;

    public GetMyEntitiesQueryHandler(
        IMyEntityRepository repository,
        ILogger<GetMyEntitiesQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<MyEntityDto>> Handle(
        GetMyEntitiesQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all entities");

        try
        {
            var entities = await _repository.GetAllAsync();
            return MyEntityMapper.ToDtoList(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving entities");
            throw;
        }
    }
}
```

### Repository Implementation Template

```csharp
using FinShark.Domain.Entities;
using FinShark.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinShark.Persistence.Repositories;

public sealed class MyEntityRepository : IMyEntityRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<MyEntityRepository> _logger;

    public MyEntityRepository(AppDbContext context, ILogger<MyEntityRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<MyEntity>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all entities");
        return await _context.MyEntities.ToListAsync();
    }

    public async Task<MyEntity?> GetByIdAsync(int id)
    {
        return await _context.MyEntities.FindAsync(id);
    }

    public async Task AddAsync(MyEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        
        _logger.LogInformation("Adding entity");
        await _context.MyEntities.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(MyEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        
        _logger.LogInformation("Deleting entity with ID: {Id}", entity.Id);
        _context.MyEntities.Remove(entity);
        await _context.SaveChangesAsync();
    }
}
```

### FluentValidation Template

```csharp
using FinShark.Application.Dtos;
using FluentValidation;

namespace FinShark.Application.Stocks.Validators;

public sealed class CreateMyEntityValidator : AbstractValidator<CreateMyEntityRequestDto>
{
    public CreateMyEntityValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage("Name is required.")
            .MaximumLength(100)
                .WithMessage("Name cannot exceed 100 characters.")
            .Matches(@"^[a-zA-Z\s]+$")
                .WithMessage("Name must contain only letters and spaces.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters.");
    }
}
```

### Controller Template

```csharp
using FinShark.Application.Dtos;
using FinShark.Application.Stocks.Commands.CreateMyEntity;
using FinShark.Application.Stocks.Queries.GetMyEntities;
using FinShark.Application.Stocks.Validators;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FinShark.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class MyEntitiesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<CreateMyEntityRequestDto> _validator;
    private readonly ILogger<MyEntitiesController> _logger;

    public MyEntitiesController(
        IMediator mediator,
        IValidator<CreateMyEntityRequestDto> validator,
        ILogger<MyEntitiesController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<MyEntityDto>>>> GetAll(CancellationToken ct)
    {
        _logger.LogInformation("GET /api/myentities");
        
        var result = await _mediator.Send(new GetMyEntitiesQuery(), ct);
        var response = ApiResponse<IEnumerable<MyEntityDto>>.SuccessResponse(result);
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> Create(
        [FromBody] CreateMyEntityRequestDto request,
        CancellationToken ct)
    {
        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<int>.FailureResponse(errors));
        }

        var command = new CreateMyEntityCommand(request.Name, request.Description);
        var id = await _mediator.Send(command, ct);

        var response = ApiResponse<int>.SuccessResponse(id, "Entity created successfully");
        return CreatedAtAction(nameof(GetAll), new { id }, response);
    }
}
```

### Mapper Template

```csharp
using FinShark.Application.Dtos;
using FinShark.Domain.Entities;

namespace FinShark.Application.Mappers;

public sealed class MyEntityMapper
{
    public static MyEntityDto ToDto(MyEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        return new MyEntityDto
        {
            Id = entity.Id,
            Name = entity.Name,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public static IEnumerable<MyEntityDto> ToDtoList(IEnumerable<MyEntity> entities)
    {
        if (entities == null) throw new ArgumentNullException(nameof(entities));
        return entities.Select(ToDto);
    }

    public static MyEntity ToEntity(CreateMyEntityRequestDto request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        return new MyEntity(request.Name);
    }
}
```

---

## Common Tasks

### Add New Feature Checklist

- [ ] Create entity in Domain/Entities/
- [ ] Create repository interface in Domain/Repositories/
- [ ] Create DTOs in Application/Dtos/
- [ ] Create mapper in Application/Mappers/
- [ ] Create validator in Application/Stocks/Validators/
- [ ] Create command in Application/Stocks/Commands/
- [ ] Create command handler
- [ ] Create query in Application/Stocks/Queries/
- [ ] Create query handler
- [ ] Create repository implementation in Persistence/Repositories/
- [ ] Create controller in API/Controllers/
- [ ] Register in DI containers
- [ ] Update AppDbContext
- [ ] Create database migration
- [ ] Update database

### Fix Common Issues

| Issue | Solution |
|-------|----------|
| "Connection string not found" | Add to appsettings.json |
| Port already in use | Change in launchSettings.json or kill process |
| Build fails | `dotnet clean && dotnet restore && dotnet build` |
| Migration fails | Check DbContext configuration |
| DI container error | Register in ServiceCollectionExtensions |

### Code Style Guidelines

- Use `sealed` for non-abstract classes
- Use `required` for mandatory constructor parameters
- Use `private set;` for domain invariants
- Always validate in constructors
- Always log in handlers
- Always wrap HTTP responses
- Always return `ActionResult<ApiResponse<T>>`

---

## Architecture Quick Guide

### CQRS Pattern

```
User Input
    ↓
Controller Receives Request
    ↓
Validate with FluentValidator
    ↓
Create Command/Query
    ↓
MediatR Dispatches to Handler
    ↓
Handler Calls Repository
    ↓
Repository Accesses Database
    ↓
Mapper Transforms to DTO
    ↓
Wrap in ApiResponse<T>
    ↓
Return to Client
```

### Dependency Injection Flow

```
Program.cs
├── AddApplicationServices()
│   ├── MediatR (handlers)
│   ├── Validators
│   └── Mappers
├── AddPersistenceServices()
│   ├── DbContext
│   └── Repositories
└── AddInfrastructureServices()
    └── External Services
```

### Request/Response Flow

```
HTTP Request
    ↓
[Authorize] Middleware
    ↓
ExceptionMiddleware
    ↓
Controller Action
    ↓
Validate InputDto
    ↓
MediatR.Send(Command/Query)
    ↓
Handler (CQRS)
    ↓
Repository
    ↓
DbContext
    ↓
Database
    ↓
Mapper (DTO)
    ↓
ApiResponse<T>
    ↓
HTTP Response (200/201/400/500)
```

### Layer Interactions

```
API Layer (Controllers)
    ↓ (Calls via MediatR)
Application Layer (Handlers, Validators)
    ↓ (Calls)
Domain Layer (Entities, Interfaces)
    ↓
Persistence Layer (DbContext, Repositories)
    ↓
Database
```

---

## Environment Variables

### Development

```bash
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Server=(local);Database=FinSharkDb_Dev;...
```

### Production

```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=server-prod-connection-string
ASPNETCORE_Urls=https://+:443;http://+:80
```

---

## Logging Levels Reference

| Level | Usage | Example |
|-------|-------|---------|
| Trace | Very detailed | Variable values |
| Debug | Detailed debug info | Method entry/exit |
| Information | General info | "User {Name} created" |
| Warning | Non-critical issues | "Retry attempt 2/3" |
| Error | Errors | "Failed to save entity" |
| Critical | Critical failures | "Database connection lost" |

---

## HTTP Status Codes

| Code | Usage |
|------|-------|
| 200 | GET/PUT success |
| 201 | POST success (Created) |
| 204 | DELETE success (No Content) |
| 400 | Validation error |
| 401 | Unauthorized |
| 403 | Forbidden |
| 404 | Not Found |
| 500 | Server error |

---

## Useful Links

- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [MediatR GitHub](https://github.com/jbogard/MediatR)
- [FluentValidation](https://fluentvalidation.net/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
