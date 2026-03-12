# FinShark Validation Rules Documentation

Comprehensive validation rules and FluentValidation implementation guide.

## Validation Architecture

```
Request Received
    ↓
Dto Binding (automatic)
    ↓
Validator Executed (MediatR behavior)
    ├─ Property-level rules
    ├─ Cross-property rules
    └─ Custom async rules
    ↓
If Invalid → ValidationException with errors
If Valid → Continue to handler
    ↓
Business Logic
    ├─ Domain invariants (constructor)
    └─ Additional validation
    ↓
Persist to database
```

---

## FluentValidation Setup

### Installation

```bash
dotnet add package FluentValidation
dotnet add package FluentValidation.DependencyInjectionExtensions
```

### Configuration

```csharp
// Program.cs
builder.Services
    .AddValidatorsFromAssembly(typeof(Program).Assembly)
    .AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    });

// Validation Behavior - automatically validates all requests
public sealed class ValidationBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}
```

---

## Stock Validation Rules

### Create Stock Validator

```csharp
// FinShark.Application/Stocks/Validators/CreateStockValidator.cs
using FinShark.Application.Stocks.Commands.CreateStock;
using FluentValidation;

namespace FinShark.Application.Stocks.Validators;

public sealed class CreateStockValidator : AbstractValidator<CreateStockCommand>
{
    private readonly IStockRepository _stockRepository;

    public CreateStockValidator(IStockRepository stockRepository)
    {
        _stockRepository = stockRepository;

        // Symbol validation
        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Symbol is required")
            .Length(1, 10).WithMessage("Symbol must be 1-10 characters")
            .Matches(@"^[A-Z0-9]+$").WithMessage("Symbol must be uppercase letters and numbers only")
            .MustAsync(BeUniqueSymbol).WithMessage("A stock with this symbol already exists");

        // Company name validation
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company Name is required")
            .Length(2, 100).WithMessage("Company Name must be 2-100 characters")
            .Must(NotContainSpecialCharacters).WithMessage("Company Name contains invalid characters");

        // Price validation
        RuleFor(x => x.CurrentPrice)
            .NotEmpty().WithMessage("Current Price is required")
            .GreaterThan(0).WithMessage("Current Price must be greater than 0")
            .LessThan(1000000).WithMessage("Current Price is too high");

        // Industry validation
        RuleFor(x => x.Industry)
            .NotEmpty().WithMessage("Industry is required")
            .IsInEnum().WithMessage("Invalid industry value");

        // Market Cap validation
        RuleFor(x => x.MarketCap)
            .NotEmpty().WithMessage("Market Cap is required")
            .GreaterThan(0).WithMessage("Market Cap must be greater than 0");
    }

    private async Task<bool> BeUniqueSymbol(string symbol, CancellationToken ct)
    {
        // Check if symbol already exists
        var exists = await _stockRepository.SymbolExistsAsync(symbol);
        return !exists;
    }

    private static bool NotContainSpecialCharacters(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true;

        return !Regex.IsMatch(value, @"[^a-zA-Z0-9\s\-',.]");
    }
}
```

### Update Stock Validator

```csharp
// FinShark.Application/Stocks/Validators/UpdateStockValidator.cs
public sealed class UpdateStockValidator : AbstractValidator<UpdateStockCommand>
{
    private readonly IStockRepository _stockRepository;

    public UpdateStockValidator(IStockRepository stockRepository)
    {
        _stockRepository = stockRepository;

        // Same rules as CreateStockValidator
        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Symbol is required")
            .Length(1, 10).WithMessage("Symbol must be 1-10 characters")
            .Matches(@"^[A-Z0-9]+$").WithMessage("Symbol must be uppercase letters and numbers only")
            .MustAsync((symbol, id, ct) => BeUniqueSymbolExcludingCurrent(symbol, id, ct))
            .WithMessage("A stock with this symbol already exists");

        RuleFor(x => x.CurrentPrice)
            .NotEmpty().WithMessage("Current Price is required")
            .GreaterThan(0).WithMessage("Current Price must be greater than 0")
            .LessThan(1000000).WithMessage("Current Price is too high");

        // ... other rules
    }

    private async Task<bool> BeUniqueSymbolExcludingCurrent(
        string symbol,
        int currentId,
        CancellationToken ct)
    {
        // Allow same symbol for current stock, forbid if another stock has it
        var stock = await _stockRepository.GetBySymbolAsync(symbol);
        return stock?.Id == currentId || stock == null;
    }
}
```

---

## Comment Validation Rules

### Create Comment Validator

