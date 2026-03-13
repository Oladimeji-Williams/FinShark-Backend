# FinShark Validation Rules Documentation

Comprehensive validation rules and FluentValidation implementation guide.

## Validation Architecture

Request -> DTO Binding -> MediatR ValidationBehavior -> Validator Rules -> Handler -> Domain Invariants

---

## FluentValidation Setup

Validation is registered in the Application layer and applied through a MediatR pipeline behavior.

```csharp
// FinShark.Application/ServiceCollectionExtensions.cs
public static IServiceCollection AddApplicationServices(this IServiceCollection services)
{
    services.AddMediatR(config =>
    {
        config.RegisterServicesFromAssemblyContaining<CreateStockValidator>();
    });

    // FluentValidation - manually register command validators
    services.AddScoped<IValidator<CreateStockCommand>, CreateStockValidator>();
    services.AddScoped<IValidator<UpdateStockCommand>, UpdateStockValidator>();
    services.AddScoped<IValidator<CreateCommentCommand>, CreateCommentValidator>();
    services.AddScoped<IValidator<UpdateCommentCommand>, UpdateCommentValidator>();

    // MediatR pipeline behaviors
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

    return services;
}
```

---

## Stock Validation Rules

### Create Stock Validator

```csharp
// FinShark.Application/Stocks/Validators/CreateStockValidator.cs
public sealed class CreateStockValidator : AbstractValidator<CreateStockCommand>
{
    public CreateStockValidator()
    {
        RuleFor(x => x.Symbol)
            .NotEmpty()
                .WithMessage("Stock symbol is required.")
            .MaximumLength(10)
                .WithMessage("Stock symbol cannot exceed 10 characters.")
            .Matches(@"^[A-Z0-9.]+$")
                .WithMessage("Stock symbol must contain only uppercase letters, numbers, and dots (e.g., AB.C).");

        RuleFor(x => x.CompanyName)
            .NotEmpty()
                .WithMessage("Company name is required.")
            .MaximumLength(255)
                .WithMessage("Company name cannot exceed 255 characters.");

        RuleFor(x => x.CurrentPrice)
            .GreaterThan(0)
                .WithMessage("Current price must be greater than zero.")
            .LessThanOrEqualTo(decimal.MaxValue)
                .WithMessage("Current price is too large.");

        RuleFor(x => x.MarketCap)
            .GreaterThanOrEqualTo(0)
                .WithMessage("Market cap must be greater than or equal to 0")
            .PrecisionScale(18, 2, ignoreTrailingZeros: true)
                .WithMessage("Market cap must have at most 2 decimal places");
    }
}
```

### Update Stock Validator

```csharp
// FinShark.Application/Stocks/Validators/UpdateStockValidator.cs
public sealed class UpdateStockValidator : AbstractValidator<UpdateStockCommand>
{
    public UpdateStockValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Stock ID must be greater than 0");

        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Symbol is required")
            .MaximumLength(10).WithMessage("Symbol must not exceed 10 characters")
            .Matches(@"^[A-Z0-9.]+$").WithMessage("Symbol must contain only uppercase letters, numbers, and dots (e.g., AB.C)")
            .When(x => x.Symbol != null);

        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company name is required")
            .MaximumLength(255).WithMessage("Company name must not exceed 255 characters")
            .When(x => x.CompanyName != null);

        RuleFor(x => x.CurrentPrice)
            .GreaterThan(0).WithMessage("Current price must be greater than 0")
            .PrecisionScale(18, 2, ignoreTrailingZeros: true)
            .WithMessage("Current price must have at most 2 decimal places")
            .When(x => x.CurrentPrice.HasValue);

        RuleFor(x => x.MarketCap)
            .GreaterThanOrEqualTo(0).WithMessage("Market cap must be greater than or equal to 0")
            .PrecisionScale(18, 2, ignoreTrailingZeros: true)
            .WithMessage("Market cap must have at most 2 decimal places")
            .When(x => x.MarketCap.HasValue);
    }
}
```

### Get Stocks Query Validator

