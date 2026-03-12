using FinShark.Application.Comments.Commands.CreateComment;
using FinShark.Domain.ValueObjects;
using FluentValidation;

namespace FinShark.Application.Comments.Validators;

/// <summary>
/// Validator for CreateCommentCommand
/// </summary>
public sealed class CreateCommentValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentValidator()
    {
        // Stock ID validation
        RuleFor(x => x.StockId)
            .GreaterThan(0).WithMessage("Stock ID must be greater than 0");

        // Title validation
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .Length(3, 200).WithMessage("Title must be between 3 and 200 characters");

        // Content validation
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required")
            .Length(10, 5000).WithMessage("Content must be between 10 and 5000 characters");

        // Rating validation
        RuleFor(x => x.Rating)
            .Must(r => r.IsValid)
            .WithMessage("Rating must be between 1 and 5");
    }
}