```csharp
// FinShark.Application/Comments/Validators/CreateCommentValidator.cs
using FinShark.Application.Comments.Commands.CreateComment;
using FinShark.Domain.Repositories;
using FluentValidation;

namespace FinShark.Application.Comments.Validators;

public sealed class CreateCommentValidator : AbstractValidator<CreateCommentCommand>
{
    private readonly IStockRepository _stockRepository;

    public CreateCommentValidator(IStockRepository stockRepository)
    {
        _stockRepository = stockRepository;

        // Stock ID validation
        RuleFor(x => x.StockId)
            .NotEmpty().WithMessage("Stock ID is required")
            .GreaterThan(0).WithMessage("Stock ID must be greater than 0")
            .MustAsync(StockExists).WithMessage("Stock with this ID does not exist");

        // Title validation
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .Length(3, 200).WithMessage("Title must be 3-200 characters long");

        // Content validation
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required")
            .Length(10, 5000).WithMessage("Content must be 10-5000 characters long");

        // Rating validation
        RuleFor(x => x.Rating)
            .NotEmpty().WithMessage("Rating is required")
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");
    }

    private async Task<bool> StockExists(int stockId, CancellationToken ct)
    {
        var stock = await _stockRepository.GetByIdAsync(stockId);
        return stock != null;
    }
}
```

### Update Comment Validator

```csharp
// FinShark.Application/Comments/Validators/UpdateCommentValidator.cs
using FinShark.Application.Comments.Commands.UpdateComment;
using FluentValidation;

namespace FinShark.Application.Comments.Validators;

public sealed class UpdateCommentValidator : AbstractValidator<UpdateCommentCommand>
{
    public UpdateCommentValidator()
    {
        // Comment ID validation
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Comment ID is required")
            .GreaterThan(0).WithMessage("Comment ID must be greater than 0");

        // Title validation
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .Length(3, 200).WithMessage("Title must be 3-200 characters long");

        // Content validation
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required")
            .Length(10, 5000).WithMessage("Content must be 10-5000 characters long");

        // Rating validation
        RuleFor(x => x.Rating)
            .NotEmpty().WithMessage("Rating is required")
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");
    }
}
```

---

## Validation Rule Types

### 1. Required Field Validation

```csharp
RuleFor(x => x.Symbol)
    .NotEmpty().WithMessage("Symbol is required")
    .NotNull().WithMessage("Symbol cannot be null");

RuleFor(x => x.CompanyName)
    .NotEmpty().WithMessage("Company Name is required")
    .NotWhiteSpace().WithMessage("Company Name cannot be whitespace only");
```

### 2. String Length Validation

```csharp
RuleFor(x => x.Symbol)
    .Length(1, 10).WithMessage("Symbol must be 1-10 characters");

RuleFor(x => x.CompanyName)
    .MinimumLength(2).WithMessage("Company Name must be at least 2 characters")
    .MaximumLength(100).WithMessage("Company Name cannot exceed 100 characters");
```

### 3. Pattern Matching

```csharp
RuleFor(x => x.Symbol)
    .Matches(@"^[A-Z0-9]+$").WithMessage("Symbol must be uppercase letters and numbers only");

RuleFor(x => x.Email)
    .EmailAddress().WithMessage("Invalid email address");
```

### 4. Numeric Validation

```csharp
RuleFor(x => x.CurrentPrice)
    .GreaterThan(0).WithMessage("Price must be greater than 0")
    .LessThan(1000000).WithMessage("Price is too high")
    .PrecisionScale(10, 2, ignoreTrailingZeros: true)
    .WithMessage("Price can have at most 2 decimal places");

RuleFor(x => x.Shares)
    .GreaterThanOrEqualTo(1).WithMessage("Shares must be at least 1")
    .LessThanOrEqualTo(long.MaxValue).WithMessage("Shares value too large");
```

### 5. Enum Validation

```csharp
RuleFor(x => x.Industry)
    .IsInEnum().WithMessage("Invalid industry value");

// Valid values: Technology, Healthcare, Finance, Energy, etc.
```

### 6. Date Validation

```csharp
RuleFor(x => x.PaymentDate)
    .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
    .WithMessage("Payment date cannot be in the past");

RuleFor(x => x.ExDividendDate)
    .LessThan(x => x.PaymentDate)
    .WithMessage("Ex-dividend date must be before payment date");
```

### 7. Custom Validation Rules

```csharp
RuleFor(x => x.CompanyName)
    .Custom((value, context) =>
    {
        if (value.StartsWith(" ") || value.EndsWith(" "))
        {
            context.AddFailure("Company Name cannot have leading or trailing spaces");
        }
    });
```

### 8. Conditional Validation

```csharp
RuleFor(x => x.DividendAmount)
    .GreaterThan(0)
    .When(x => x.HasDividends)
    .WithMessage("Dividend amount must be greater than 0 when dividends are declared");
```

### 9. Async Custom Validation

```csharp
RuleFor(x => x.Symbol)
    .MustAsync(async (symbol, ct) =>
    {
        var symbol exists = await _repository.SymbolExistsAsync(symbol, ct);
        return !exists;
    })
    .WithMessage("Symbol already exists");
```

### 10. Cross-Property Validation

