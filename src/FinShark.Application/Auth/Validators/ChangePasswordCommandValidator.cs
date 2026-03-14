using FinShark.Application.Auth.Commands.ChangePassword;
using FluentValidation;

namespace FinShark.Application.Auth.Validators;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty().WithMessage("UserId is required.");
        RuleFor(c => c.CurrentPassword).NotEmpty().WithMessage("Current password is required.");
        RuleFor(c => c.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(8).WithMessage("New password must be at least 8 characters.")
            .Matches("[A-Z]").WithMessage("New password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("New password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("New password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("New password must contain at least one symbol.");
    }
}
