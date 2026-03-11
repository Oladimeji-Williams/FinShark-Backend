# FinShark Backend - Implementation Guide

Complete guide for implementing new features following CQRS, clean architecture, and domain-driven design patterns.

## Table of Contents

1. [Adding a New Feature](#adding-a-new-feature)
2. [Domain Layer Implementation](#domain-layer-implementation)
3. [Application Layer (CQRS)](#application-layer-cqrs)
4. [Persistence Layer](#persistence-layer)
5. [API Layer](#api-layer)
6. [Testing](#testing)
7. [Common Patterns](#common-patterns)

---

## Adding a New Feature

### Example: Stock Dividend Management

Let's implement a dividend feature for stocks. Follow these steps in order:

### Step 1: Define Domain Entity

**File**: `src/FinShark.Domain/Entities/Dividend.cs`

```csharp
namespace FinShark.Domain.Entities;

/// <summary>
/// Represents a stock dividend
/// Maintains domain invariants and business logic
/// </summary>
public class Dividend : BaseEntity
{
    public int StockId { get; private set; }
    public required decimal Amount { get; private set; }
    public required DateTime PaymentDate { get; private set; }
    public required DateTime RecordDate { get; private set; }
    public required DateTime ExDividendDate { get; private set; }
    public string Notes { get; set; } = string.Empty;

    // Navigation property
    public Stock Stock { get; private set; } = null!;

    private Dividend() { } // EF Core

    public Dividend(int stockId, decimal amount, DateTime paymentDate, 
        DateTime recordDate, DateTime exDividendDate)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive", nameof(amount));
        if (paymentDate < DateTime.UtcNow.Date) 
            throw new ArgumentException("Payment date cannot be in the past", nameof(paymentDate));

        StockId = stockId;
        Amount = amount;
        PaymentDate = paymentDate;
        RecordDate = recordDate;
        ExDividendDate = exDividendDate;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(decimal? amount = null, DateTime? paymentDate = null,
        DateTime? recordDate = null, DateTime? exDividendDate = null, string? notes = null)
    {
        if (amount.HasValue && amount.Value > 0) Amount = amount.Value;
        if (paymentDate.HasValue) PaymentDate = paymentDate.Value;
        if (recordDate.HasValue) RecordDate = recordDate.Value;
        if (exDividendDate.HasValue) ExDividendDate = exDividendDate.Value;
        if (!string.IsNullOrWhiteSpace(notes)) Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

### Step 2: Create Domain Repository Interface

**File**: `src/FinShark.Domain/Repositories/IDividendRepository.cs`

```csharp
using FinShark.Domain.Entities;

namespace FinShark.Domain.Repositories;

/// <summary>
/// Repository interface for Dividend entity
/// Defines contracts for dividend data access
/// </summary>
public interface IDividendRepository
{
    Task<Dividend?> GetByIdAsync(int id);
    Task<IEnumerable<Dividend>> GetByStockIdAsync(int stockId);
    Task<IEnumerable<Dividend>> GetAllAsync();
    Task AddAsync(Dividend dividend);
    Task UpdateAsync(Dividend dividend);
    Task DeleteAsync(Dividend dividend);
}
```

### Step 3: Create DTOs

**File**: `src/FinShark.Application/Dtos/DividendDto.cs`

```csharp
namespace FinShark.Application.Dtos;

/// <summary>
/// DTO for reading dividend data
/// </summary>
public sealed class DividendDto
{
    public required int Id { get; init; }
    public required int StockId { get; init; }
    public required decimal Amount { get; init; }
    public required DateTime PaymentDate { get; init; }
    public required DateTime RecordDate { get; init; }
    public required DateTime ExDividendDate { get; init; }
    public string Notes { get; init; } = string.Empty;
    public required DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// DTO for creating a dividend
/// </summary>
public sealed class CreateDividendRequestDto
{
    public required int StockId { get; init; }
    public required decimal Amount { get; init; }
    public required DateTime PaymentDate { get; init; }
    public required DateTime RecordDate { get; init; }
    public required DateTime ExDividendDate { get; init; }
    public string Notes { get; init; } = string.Empty;
}
```

### Step 4: Create Mapper

**File**: `src/FinShark.Application/Mappers/DividendMapper.cs`

```csharp
using FinShark.Application.Dtos;
using FinShark.Domain.Entities;

namespace FinShark.Application.Mappers;

/// <summary>
/// Manual mapper for Dividend entity to/from DTOs
/// </summary>
public sealed class DividendMapper
{
    public static DividendDto ToDto(Dividend dividend)
    {
        if (dividend == null) throw new ArgumentNullException(nameof(dividend));

        return new DividendDto
        {
            Id = dividend.Id,
            StockId = dividend.StockId,
            Amount = dividend.Amount,
            PaymentDate = dividend.PaymentDate,
            RecordDate = dividend.RecordDate,
            ExDividendDate = dividend.ExDividendDate,
            Notes = dividend.Notes,
            CreatedAt = dividend.CreatedAt,
            UpdatedAt = dividend.UpdatedAt
        };
    }

    public static IEnumerable<DividendDto> ToDtoList(IEnumerable<Dividend> dividends)
    {
        if (dividends == null) throw new ArgumentNullException(nameof(dividends));
        return dividends.Select(ToDto);
    }

    public static Dividend ToEntity(CreateDividendRequestDto request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        return new Dividend(
            request.StockId,
            request.Amount,
            request.PaymentDate,
            request.RecordDate,
            request.ExDividendDate
        )
        {
            Notes = request.Notes
        };
    }
}
```

### Step 5: Create FluentValidation Validator

**File**: `src/FinShark.Application/Stocks/Validators/CreateDividendValidator.cs`

```csharp
using FinShark.Application.Dtos;
using FluentValidation;

namespace FinShark.Application.Stocks.Validators;

/// <summary>
/// Validator for dividend creation
/// </summary>
public sealed class CreateDividendValidator : AbstractValidator<CreateDividendRequestDto>
{
    public CreateDividendValidator()
    {
        RuleFor(x => x.StockId)
            .GreaterThan(0)
                .WithMessage("Stock ID must be greater than zero.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
                .WithMessage("Dividend amount must be greater than zero.")
            .LessThanOrEqualTo(decimal.MaxValue)
                .WithMessage("Dividend amount is too large.");

        RuleFor(x => x.PaymentDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("Payment date must be in the future.");

        RuleFor(x => x.RecordDate)
            .LessThan(x => x.PaymentDate)
                .WithMessage("Record date must be before payment date.");

        RuleFor(x => x.ExDividendDate)
            .LessThan(x => x.RecordDate)
                .WithMessage("Ex-dividend date must be before record date.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
                .WithMessage("Notes cannot exceed 500 characters.");
    }
}
```

### Step 6: Create CQRS Commands and Queries

**File**: `src/FinShark.Application/Stocks/Commands/CreateDividend/CreateDividendCommand.cs`

```csharp
using MediatR;

namespace FinShark.Application.Stocks.Commands.CreateDividend;

/// <summary>
/// Command to create a dividend
/// </summary>
public sealed record CreateDividendCommand(
    int StockId,
    decimal Amount,
    DateTime PaymentDate,
    DateTime RecordDate,
    DateTime ExDividendDate,
    string Notes = ""
) : IRequest<int>;
```

**File**: `src/FinShark.Application/Stocks/Queries/GetDividends/GetDividendsByStockQuery.cs`

```csharp
using FinShark.Application.Dtos;
using MediatR;

namespace FinShark.Application.Stocks.Queries.GetDividends;

/// <summary>
/// Query to get dividends for a specific stock
/// </summary>
public sealed record GetDividendsByStockQuery(int StockId) : IRequest<IEnumerable<DividendDto>>;
```

### Step 7: Create Command/Query Handlers

**File**: `src/FinShark.Application/Stocks/Commands/CreateDividend/CreateDividendCommandHandler.cs`

```csharp
using FinShark.Application.Mappers;
using FinShark.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinShark.Application.Stocks.Commands.CreateDividend;

/// <summary>
/// Handler for creating a dividend
/// </summary>
public sealed class CreateDividendCommandHandler : IRequestHandler<CreateDividendCommand, int>
{
    private readonly IDividendRepository _dividendRepository;
    private readonly ILogger<CreateDividendCommandHandler> _logger;

    public CreateDividendCommandHandler(
        IDividendRepository dividendRepository,
        ILogger<CreateDividendCommandHandler> logger)
    {
        _dividendRepository = dividendRepository ?? throw new ArgumentNullException(nameof(dividendRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<int> Handle(CreateDividendCommand request, CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        _logger.LogInformation("Creating dividend for stock {StockId}, amount: {Amount}", 
            request.StockId, request.Amount);

        try
        {
            var dividend = new Domain.Entities.Dividend(
                request.StockId,
                request.Amount,
                request.PaymentDate,
                request.RecordDate,
                request.ExDividendDate
            )
            {
                Notes = request.Notes
            };

            await _dividendRepository.AddAsync(dividend);

            _logger.LogInformation("Dividend created with ID: {DividendId}", dividend.Id);

            return dividend.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating dividend for stock: {StockId}", request.StockId);
            throw;
        }
    }
}
```

**File**: `src/FinShark.Application/Stocks/Queries/GetDividends/GetDividendsByStockQueryHandler.cs`

```csharp
using FinShark.Application.Dtos;
using FinShark.Application.Mappers;
using FinShark.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinShark.Application.Stocks.Queries.GetDividends;

/// <summary>
/// Handler for retrieving dividends by stock
/// </summary>
public sealed class GetDividendsByStockQueryHandler : IRequestHandler<GetDividendsByStockQuery, IEnumerable<DividendDto>>
{
    private readonly IDividendRepository _dividendRepository;
    private readonly ILogger<GetDividendsByStockQueryHandler> _logger;

    public GetDividendsByStockQueryHandler(
        IDividendRepository dividendRepository,
        ILogger<GetDividendsByStockQueryHandler> logger)
    {
        _dividendRepository = dividendRepository ?? throw new ArgumentNullException(nameof(dividendRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<DividendDto>> Handle(GetDividendsByStockQuery request, CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        _logger.LogInformation("Retrieving dividends for stock: {StockId}", request.StockId);

        try
        {
            var dividends = await _dividendRepository.GetByStockIdAsync(request.StockId);
            var dividendDtos = DividendMapper.ToDtoList(dividends);

            _logger.LogInformation("Retrieved {Count} dividends for stock {StockId}", 
                dividends.Count(), request.StockId);

            return dividendDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dividends for stock: {StockId}", request.StockId);
            throw;
        }
    }
}
```

### Step 8: Implement Repository

**File**: `src/FinShark.Persistence/Repositories/DividendRepository.cs`

```csharp
using FinShark.Domain.Entities;
using FinShark.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinShark.Persistence.Repositories;

/// <summary>
/// Repository implementation for Dividend entity
/// </summary>
public sealed class DividendRepository : IDividendRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<DividendRepository> _logger;

    public DividendRepository(AppDbContext context, ILogger<DividendRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Dividend?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Fetching dividend with ID: {DividendId}", id);
        return await _context.Dividends.FindAsync(id);
    }

    public async Task<IEnumerable<Dividend>> GetByStockIdAsync(int stockId)
    {
        _logger.LogInformation("Fetching dividends for stock: {StockId}", stockId);
        return await _context.Dividends
            .Where(d => d.StockId == stockId)
            .OrderByDescending(d => d.PaymentDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Dividend>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all dividends");
        return await _context.Dividends.ToListAsync();
    }

    public async Task AddAsync(Dividend dividend)
    {
        if (dividend == null) throw new ArgumentNullException(nameof(dividend));
        _logger.LogInformation("Adding dividend for stock: {StockId}", dividend.StockId);
        await _context.Dividends.AddAsync(dividend);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Dividend dividend)
    {
        if (dividend == null) throw new ArgumentNullException(nameof(dividend));
        _logger.LogInformation("Updating dividend: {DividendId}", dividend.Id);
        _context.Dividends.Update(dividend);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Dividend dividend)
    {
        if (dividend == null) throw new ArgumentNullException(nameof(dividend));
        _logger.LogInformation("Deleting dividend: {DividendId}", dividend.Id);
        _context.Dividends.Remove(dividend);
        await _context.SaveChangesAsync();
    }
}
```

### Step 9: Create API Controller

**File**: `src/FinShark.API/Controllers/DividendsController.cs`

```csharp
using FinShark.Application.Dtos;
using FinShark.Application.Stocks.Commands.CreateDividend;
using FinShark.Application.Stocks.Queries.GetDividends;
using FinShark.Application.Stocks.Validators;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FinShark.API.Controllers;

/// <summary>
/// Dividends API Controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class DividendsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<CreateDividendRequestDto> _createDividendValidator;
    private readonly ILogger<DividendsController> _logger;

    public DividendsController(
        IMediator mediator,
        IValidator<CreateDividendRequestDto> createDividendValidator,
        ILogger<DividendsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _createDividendValidator = createDividendValidator ?? throw new ArgumentNullException(nameof(createDividendValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get dividends for a stock
    /// </summary>
    [HttpGet("stock/{stockId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<DividendDto>>>> GetDividendsByStock(
        int stockId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDividendsByStockQuery(stockId), cancellationToken);
        var response = ApiResponse<IEnumerable<DividendDto>>.SuccessResponse(result);
        return Ok(response);
    }

    /// <summary>
    /// Create a dividend
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> CreateDividend(
        [FromBody] CreateDividendRequestDto request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _createDividendValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<int>.FailureResponse(errors));
        }

        var command = new CreateDividendCommand(
            request.StockId,
            request.Amount,
            request.PaymentDate,
            request.RecordDate,
            request.ExDividendDate,
            request.Notes
        );

        var dividendId = await _mediator.Send(command, cancellationToken);
        var response = ApiResponse<int>.SuccessResponse(dividendId, "Dividend created successfully");
        return CreatedAtAction(nameof(GetDividendsByStock), new { stockId = request.StockId }, response);
    }
}
```

### Step 10: Register in DI Container

Update `src/FinShark.Application/ServiceCollectionExtensions.cs`:

```csharp
// FluentValidation - manually register validators
services.AddScoped<IValidator<CreateStockRequestDto>, CreateStockValidator>();
services.AddScoped<IValidator<CreateDividendRequestDto>, CreateDividendValidator>();
```

Update `src/FinShark.Persistence/ServiceCollectionExtensions.cs`:

```csharp
// Register repositories
services.AddScoped<IStockRepository, StockRepository>();
services.AddScoped<IDividendRepository, DividendRepository>();
```

### Step 11: Update Database Context

Update `src/FinShark.Persistence/AppDbContext.cs`:

```csharp
public required DbSet<Dividend> Dividends { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // ... existing Stock configuration ...

    // Configure Dividend entity
    modelBuilder.Entity<Dividend>(entity =>
    {
        entity.HasKey(e => e.Id);

        entity.HasOne(e => e.Stock)
            .WithMany()
            .HasForeignKey(e => e.StockId)
            .IsRequired();

        entity.Property(e => e.Amount)
            .HasPrecision(18, 2);
    });
}
```

### Step 12: Create Database Migration

```bash
cd src/FinShark.API
dotnet ef migrations add AddDividendEntity -p ../FinShark.Persistence
dotnet ef database update
```

---

## Domain Layer Implementation

### Entity Rules

- Always validate in constructors
- Use private setters for invariants
- Add domain methods for complex operations
- Extend `BaseEntity` for audit fields

### Example Domain Logic

```csharp
public class Stock : BaseEntity
{
    public void RecordDividend(decimal amount)
    {
        if (amount <= 0) throw new InvalidOperationException("Dividend must be positive");
        // Perform dividend operations
    }

    public bool IsActivelyTraded()
    {
        return CurrentPrice > 0 && !string.IsNullOrEmpty(Symbol);
    }
}
```

---

## Application Layer (CQRS)

### Command Pattern

```csharp
// 1. Define command (state-changing request)
public sealed record CreateStockCommand(...) : IRequest<int>;

// 2. Implement handler
public sealed class CreateStockCommandHandler : IRequestHandler<CreateStockCommand, int> { }

// 3. Dispatch from controller
var id = await _mediator.Send(new CreateStockCommand(...));
```

### Query Pattern

```csharp
// 1. Define query (read-only request)
public sealed record GetStocksQuery() : IRequest<IEnumerable<StockDto>>;

// 2. Implement handler
public sealed class GetStocksQueryHandler : IRequestHandler<GetStocksQuery, IEnumerable<StockDto>> { }

// 3. Dispatch from controller
var stocks = await _mediator.Send(new GetStocksQuery());
```

---

## Persistence Layer

### Repository Implementation

```csharp
public sealed class StockRepository : IStockRepository
{
    private readonly AppDbContext _context;

    // Always log operations
    // Always handle exceptions
    // Always validate input parameters
    // Always call SaveChangesAsync
}
```

### DbContext Configuration

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Stock>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Symbol).IsRequired().HasMaxLength(10);
        entity.Property(e => e.CurrentPrice).HasPrecision(18, 2);
    });
}
```

---

## API Layer

### Controller Pattern

```csharp
[ApiController]
[Route("api/[controller]")]
public sealed class StocksController : ControllerBase
{
    // Always validate input
    // Always wrap responses
    // Always log requests
    // Always handle exceptions gracefully
    
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<StockDto>>>> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetStocksQuery(), ct);
        return Ok(ApiResponse<IEnumerable<StockDto>>.SuccessResponse(result));
    }
}
```

---

## Testing

### Unit Test Example

```csharp
[TestClass]
public class CreateStockCommandHandlerTests
{
    [TestMethod]
    public async Task Handle_WithValidCommand_ReturnsStockId()
    {
        // Arrange
        var command = new CreateStockCommand("AAPL", "Apple", 150.00m);
        var mockRepository = new Mock<IStockRepository>();
        var handler = new CreateStockCommandHandler(mockRepository.Object, new MockLogger());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result > 0);
        mockRepository.Verify(r => r.AddAsync(It.IsAny<Stock>()), Times.Once);
    }
}
```

---

## Common Patterns

### Exception Handling

```csharp
try
{
    // Operation
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error message with context");
    throw;
}
```

### Async/Await

```csharp
// Always use async/await
public async Task<IEnumerable<Stock>> GetAllAsync()
{
    return await _context.Stocks.ToListAsync();
}
```

### Null Validation

```csharp
// Use inline validation
if (request == null) throw new ArgumentNullException(nameof(request));

// Or constructor validation
_mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
```

### Logging Levels

- **Information**: User actions, feature usage
- **Warning**: Non-critical issues, missing data
- **Error**: Failed operations, exceptions
- **Debug**: Variable values, flow tracing
