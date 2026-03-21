using FinShark.Application.Comments.Queries.GetAllComments;
using FinShark.Domain.Queries;
using FluentValidation;

namespace FinShark.Application.Comments.Validators;

/// <summary>
/// Validator for GetAllCommentsQuery
/// Ensures pagination inputs are valid
/// </summary>
public sealed class GetAllCommentsQueryValidator : AbstractValidator<GetAllCommentsQuery>
{
    private const int MaxPageSize = 100;

    public GetAllCommentsQueryValidator()
    {
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
