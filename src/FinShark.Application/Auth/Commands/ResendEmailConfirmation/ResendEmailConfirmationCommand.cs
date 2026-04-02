using FinShark.Application.Dtos;
using MediatorFlow.Core.Contracts;

namespace FinShark.Application.Auth.Commands.ResendEmailConfirmation;

/// <summary>
/// Command to resend email confirmation link to the user.
/// </summary>
public sealed record ResendEmailConfirmationCommand(string Email) : IRequest<AuthResponseDto>;
