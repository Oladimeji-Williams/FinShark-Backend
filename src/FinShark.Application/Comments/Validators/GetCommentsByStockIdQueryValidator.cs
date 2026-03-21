using FinShark.Application.Comments.Queries.GetCommentsByStockId;
using FinShark.Domain.Queries;
using FluentValidation;

namespace FinShark.Application.Comments.Validators;

/// <summary>
/// Validator for GetCommentsByStockIdQuery
/// Ensures stock ID and pagination inputs are valid
/// </summary>
public sealed class GetCommentsByStockIdQueryValidator : AbstractValidator<GetCommentsByStockIdQuery>
{
    private const int MaxPageSize = 100;

    public GetCommentsByStockIdQueryValidator()
    {
        RuleFor(x => x.StockId)
            .GreaterThan(0)
            .WithMessage("Stock ID must be greater than 0.");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("PageNumber must be greater than 0.")
            .When(x => x.PageNumber.HasValue);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, MaxPageSize)
            .WithMessage($"PageSize must be between 1 and {MaxPageSize}.")
            .When(x => x.PageSize.HasValue);

        RuleFor(x => x.SortBy)
            .IsInEnum()
            .WithMessage("SortBy is invalid.");

        RuleFor(x => x.SortDirection)
            .IsInEnum()
            .WithMessage("SortDirection is invalid.");

        RuleFor(x => x.MinRating)
            .InclusiveBetween(1, 5)
            .WithMessage("MinRating must be between 1 and 5.")
            .When(x => x.MinRating.HasValue);

        RuleFor(x => x.MaxRating)
            .InclusiveBetween(1, 5)
            .WithMessage("MaxRating must be between 1 and 5.")
            .When(x => x.MaxRating.HasValue);

        RuleFor(x => x)
            .Must(x => !x.MinRating.HasValue || !x.MaxRating.HasValue || x.MinRating <= x.MaxRating)
            .WithMessage("MinRating cannot be greater than MaxRating.");
    }
}
