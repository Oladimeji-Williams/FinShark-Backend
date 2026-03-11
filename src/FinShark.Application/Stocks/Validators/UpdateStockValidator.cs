using FluentValidation;
using FinShark.Application.Dtos;

namespace FinShark.Application.Stocks.Validators;

/// <summary>
/// Validator for UpdateStockRequestDto
/// Ensures all update data meets business requirements
/// </summary>
public sealed class UpdateStockValidator : AbstractValidator<UpdateStockRequestDto>
{
    public UpdateStockValidator()
    {
        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Symbol is required")
            .MaximumLength(10).WithMessage("Symbol must not exceed 10 characters");

        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company name is required")
            .MaximumLength(255).WithMessage("Company name must not exceed 255 characters");

        RuleFor(x => x.CurrentPrice)
            .GreaterThan(0).WithMessage("Current price must be greater than 0")
            .PrecisionScale(18, 2, ignoreTrailingZeros: true)
            .WithMessage("Current price must have at most 2 decimal places");

        RuleFor(x => x.Industry)
            .NotEmpty().WithMessage("Industry is required")
            .MaximumLength(100).WithMessage("Industry must not exceed 100 characters");

        RuleFor(x => x.MarketCap)
            .GreaterThanOrEqualTo(0).WithMessage("Market cap must be greater than or equal to 0")
            .PrecisionScale(18, 2, ignoreTrailingZeros: true)
            .WithMessage("Market cap must have at most 2 decimal places");
    }
}
