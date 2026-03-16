using FinShark.Application.Dtos;
using MediatR;

namespace FinShark.Application.Auth.Commands.ResendEmailConfirmation;

/// <summary>
/// Command to resend email confirmation link to the user.
/// </summary>
public sealed record ResendEmailConfirmationCommand(string Email) : IRequest<AuthResponseDto>;