```csharp
RuleFor(x => x)
    .Custom((stock, context) =>
    {
        if (stock.CurrentPrice < stock.OpeningPrice)
        {
            context.AddFailure(
                nameof(stock.CurrentPrice),
                "Current price cannot be less than opening price");
        }
    });
```

---

## Validation Severities

### Error (Default)

```csharp
RuleFor(x => x.Symbol)
    .NotEmpty()
    .WithSeverity(Severity.Error); // Request fails
```

### Warning

```csharp
RuleFor(x => x.CompanyName)
    .Matches(@"^[A-Z]") // Must start with uppercase
    .WithSeverity(Severity.Warning); // Warning but continues
```

---

## Custom Validator Classes

Create reusable validators:

```csharp
public static class CustomValidators
{
    public static IRuleBuilderOptions<T, string> BeValidCurrency<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .Matches(@"^[A-Z]{3}$") // USD, EUR, GBP
            .WithMessage("{PropertyName} must be a valid 3-letter currency code");
    }

    public static IRuleBuilderOptions<T, decimal> BeValidPrice<T>(
        this IRuleBuilder<T, decimal> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(0)
            .LessThan(1000000)
            .PrecisionScale(10, 2)
            .WithMessage("{PropertyName} must be a valid price (0-999999.99)");
    }
}

// Usage
RuleFor(x => x.Currency).BeValidCurrency();
RuleFor(x => x.Price).BeValidPrice();
```

---

## Validation Error Response Examples

### Single Field Error

```json
{
  "success": false,
  "data": null,
  "errors": ["Symbol must be 1-10 characters"],
  "message": null
}
```

### Multiple Field Errors

```json
{
  "success": false,
  "data": null,
  "errors": [
    "Symbol is required",
    "Company Name is required",
    "Current Price must be greater than 0",
    "Industry is required"
  ],
  "message": null
}
```

### Grouped by Field

```json
{
  "success": false,
  "data": null,
  "errors": {
    "Symbol": ["Symbol is required", "Symbol must be uppercase"],
    "CurrentPrice": ["Current Price must be greater than 0"]
  },
  "message": null
}
```

---

## Testing Validators

```csharp
[Fact]
public async Task Validate_WithValidData_Passes()
{
    // Arrange
    var command = new CreateStockCommand(
        Symbol: "AAPL",
        CompanyName: "Apple Inc.",
        CurrentPrice: 150.00m,
        Industry: Industry.Technology,
        MarketCap: 2500000000000m);

    var validator = new CreateStockValidator(_mockRepository.Object);

    // Act
    var result = await validator.ValidateAsync(command);

    // Assert
    Assert.True(result.IsValid);
}

[Fact]
public async Task Validate_WithEmptySymbol_Fails()
{
    // Arrange
    var command = new CreateStockCommand(
        Symbol: "",
        CompanyName: "Apple Inc.",
        CurrentPrice: 150.00m,
        Industry: Industry.Technology,
        MarketCap: 2500000000000m);

    var validator = new CreateStockValidator(_mockRepository.Object);

    // Act
    var result = await validator.ValidateAsync(command);

    // Assert
    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Symbol));
}

[Theory]
[InlineData("AAPL123")] // Too long
[InlineData("aapl")] // Lowercase
[InlineData("AA-PL")] // Special chars
public async Task Validate_WithInvalidSymbols_Fails(string symbol)
{
    // Arrange
    var command = new CreateStockCommand(symbol, /* ... */);
    var validator = new CreateStockValidator(_mockRepository.Object);

    // Act
    var result = await validator.ValidateAsync(command);

    // Assert
    Assert.False(result.IsValid);
}
```

---

## Validation Best Practices

✅ **Do This**:
- Validate at API boundary (DTOs)
- Use async rules for database checks
- Create reusable custom validators
- Test all validation rules
- Provide clear error messages
- Validate enums properly

❌ **Never Do This**:
- Skip validation to speed up development
- Validate only in repository layer
- Hardcode validation rules
- Ignore async validation requirements
- Show cryptic error messages
- Validate sensitive data in logs

---

## Common Validation Scenarios

### Email Validation
```csharp
RuleFor(x => x.Email)
    .EmailAddress(mode: EmailValidationMode.AspNetCoreCompatible)
    .WithMessage("Invalid email address");
```

### URL Validation
```csharp
RuleFor(x => x.Website)
    .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
    .WithMessage("Invalid URL");
```

### Unique Constraint
```csharp
RuleFor(x => x.Symbol)
    .MustAsync(BeUniqueSymbol)
    .WithMessage("Symbol already exists");
```

### Future Date
```csharp
RuleFor(x => x.EventDate)
    .GreaterThan(DateTime.UtcNow)
    .WithMessage("Event date must be in the future");
```

### Collection Validation
```csharp
RuleForEach(x => x.Tags)
    .NotEmpty().WithMessage("Tag cannot be empty")
    .MaximumLength(20).WithMessage("Tag cannot exceed 20 characters");
```
