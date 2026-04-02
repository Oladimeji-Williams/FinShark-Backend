using FinShark.Application.Dtos;
using MediatorFlow.Core.Contracts;

namespace FinShark.Application.Auth.Commands.ChangePassword;

/// <summary>
/// Command to change the authenticated user's password.
/// </summary>
public sealed record ChangePasswordCommand(
    string UserId,
    string CurrentPassword,
    string NewPassword
) : IRequest<AuthResponseDto>;
