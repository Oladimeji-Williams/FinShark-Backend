# FinShark Backend - Development Guide

Complete guide for code standards, best practices, and development workflows.

## Code Standards

### F# Naming Conventions

| Element | Convention | Example |
|---------|-----------|---------|
| Classes | PascalCase | `CreateStockCommand` |
| Methods | PascalCase | `GetAllAsync` |
| Properties | PascalCase | `CompanyName` |
| Private fields | _camelCase | `_logger` |
| Local variables | camelCase | `result`, `stockId` |
| Constants | UPPER_SNAKE_CASE | `MAX_ATTEMPTS` |
| Namespaces | PascalCase (dots) | `FinShark.Application.Stocks` |

### C# 12+ Features to Use

✅ **Use These**

```csharp
// Required properties
public required string Name { get; init; }

// Sealed classes
public sealed class Stock { }

// Records for immutable types
public sealed record CreateStockCommand(string Symbol) : IRequest<int>;

// Init-only properties
public string Name { get; init; }

// Null-coalescing operator
var name = request?.Name ?? throw new ArgumentNullException(nameof(request));

// Expression-bodied members
public decimal GetTotalValue() => Shares * CurrentPrice;

// Collection expressions
var ids = new[] { 1, 2, 3 };

// Nullable reference types
public string? OptionalField { get; set; }
```

❌ **Avoid These**

```csharp
// Nullable enable on class level
#nullable enable

// Mutable public properties
public string Name { get; set; }

// Non-sealed classes
public class Stock { }

// Direct exception throwing
throw new ArgumentNullException(nameof(name));
```

## Project Organization

### File Structure

```
FinShark.Application/
├── Dtos/
│   ├── ApiResponse.cs
│   ├── CreateStockRequestDto.cs
│   └── StockDto.cs
├── Mappers/
│   ├── DividendMapper.cs
│   └── StockMapper.cs
├── Stocks/
│   ├── Commands/
│   │   ├── CreateStock/
│   │   │   ├── CreateStockCommand.cs
│   │   │   └── CreateStockCommandHandler.cs
│   │   └── UpdateStock/
│   ├── Queries/
│   │   ├── GetStocks/
│   │   │   ├── GetStocksQuery.cs
│   │   │   └── GetStocksQueryHandler.cs
│   │   └── GetStockById/
│   └── Validators/
│       ├── CreateStockValidator.cs
│       └── UpdateStockValidator.cs
└── ServiceCollectionExtensions.cs
```

### Folder Naming Rules

- Use `Commands/` for command definitions and handlers
- Use `Queries/` for query definitions and handlers
- Use `Validators/` for FluentValidation rules
- Use `Dtos/` for data transfer objects
- Use `Mappers/` for manual mappers
- Use `Repositories/` for repository implementations
- Use `Middleware/` for middleware components

## Code Organization Rules

### Class Structure

```csharp
public sealed class StockRepository : IStockRepository
{
    // 1. Private fields (readonly, use _camelCase)
    private readonly AppDbContext _context;
    private readonly ILogger<StockRepository> _logger;

    // 2. Constructor
    public StockRepository(
        AppDbContext context,
        ILogger<StockRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // 3. Public methods
    public async Task<IEnumerable<Stock>> GetAllAsync()
    {
        // Method body
    }

    // 4. Private methods (if needed)
    private void ValidateEntity(Stock entity)
    {
        // Validation logic
    }
}
```

### Property Ordering

```csharp
public class Stock : BaseEntity
{
    // 1. Public properties (getters first)
    public string Symbol { get; private set; }
    public string CompanyName { get; private set; }
    public decimal CurrentPrice { get; private set; }
    
    // 2. Init-only properties
    public string Industry { get; init; }
    public string MarketCap { get; init; }
    
    // 3. Navigation properties
    public ICollection<Dividend> Dividends { get; private set; } = new List<Dividend>();
}
```

## Validation Standards

### Constructor Validation

```csharp
public Stock(string symbol, string companyName, decimal currentPrice)
{
    // Validate all parameters
    if (string.IsNullOrWhiteSpace(symbol))
        throw new ArgumentException("Symbol cannot be empty", nameof(symbol));
    
    if (string.IsNullOrWhiteSpace(companyName))
        throw new ArgumentException("Company name cannot be empty", nameof(companyName));
    
    if (currentPrice <= 0)
        throw new ArgumentException("Price must be positive", nameof(currentPrice));
    
    // Set properties after validation
    Symbol = symbol;
    CompanyName = companyName;
    CurrentPrice = currentPrice;
    CreatedAt = DateTime.UtcNow;
}
```

### Handler Validation

