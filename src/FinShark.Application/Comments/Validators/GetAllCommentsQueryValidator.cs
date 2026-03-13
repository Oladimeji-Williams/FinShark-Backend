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