```csharp
// FinShark.Application/Stocks/Validators/GetStocksQueryValidator.cs
public sealed class GetStocksQueryValidator : AbstractValidator<GetStocksQuery>
{
    private const int MaxPageSize = 100;

    public GetStocksQueryValidator()
    {
        RuleFor(x => x.QueryParameters)
            .NotNull()
            .WithMessage("Query parameters are required.");

        When(x => x.QueryParameters != null, () =>
        {
            RuleFor(x => x.QueryParameters.PageNumber)
                .GreaterThan(0)
                .WithMessage("PageNumber must be greater than 0.")
                .When(x => x.QueryParameters.PageNumber.HasValue);

            RuleFor(x => x.QueryParameters.PageSize)
                .InclusiveBetween(1, MaxPageSize)
                .WithMessage($"PageSize must be between 1 and {MaxPageSize}.")
                .When(x => x.QueryParameters.PageSize.HasValue);

            RuleFor(x => x.QueryParameters.Symbol)
                .MaximumLength(10)
                .WithMessage("Symbol cannot exceed 10 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.QueryParameters.Symbol));

            RuleFor(x => x.QueryParameters.CompanyName)
                .MaximumLength(255)
                .WithMessage("Company name cannot exceed 255 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.QueryParameters.CompanyName));

            RuleFor(x => x.QueryParameters.SortBy)
                .IsInEnum()
                .WithMessage("SortBy is invalid.");

            RuleFor(x => x.QueryParameters.SortDirection)
                .IsInEnum()
                .WithMessage("SortDirection is invalid.");

            RuleFor(x => x.QueryParameters.MinPrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage("MinPrice must be greater than or equal to 0.")
                .When(x => x.QueryParameters.MinPrice.HasValue);

            RuleFor(x => x.QueryParameters.MaxPrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage("MaxPrice must be greater than or equal to 0.")
                .When(x => x.QueryParameters.MaxPrice.HasValue);

            RuleFor(x => x.QueryParameters.MinMarketCap)
                .GreaterThanOrEqualTo(0)
                .WithMessage("MinMarketCap must be greater than or equal to 0.")
                .When(x => x.QueryParameters.MinMarketCap.HasValue);

            RuleFor(x => x.QueryParameters.MaxMarketCap)
                .GreaterThanOrEqualTo(0)
                .WithMessage("MaxMarketCap must be greater than or equal to 0.")
                .When(x => x.QueryParameters.MaxMarketCap.HasValue);

            RuleFor(x => x.QueryParameters)
                .Must(p => !p.MinPrice.HasValue || !p.MaxPrice.HasValue || p.MinPrice <= p.MaxPrice)
                .WithMessage("MinPrice must be less than or equal to MaxPrice.");

            RuleFor(x => x.QueryParameters)
                .Must(p => !p.MinMarketCap.HasValue || !p.MaxMarketCap.HasValue || p.MinMarketCap <= p.MaxMarketCap)
                .WithMessage("MinMarketCap must be less than or equal to MaxMarketCap.");
        });
    }
}
```
---

## Comment Validation Rules

### Create Comment Validator

```csharp
// FinShark.Application/Comments/Validators/CreateCommentValidator.cs
public sealed class CreateCommentValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentValidator()
    {
        RuleFor(x => x.StockId)
            .GreaterThan(0).WithMessage("Stock ID must be greater than 0");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .Length(3, 200).WithMessage("Title must be between 3 and 200 characters");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required")
            .Length(10, 5000).WithMessage("Content must be between 10 and 5000 characters");

        RuleFor(x => x.Rating)
            .Must(r => r.IsValid)
            .WithMessage("Rating must be between 1 and 5");
    }
}
```

### Update Comment Validator

```csharp
// FinShark.Application/Comments/Validators/UpdateCommentValidator.cs
public sealed class UpdateCommentValidator : AbstractValidator<UpdateCommentCommand>
{
    public UpdateCommentValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Comment ID must be greater than 0");

        RuleFor(x => x.Title)
            .Length(3, 200).WithMessage("Title must be between 3 and 200 characters")
            .When(x => x.Title != null);

        RuleFor(x => x.Content)
            .Length(10, 5000).WithMessage("Content must be between 10 and 5000 characters")
            .When(x => x.Content != null);

        RuleFor(x => x.Rating)
            .Must(r => r == null || r.Value.IsValid)
            .WithMessage("Rating must be between 1 and 5");
    }
}
```

### Get All Comments Query Validator

```csharp
// FinShark.Application/Comments/Validators/GetAllCommentsQueryValidator.cs
public sealed class GetAllCommentsQueryValidator : AbstractValidator<GetAllCommentsQuery>
{
    public GetAllCommentsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .When(x => x.PageNumber.HasValue);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .When(x => x.PageSize.HasValue);
    }
}
```

### Get Comments By Stock Id Query Validator

```csharp
// FinShark.Application/Comments/Validators/GetCommentsByStockIdQueryValidator.cs
public sealed class GetCommentsByStockIdQueryValidator : AbstractValidator<GetCommentsByStockIdQuery>
{
    public GetCommentsByStockIdQueryValidator()
    {
        RuleFor(x => x.StockId)
            .GreaterThan(0);

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .When(x => x.PageNumber.HasValue);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .When(x => x.PageSize.HasValue);
    }
}
```

---

## Notes

1. Existence checks and uniqueness rules that require database access (for example, duplicate stock symbols) are enforced in command handlers, not validators.
2. Domain invariants remain in the domain layer (entity constructors and value objects).
