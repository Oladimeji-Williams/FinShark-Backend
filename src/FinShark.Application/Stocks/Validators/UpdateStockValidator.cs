using FinShark.Application.Stocks.Commands.UpdateStock;
using FluentValidation;

namespace FinShark.Application.Stocks.Validators;

/// <summary>
/// Validator for UpdateStockCommand
/// Ensures all update data meets business requirements
/// </summary>
public sealed class UpdateStockValidator : AbstractValidator<UpdateStockCommand>
{
    public UpdateStockValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Stock ID must be greater than 0");

        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Symbol is required")
            .MaximumLength(20).WithMessage("Symbol must not exceed 20 characters")
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