```csharp
public async Task<int> Handle(CreateStockCommand request, CancellationToken ct)
{
    // 1. Validate input
    if (request == null)
        throw new ArgumentNullException(nameof(request));
    
    // 2. Log operation
    _logger.LogInformation("Creating stock: {Symbol}", request.Symbol);
    
    // 3. Try-catch with logging
    try
    {
        // Implementation
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating stock");
        throw;
    }
}
```

### FluentValidation Rules

```csharp
public CreateStockValidator()
{
    // Structure: Property -> Condition -> Message
    
    RuleFor(x => x.Symbol)
        .NotEmpty()
            .WithMessage("Symbol is required.")
        .MaximumLength(10)
            .WithMessage("Symbol cannot exceed 10 characters.")
        .Matches(@"^[A-Z0-9]+$")
            .WithMessage("Symbol must contain only uppercase letters and numbers.");
}
```

## Logging Standards

### Logging Levels

**Information** - User actions and key events
```csharp
_logger.LogInformation("Creating new stock with symbol: {Symbol}", symbol);
_logger.LogInformation("Retrieved {Count} stocks", stocks.Count());
```

**Warning** - Expected issues
```csharp
_logger.LogWarning("Stock with ID {StockId} not found", id);
_logger.LogWarning("Retry attempt {Attempt}/3", retryCount);
```

**Error** - Unexpected errors
```csharp
_logger.LogError(ex, "Error creating stock with symbol: {Symbol}", symbol);
_logger.LogError("Database connection failed");
```

### Structured Logging

Always use named placeholders for context:

✅ **Good**
```csharp
_logger.LogInformation("User {UserId} created stock {StockId}", userId, stockId);
```

❌ **Bad**
```csharp
_logger.LogInformation($"User {userId} created stock {stockId}");
_logger.LogInformation("User " + userId + " created stock " + stockId);
```

## Database Design

### Entity Configuration

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Stock>(entity =>
    {
        // Key
        entity.HasKey(e => e.Id);
        
        // Required properties
        entity.Property(e => e.Symbol)
            .IsRequired()
            .HasMaxLength(10);
        
        // Decimal precision (important!)
        entity.Property(e => e.CurrentPrice)
            .HasPrecision(18, 2);
        
        // Default values
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
        
        // Indexes (for frequently queried columns)
        entity.HasIndex(e => e.Symbol).IsUnique();
        
        // Foreign keys and relationships
        entity.HasMany(e => e.Dividends)
            .WithOne(d => d.Stock)
            .HasForeignKey(d => d.StockId);
    });
}
```

### Migration Guidelines

```bash
# Descriptive migration names
dotnet ef migrations add AddStockSymbolUniqueIndex
dotnet ef migrations add CreateDividendTable
dotnet ef migrations add AddAuditColumnsToStock
```

## Error Handling

### API Error Response Format

```json
{
  "success": false,
  "data": null,
  "message": "Validation failed",
  "errors": [
    "Symbol cannot be empty",
    "Price must be greater than zero"
  ]
}
```

### Exception Handling in Handlers

```csharp
public async Task<int> Handle(CreateStockCommand request, CancellationToken ct)
{
    try
    {
        // Implementation
        return stock.Id;
    }
    catch (DbUpdateException ex)
    {
        _logger.LogError(ex, "Database error");
        throw new InvalidOperationException("Failed to create stock", ex);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error");
        throw;
    }
}
```

## Testing Standards

### Unit Test Structure

```csharp
[TestClass]
public class CreateStockCommandHandlerTests
{
    private Mock<IStockRepository> _mockRepository;
    private Mock<ILogger<CreateStockCommandHandler>> _mockLogger;
    private CreateStockCommandHandler _handler;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IStockRepository>();
        _mockLogger = new Mock<ILogger<CreateStockCommandHandler>>();
        _handler = new CreateStockCommandHandler(_mockRepository.Object, _mockLogger.Object);
    }

    [TestMethod]
    public async Task Handle_WithValidCommand_CreatesStock()
    {
        // Arrange
        var command = new CreateStockCommand("AAPL", "Apple Inc.", 150m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result > 0);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Stock>()), Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task Handle_WithNullCommand_ThrowsException()
    {
        // Act
        await _handler.Handle(null, CancellationToken.None);
    }
}
```

### Test Naming Convention

```
MethodName_StateUnderTest_ExpectedBehavior

Examples:
- Handle_WithValidCommand_ReturnsStockId
- Handle_WithInvalidSymbol_ThrowsException
- GetAllAsync_WithNoStocks_ReturnsEmptyList
```

## Performance Optimization

### Database Queries

✅ **Good - Only load needed data**
```csharp
var stocks = await _context.Stocks
    .Where(s => s.Symbol == symbol)
    .Select(s => new StockDto { /* properties */ })
    .FirstOrDefaultAsync();
