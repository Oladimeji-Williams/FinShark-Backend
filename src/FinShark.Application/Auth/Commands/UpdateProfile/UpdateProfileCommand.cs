using FinShark.Application.Dtos;
using MediatR;

namespace FinShark.Application.Auth.Commands.UpdateProfile;

/// <summary>
/// Command to update the authenticated user's profile.
/// </summary>
public sealed record UpdateProfileCommand(
    string UserId,
    string? UserName = null,
    string? FirstName = null,
    string? LastName = null
) : IRequest<AuthResponseDto>;
