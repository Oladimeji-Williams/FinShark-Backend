using FinShark.Application.Auth.Commands.UpdateProfile;
using FluentValidation;

namespace FinShark.Application.Auth.Validators;

/// <summary>
/// Validator for UpdateProfileCommand
/// </summary>
public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.UserName) || x.FirstName != null || x.LastName != null)
            .WithMessage("At least one field must be provided to update.");

        RuleFor(x => x.UserName)
            .MinimumLength(3).WithMessage("Username must be at least 3 characters")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters")
            .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Username can only contain letters, numbers, underscores, and hyphens")
            .When(x => !string.IsNullOrWhiteSpace(x.UserName));

        RuleFor(x => x.FirstName)
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters")
            .When(x => x.FirstName != null);

        RuleFor(x => x.LastName)
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters")
            .When(x => x.LastName != null);
    }
}
