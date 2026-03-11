using FinShark.Application.Dtos;
using FluentValidation;

namespace FinShark.Application.Stocks.Validators;

/// <summary>
/// Validator for creating a new Stock using the CreateStockRequestDto
/// Ensures data integrity before entity creation
/// </summary>
public sealed class CreateStockValidator : AbstractValidator<CreateStockRequestDto>
{
    public CreateStockValidator()
    {
        RuleFor(x => x.Symbol)
            .NotEmpty()
                .WithMessage("Stock symbol is required.")
            .MaximumLength(10)
                .WithMessage("Stock symbol cannot exceed 10 characters.")
            .Matches(@"^[A-Z0-9]+$")
                .WithMessage("Stock symbol must contain only uppercase letters and numbers.");

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

        RuleFor(x => x.Industry)
            .MaximumLength(100)
                .WithMessage("Industry cannot exceed 100 characters.");

        RuleFor(x => x.MarketCap)
            .GreaterThanOrEqualTo(0)
                .WithMessage("Market cap must be greater than or equal to 0")
            .PrecisionScale(18, 2, ignoreTrailingZeros: true)
                .WithMessage("Market cap must have at most 2 decimal places");
    }
}