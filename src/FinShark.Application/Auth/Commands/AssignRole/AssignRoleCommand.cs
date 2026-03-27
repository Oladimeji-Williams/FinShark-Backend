using FinShark.Application.Dtos;
using MediatR;

namespace FinShark.Application.Auth.Commands.AssignRole;

public sealed record AssignRoleCommand(
    string UserId,
    string Role
) : IRequest<AuthResponseDto>;
