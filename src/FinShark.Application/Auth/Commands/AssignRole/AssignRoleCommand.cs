using FinShark.Application.Dtos;
using MediatorFlow.Core.Contracts;

namespace FinShark.Application.Auth.Commands.AssignRole;

public sealed record AssignRoleCommand(
    string UserId,
    string Role
) : IRequest<AuthResponseDto>;
