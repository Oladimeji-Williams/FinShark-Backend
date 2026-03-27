using FinShark.Application.Comments.Queries.GetAllComments;
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
        RuleFor(x => x.QueryParameters)
            .NotNull()
            .WithMessage("Query parameters are required.");

        RuleFor(x => x.QueryParameters.PageNumber)
            .GreaterThan(0)
            .WithMessage("PageNumber must be greater than 0.")
            .When(x => x.QueryParameters.PageNumber.HasValue);

        RuleFor(x => x.QueryParameters.PageSize)
            .InclusiveBetween(1, MaxPageSize)
            .WithMessage($"PageSize must be between 1 and {MaxPageSize}.")
            .When(x => x.QueryParameters.PageSize.HasValue);

        RuleFor(x => x.QueryParameters.SortBy)
            .IsInEnum()
            .WithMessage("SortBy is invalid.");

        RuleFor(x => x.QueryParameters.SortDirection)
            .IsInEnum()
            .WithMessage("SortDirection is invalid.");

        RuleFor(x => x.QueryParameters.MinRating)
            .InclusiveBetween(1, 5)
            .WithMessage("MinRating must be between 1 and 5.")
            .When(x => x.QueryParameters.MinRating.HasValue);

        RuleFor(x => x.QueryParameters.MaxRating)
            .InclusiveBetween(1, 5)
            .WithMessage("MaxRating must be between 1 and 5.")
            .When(x => x.QueryParameters.MaxRating.HasValue);

        RuleFor(x => x)
            .Must(x => !x.QueryParameters.MinRating.HasValue || !x.QueryParameters.MaxRating.HasValue || x.QueryParameters.MinRating <= x.QueryParameters.MaxRating)
            .WithMessage("MinRating cannot be greater than MaxRating.");
    }
}
