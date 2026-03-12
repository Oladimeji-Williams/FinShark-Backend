# FinShark Error Handling Guide

Comprehensive error handling strategy and implementation patterns.

## Error Handling Architecture

```
User Request
    ↓
Controller
    ↓
Validation
    ├─ FluentValidation
    └─ If fails → ValidationException
    ↓
Business Logic
    ├─ Domain Logic
    └─ If fails → DomainException, NotFoundException, etc.
    ↓
Repository/Database
    └─ If fails → DbException (caught and wrapped)
    ↓
Exception Middleware
    ├─ Catches ALL exceptions
    ├─ Logs exception
    ├─ Converts to ApiResponse
    └─ Returns HTTP response
    ↓
Client receives standardized ErrorResponse
```

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
            .Length(1, 10).WithMessage("Symbol must be 1-10 characters");
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
  "errors": ["Symbol is required"],
  "message": null
}
```

### 2. Not Found Exceptions

**When**: Resource doesn't exist

**Status Code**: 404 Not Found

**Example**:
```csharp
public class GetStockByIdQueryHandler : IRequestHandler<GetStockByIdQuery, StockDto>
{
    private readonly IStockRepository _repository;

    public async Task<StockDto> Handle(GetStockByIdQuery request, CancellationToken ct)
    {
        var stock = await _repository.GetByIdAsync(request.Id);
        
        if (stock == null)
            throw new StockNotFoundException($"Stock with ID {request.Id} not found");
        
        return StockMapper.ToDto(stock);
    }
}

// Response:
{
  "success": false,
  "data": null,
  "errors": ["Stock with ID 999 not found"],
  "message": null
}
```

### 3. Conflict Exceptions

**When**: Business rule violation

**Status Code**: 409 Conflict

**Example**:
```csharp
// Duplicate stock symbol
public class CreateStockCommandHandler : IRequestHandler<CreateStockCommand, int>
{
    private readonly IStockRepository _repository;

    public async Task<int> Handle(CreateStockCommand request, CancellationToken ct)
    {
        var existingStock = await _repository.GetBySymbolAsync(request.Symbol);
        
        if (existingStock != null)
            throw new StockAlreadyExistsException(
                $"A stock with symbol '{request.Symbol}' already exists");
        
        // ... create stock
        return stock.Id;
    }
}

// Response:
{
  "success": false,
  "data": null,
  "errors": ["A stock with symbol 'AAPL' already exists"],
  "message": null
}
```

### 4. Invalid Operation Exceptions

**When**: Operation violates business invariants

**Status Code**: 422 Unprocessable Entity

**Example**:
```csharp
// Trying to delete stock with pending transactions
public class DeleteStockCommandHandler : IRequestHandler<DeleteStockCommand>
{
    public async Task Handle(DeleteStockCommand request, CancellationToken ct)
    {
        var stock = await _repository.GetByIdAsync(request.Id);
        
        if (stock.HasPendingTransactions)
            throw new InvalidOperationException(
                "Cannot delete stock with pending transactions");
        }
    
        await _repository.DeleteAsync(stock);
    }
}

// Response:
{
  "success": false,
  "data": null,
  "errors": ["Cannot delete stock with pending transactions"],
  "message": null
}
```

### 5. Server Errors

**When**: Unhandled exceptions

**Status Code**: 500 Internal Server Error

**Example**:
```csharp
// Unexpected error in business logic
public async Task Handle(CreateStockCommand request, CancellationToken ct)
{
    try
    {
        var stock = new Stock(request.Symbol, /* ... */);
        // Unexpected error occurs here
        var result = await _repository.AddAsync(stock);
        return result.Id;
    }
    catch (Exception ex)
    {
        // Exception middleware catches ALL unhandled exceptions
        // and converts to 500 error
        throw;
    }
}

// Response:
{
  "success": false,
  "data": null,
  "errors": ["An unexpected error occurred. Please try again later."],
  "message": null
}
```

---

## Custom Exception Classes

### Base Exception

```csharp
// FinShark.Domain/Exceptions/FinSharkException.cs
namespace FinShark.Domain.Exceptions;

/// <summary>
/// Base exception for all FinShark domain exceptions
/// </summary>
public abstract class FinSharkException : Exception
{
    public virtual int ErrorCode { get; protected set; } = 1000;
    
    protected FinSharkException(string message)
        : base(message) { }
    
    protected FinSharkException(string message, Exception innerException)
        : base(message, innerException) { }
}
```

### Specific Exception Types

```csharp
// Not found
public sealed class StockNotFoundException : FinSharkException
{
    public StockNotFoundException(string message) 
        : base(message) 
    {
        ErrorCode = 1001;
    }
}

// Already exists
public sealed class StockAlreadyExistsException : FinSharkException
{
    public StockAlreadyExistsException(string message) 
        : base(message) 
    {
        ErrorCode = 1002;
    }
}

// Invalid operation
public sealed class InvalidStockOperationException : FinSharkException
{
    public InvalidStockOperationException(string message) 
        : base(message) 
    {
        ErrorCode = 1003;
    }
}
```

---

## Exception Middleware

### Complete Implementation

```csharp
// FinShark.API/Middleware/ExceptionMiddleware.cs
using FinShark.Application.Dtos;
using FinShark.Domain.Exceptions;
using FluentValidation;
using System.Net;

