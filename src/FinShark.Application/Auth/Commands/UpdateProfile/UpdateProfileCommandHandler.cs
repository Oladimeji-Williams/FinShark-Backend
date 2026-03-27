using FinShark.Application.Auth.Commands.UpdateProfile;
using FinShark.Application.Auth.Services;
using FinShark.Application.Dtos;
using FinShark.Application.Mappers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinShark.Application.Auth.Commands.UpdateProfile;

/// <summary>
/// Handler for updating a user's profile
/// </summary>
public sealed class UpdateProfileCommandHandler(
    IAuthService authService,
    ILogger<UpdateProfileCommandHandler> logger) : IRequestHandler<UpdateProfileCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await authService.UpdateProfileAsync(request.UserId, AuthMapper.ToRequest(request));

            if (result.Success)
            {
                logger.LogInformation("User profile updated successfully: {UserId}", request.UserId);
            }
            else
            {
                logger.LogWarning("User profile update failed for {UserId}: {Message}", request.UserId, result.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user profile for {UserId}", request.UserId);
            return new AuthResponseDto(false, null, "An error occurred while updating profile");
        }
    }
}