```

❌ **Bad - Load entire entity**
```csharp
var stock = await _context.Stocks
    .FirstOrDefaultAsync(s => s.Symbol == symbol);
// Then extract properties
```

### Async/Await

Always use async operations:

✅ **Good**
```csharp
public async Task<IEnumerable<Stock>> GetAllAsync()
{
    return await _context.Stocks.ToListAsync();
}
```

❌ **Bad**
```csharp
public IEnumerable<Stock> GetAll()
{
    return _context.Stocks.ToList();
}
```

### Dependency Injection

Always inject dependencies via constructor:

✅ **Good**
```csharp
public CreateStockCommandHandler(
    IStockRepository repository,
    ILogger<CreateStockCommandHandler> logger)
{
    _repository = repository;
    _logger = logger;
}
```

❌ **Bad**
```csharp
public class CreateStockCommandHandler
{
    private IStockRepository _repository = new StockRepository();
}
```

## Security Guidelines

### Input Validation

Never trust user input:

```csharp
// Always validate in FluentValidator
RuleFor(x => x.Symbol)
    .NotEmpty()
    .MaximumLength(10)
    .Matches(@"^[A-Z0-9]+$");

// Always validate in constructor
if (string.IsNullOrWhiteSpace(symbol))
    throw new ArgumentException(...);
```

### SQL Injection Prevention

Use Entity Framework Core (never raw SQL):

✅ **Good - Parameterized**
```csharp
var stocks = await _context.Stocks
    .Where(s => s.Symbol == symbol)
    .ToListAsync();
```

❌ **Bad - SQL Injection Risk**
```csharp
var stocks = _context.Stocks
    .FromSql($"SELECT * FROM Stocks WHERE Symbol = {symbol}");
```

### Null Reference Protection

Always validate null references:

```csharp
public StockRepository(AppDbContext context, ILogger<StockRepository> logger)
{
    _context = context ?? throw new ArgumentNullException(nameof(context));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
}
```

## Git Commit Standards

### Commit Message Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

- `feat:` New feature
- `fix:` Bug fix
- `refactor:` Code restructuring
- `docs:` Documentation changes
- `test:` Test additions/changes
- `chore:` Dependencies, tooling

### Examples

```
feat(stocks): add dividend management feature
fix(api): handle null stock symbol in validation
refactor(mappers): extract common mapping logic
docs(setup): add database configuration section
test(handlers): add integration tests for stock creation
chore(deps): upgrade MediatR to 14.2.0
```

## Code Review Checklist

- [ ] Follows naming conventions
- [ ] Has appropriate logging
- [ ] Validates all inputs
- [ ] Has XML documentation for public APIs
- [ ] Uses sealed classes where applicable
- [ ] Uses required properties appropriately
- [ ] Has error handling with try-catch
- [ ] Uses async/await for I/O operations
- [ ] No hardcoded values (use configuration)
- [ ] Tests are included for new features
- [ ] No code duplication
- [ ] No unused using statements

## Documentation Standards

### XML Documentation

```csharp
/// <summary>
/// Creates a new stock entity and saves it to the database.
/// </summary>
/// <param name="symbol">The stock ticker symbol (e.g., "AAPL")</param>
/// <param name="companyName">The full company name</param>
/// <param name="currentPrice">The current stock price in USD</param>
/// <returns>The ID of the newly created stock</returns>
/// <exception cref="ArgumentException">Thrown when input validation fails</exception>
/// <exception cref="InvalidOperationException">Thrown when database save fails</exception>
public async Task<int> CreateStockAsync(string symbol, string companyName, decimal currentPrice)
{
    // Implementation
}
```

### Class Documentation

```csharp
/// <summary>
/// Repository implementation for Stock entity.
/// Handles all database operations related to stocks with comprehensive logging.
/// </summary>
public sealed class StockRepository : IStockRepository
{
    // Implementation
}
```

## Useful VS Code Settings

### settings.json

```json
{
    "editor.codeActionsOnSave": {
        "source.fixAll": "explicit"
    },
    "editor.formatOnSave": true,
    "editor.defaultFormatter": "ms-dotnettools.csharp",
    "[csharp]": {
        "editor.defaultFormatter": "ms-dotnettools.csharp",
        "editor.formatOnSave": true,
        "editor.codeActionsOnSave": {
            "source.fixAll": "explicit"
        }
    },
    "omnisharp.enableEditorConfigSupport": true,
    "omnisharp.enableRoslynAnalyzers": true
}
```

---

**Last Updated**: March 2026  
**Version**: 1.0.0