namespace FinShark.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var apiResponse = exception switch
        {
            // Validation errors (400)
            ValidationException validationEx => HandleValidationException(response, validationEx),
            
            // Not found (404)
            StockNotFoundException notFoundEx => HandleNotFoundException(response, notFoundEx),
            
            // Conflict (409)
            StockAlreadyExistsException conflictEx => HandleConflictException(response, conflictEx),
            
            // Invalid operation (422)
            InvalidOperationException invalidOpEx => HandleInvalidOperationException(response, invalidOpEx),
            
            // Server errors (500)
            _ => HandleUnexpectedException(response, exception)
        };

        return response.WriteAsJsonAsync(apiResponse);
    }

    private static ApiResponse<object> HandleValidationException(
        HttpResponse response,
        ValidationException ex)
    {
        response.StatusCode = StatusCodes.Status400BadRequest;
        
        return new ApiResponse<object>
        {
            Success = false,
            Data = null,
            Errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .Select(group => group.First().ErrorMessage)
                .ToArray(),
            Message = null
        };
    }

    private static ApiResponse<object> HandleNotFoundException(
        HttpResponse response,
        Exception ex)
    {
        response.StatusCode = StatusCodes.Status404NotFound;
        
        return new ApiResponse<object>
        {
            Success = false,
            Data = null,
            Errors = new[] { ex.Message },
            Message = null
        };
    }

    private static ApiResponse<object> HandleConflictException(
        HttpResponse response,
        Exception ex)
    {
        response.StatusCode = StatusCodes.Status409Conflict;
        
        return new ApiResponse<object>
        {
            Success = false,
            Data = null,
            Errors = new[] { ex.Message },
            Message = null
        };
    }

    private static ApiResponse<object> HandleInvalidOperationException(
        HttpResponse response,
        Exception ex)
    {
        response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        
        return new ApiResponse<object>
        {
            Success = false,
            Data = null,
            Errors = new[] { ex.Message },
            Message = null
        };
    }

    private static ApiResponse<object> HandleUnexpectedException(
        HttpResponse response,
        Exception ex)
    {
        response.StatusCode = StatusCodes.Status500InternalServerError;
        
        // Don't expose internal error details in production
        var errorMessage = "An unexpected error occurred. Please try again later.";
        
        return new ApiResponse<object>
        {
            Success = false,
            Data = null,
            Errors = new[] { errorMessage },
            Message = null
        };
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

### ✅ Correct Pattern

```csharp
// Catch specific exceptions at appropriate layer
public async Task<Stock> GetStockAsync(int id)
{
    try
    {
        var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.Id == id);
        return stock ?? throw new StockNotFoundException($"Stock with ID {id} not found");
    }
    catch (DbException ex)
    {
        // Unexpected database error - log and wrap
        _logger.LogError(ex, "Database connection error while fetching stock {StockId}", id);
        throw new ApplicationException("Database operation failed", ex);
    }
}
```

### ❌ Incorrect Patterns

```csharp
// Don't catch and swallow exceptions
try
{
    await _repository.AddAsync(stock);
}
catch
{
    // BAD: Exception is lost, caller doesn't know about error
}

// Don't catch too broad
try
{
    await _repository.AddAsync(stock);
}
catch (Exception ex) // BAD: Too broad, catches system exceptions
{
    _logger.LogError(ex, "Error");
}

// Don't throw without context
if (stock == null)
{
    throw new Exception("Not found"); // BAD: Generic message, no context
}

// Don't create new exception and lose original
catch (DbException ex)
{
    throw new Exception("Database error"); // BAD: Loses original exception
}
```

---

## Error Codes

| Code | Error Type | HTTP Status | Description |
|------|-----------|-------------|-------------|
| 1000 | Base Error | 500 | Generic FinShark error |
| 1001 | Not Found | 404 | Stock/resource not found |
| 1002 | Already Exists | 409 | Duplicate resource |
| 1003 | Invalid Operation | 422 | Business rule violation |
| 4000 | Validation Error | 400 | Input validation failed |
| 5000 | Server Error | 500 | Unexpected error |

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
  "message": null,
  "errors": [
    "Symbol is required",
    "Current Price must be greater than 0"
  ]
}
```

### Not Found Response

```json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": ["Stock with ID 999 not found"]
}
```

### Server Error Response

```json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": ["An unexpected error occurred. Please try again later."]
}
```

---

## Error Handling Checklist

- [ ] All exceptions caught at appropriate layer
- [ ] Exceptions logged with context
- [ ] Custom exception classes created
- [ ] Exception middleware implemented
- [ ] Error responses standardized
- [ ] HTTP status codes correct
- [ ] No sensitive data in error messages
- [ ] Production errors don't expose internals
- [ ] All code paths tested for errors
- [ ] Error messages are user-friendly

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
    var handler = new CreateStockCommandHandler(_repository);

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
    var handler = new GetStockByIdQueryHandler(_repository);

    // Act & Assert
    var ex = await Assert.ThrowsAsync<StockNotFoundException>(
        () => handler.Handle(query, CancellationToken.None));
    
    Assert.Contains("999", ex.Message);
}
```
