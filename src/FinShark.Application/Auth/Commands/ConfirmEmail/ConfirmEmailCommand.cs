using FinShark.Application.Dtos;
using MediatR;

namespace FinShark.Application.Auth.Commands.ConfirmEmail;

/// <summary>
/// Command to confirm user email with token.
/// </summary>
public sealed record ConfirmEmailCommand(string UserId, string Token) : IRequest<AuthResponseDto>;
