# FinShark Error Handling Guide

Comprehensive error handling strategy and implementation patterns.

## Error Handling Architecture

Request -> Controller -> Validation (FluentValidation via MediatR) -> Business Logic -> Repository/Database -> Exception Middleware -> Client

---

## Exception Types

### 1. Validation Exceptions

**When**: Input fails validation

**Status Code**: 400 Bad Request

**Example**:
```csharp
// FluentValidation automatically throws on failure
public class CreateStockValidator : AbstractValidator<CreateStockCommand>
{
    public CreateStockValidator()
    {
        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Symbol is required")
            .MaximumLength(10).WithMessage("Symbol cannot exceed 10 characters");
    }
}

// When invalid data supplied:
var command = new CreateStockCommand(Symbol: "");
var result = await mediator.Send(command);
// Throws: ValidationException with error messages

// Response:
{
  "success": false,
  "data": null,
  "message": "Validation failed",
  "errors": ["Symbol is required"]
}
```

### 2. Not Found Exceptions

**When**: Resource does not exist

**Status Code**: 404 Not Found

**Example**:
```csharp
public class GetStockByIdQueryHandler : IRequestHandler<GetStockByIdQuery, StockDto>
{
    private readonly IStockRepository _repository;

    public async Task<StockDto> Handle(GetStockByIdQuery request, CancellationToken ct)
    {
        var stock = await _repository.GetByIdAsync(request.Id, ct);

        if (stock == null)
            throw new StockNotFoundException($"Stock with ID {request.Id} not found");

        return StockMapper.ToDto(stock);
    }
}

// Response:
{
  "success": false,
  "data": null,
  "message": "Stock with ID 999 not found",
  "errors": null
}
```

### 3. Conflict Exceptions

**When**: Business rule violation (duplicate resource)

**Status Code**: 409 Conflict

**Example**:
```csharp
public class CreateStockCommandHandler : IRequestHandler<CreateStockCommand, CreateStockResponseDto>
{
    private readonly IStockRepository _repository;

    public async Task<CreateStockResponseDto> Handle(CreateStockCommand request, CancellationToken ct)
    {
        var existingStock = await _repository.GetBySymbolAsync(request.Symbol, ct);

        if (existingStock != null)
            throw new StockAlreadyExistsException(
                $"A stock with symbol '{request.Symbol}' already exists");

        // ... create stock
        return new CreateStockResponseDto { Id = stock.Id };
    }
}

// Response:
{
  "success": false,
  "data": null,
  "message": "A stock with symbol 'AAPL' already exists",
  "errors": null
}
```

### 4. Domain Exceptions (Generic)

**When**: A domain exception is thrown without a more specific HTTP mapping

**Status Code**: 400 Bad Request

**Example**:
```csharp
throw new FinSharkException("Business rule violation");
```

### 5. Server Errors

**When**: Unhandled exceptions

**Status Code**: 500 Internal Server Error

**Response**:
```json
{
  "success": false,
  "data": null,
  "message": "An unexpected error occurred. Please contact support.",
  "errors": null
}
```

---

## Custom Exception Classes

```csharp
// FinShark.Domain/Exceptions/FinSharkException.cs
namespace FinShark.Domain.Exceptions;

public abstract class FinSharkException : Exception
{
    public virtual int ErrorCode { get; protected set; } = 1000;

    protected FinSharkException(string message)
        : base(message) { }

    protected FinSharkException(string message, Exception innerException)
        : base(message, innerException) { }
}

public sealed class StockNotFoundException : FinSharkException
{
    public StockNotFoundException(string message)
        : base(message)
    {
        ErrorCode = 1001;
    }
}

public sealed class CommentNotFoundException : FinSharkException
{
    public CommentNotFoundException(string message)
        : base(message)
    {
        ErrorCode = 1001;
    }
}

public sealed class StockAlreadyExistsException : FinSharkException
{
    public StockAlreadyExistsException(string message)
        : base(message)
    {
        ErrorCode = 1002;
    }
}
```

---

## Exception Middleware

