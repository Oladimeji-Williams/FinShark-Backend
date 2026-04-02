using FinShark.Application.Dtos;
using MediatorFlow.Core.Contracts;

namespace FinShark.Application.Auth.Commands.Login;

/// <summary>
/// Command to authenticate a user
/// </summary>
public sealed record LoginCommand(
    string Email,
    string Password
) : IRequest<AuthResponseDto>;
