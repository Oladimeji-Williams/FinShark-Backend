using FinShark.Application.Comments.Commands.UpdateComment;
using FluentValidation;

namespace FinShark.Application.Comments.Validators;

/// <summary>
/// Validator for UpdateCommentCommand
/// </summary>
public sealed class UpdateCommentValidator : AbstractValidator<UpdateCommentCommand>
{
    public UpdateCommentValidator()
    {
        // ID validation
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Comment ID must be greater than 0");

        // Title validation - OPTIONAL for partial updates
        RuleFor(x => x.Title)
            .Length(3, 200).WithMessage("Title must be between 3 and 200 characters")
            .When(x => x.Title != null);

        // Content validation - OPTIONAL for partial updates
        RuleFor(x => x.Content)
            .Length(10, 5000).WithMessage("Content must be between 10 and 5000 characters")
            .When(x => x.Content != null);

        // Rating validation - OPTIONAL for partial updates
        RuleFor(x => x.Rating)
            .Must(r => r == null || r.Value.IsValid)
            .WithMessage("Rating must be between 1 and 5");
    }
}