```csharp
// FinShark.API/Middleware/ExceptionMiddleware.cs
public async Task InvokeAsync(HttpContext context)
{
    try
    {
        await _next(context);
    }
    catch (ValidationException vex)
    {
        await HandleValidationExceptionAsync(context, vex);
    }
    catch (StockAlreadyExistsException saex)
    {
        await HandleConflictExceptionAsync(context, saex);
    }
    catch (StockNotFoundException snfex)
    {
        await HandleNotFoundExceptionAsync(context, snfex);
    }
    catch (CommentNotFoundException cnfex)
    {
        await HandleNotFoundExceptionAsync(context, cnfex);
    }
    catch (FinSharkException fex)
    {
        await HandleDomainExceptionAsync(context, fex);
    }
    catch (KeyNotFoundException knfex)
    {
        await HandleNotFoundExceptionAsync(context, knfex);
    }
    catch (Exception)
    {
        await HandleExceptionAsync(context);
    }
}
```

### Register Middleware

```csharp
// Program.cs
app.UseMiddleware<ExceptionMiddleware>();
```

---

## Try-Catch Best Practices

### Correct Pattern

```csharp
public async Task<Stock> GetStockAsync(int id)
{
    try
    {
        var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.Id == id);
        return stock ?? throw new StockNotFoundException($"Stock with ID {id} not found");
    }
    catch (DbException ex)
    {
        _logger.LogError(ex, "Database connection error while fetching stock {StockId}", id);
        throw new ApplicationException("Database operation failed", ex);
    }
}
```

### Incorrect Patterns

```csharp
// Don't catch and swallow exceptions
try
{
    await _repository.AddAsync(stock);
}
catch
{
    // BAD: Exception is lost
}

// Don't catch too broad
try
{
    await _repository.AddAsync(stock);
}
catch (Exception ex) // BAD
{
    _logger.LogError(ex, "Error");
}

// Don't throw without context
if (stock == null)
{
    throw new Exception("Not found"); // BAD
}

// Don't create new exception and lose original
catch (DbException ex)
{
    throw new Exception("Database error"); // BAD
}
```

---

## Error Codes

| Code | Error Type | HTTP Status | Description |
|------|-----------|-------------|-------------|
| 1000 | Base Error | 500 | Generic FinShark error |
| 1001 | Not Found | 404 | Stock/comment not found |
| 1002 | Already Exists | 409 | Duplicate resource |

---

## Response Formats

### Success Response

```json
{
  "success": true,
  "data": {
    "id": 1,
    "symbol": "AAPL"
  },
  "message": "Stock created successfully",
  "errors": null
}
```

### Validation Error Response

```json
{
  "success": false,
  "data": null,
  "message": "Validation failed",
  "errors": [
    "Symbol is required",
    "Current price must be greater than 0"
  ]
}
```

### Not Found Response

```json
{
  "success": false,
  "data": null,
  "message": "Stock with ID 999 not found",
  "errors": null
}
```

### Conflict Response

```json
{
  "success": false,
  "data": null,
  "message": "A stock with symbol 'AAPL' already exists",
  "errors": null
}
```

### Server Error Response

```json
{
  "success": false,
  "data": null,
  "message": "An unexpected error occurred. Please contact support.",
  "errors": null
}
```

---

## Testing Error Scenarios

```csharp
[Fact]
public async Task CreateStock_WithDuplicateSymbol_ThrowsException()
{
    // Arrange
    var existingStock = new Stock("AAPL", /* ... */);
    await _repository.AddAsync(existingStock);

    var command = new CreateStockCommand("AAPL", /* ... */);
    var handler = new CreateStockCommandHandler(_repository, _logger);

    // Act & Assert
    var ex = await Assert.ThrowsAsync<StockAlreadyExistsException>(
        () => handler.Handle(command, CancellationToken.None));

    Assert.Contains("already exists", ex.Message);
}

[Fact]
public async Task GetStock_WithInvalidId_ReturnsNotFound()
{
    // Arrange
    var query = new GetStockByIdQuery(999);
    var handler = new GetStockByIdQueryHandler(_repository, _logger);

    // Act & Assert
    var ex = await Assert.ThrowsAsync<StockNotFoundException>(
        () => handler.Handle(query, CancellationToken.None));

    Assert.Contains("999", ex.Message);
}
```