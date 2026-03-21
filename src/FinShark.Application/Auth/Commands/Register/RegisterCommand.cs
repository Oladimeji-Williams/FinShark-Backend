using FinShark.Application.Dtos;
using MediatR;

namespace FinShark.Application.Auth.Commands.Register;

/// <summary>
/// Command to register a new user
/// </summary>
public sealed record RegisterCommand(
    string Email,
    string Password,
    string? UserName = null,
    string? FirstName = null,
    string? LastName = null
) : IRequest<AuthResponseDto>;
