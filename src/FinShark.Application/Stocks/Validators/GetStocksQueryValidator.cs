using FinShark.Application.Stocks.Queries.GetStocks;
using FluentValidation;

namespace FinShark.Application.Stocks.Validators;

/// <summary>
/// Validator for GetStocksQuery
/// Ensures filtering, sorting, and pagination inputs are valid
/// </summary>
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
