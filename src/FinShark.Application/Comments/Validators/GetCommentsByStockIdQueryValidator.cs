using FinShark.Application.Comments.Queries.GetCommentsByStockId;
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
    }
}
