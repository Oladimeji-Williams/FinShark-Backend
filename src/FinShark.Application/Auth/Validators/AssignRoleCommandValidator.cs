using FinShark.Application.Auth.Commands.AssignRole;
using FluentValidation;

namespace FinShark.Application.Auth.Validators;

public sealed class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
{
    public AssignRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(x => x.Role)
            .NotEmpty()
            .WithMessage("Role is required.")
            .MaximumLength(100)
            .WithMessage("Role must not exceed 100 characters.");
    }
}
