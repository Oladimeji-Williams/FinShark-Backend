using FinShark.Application.Auth.Commands.UpdateProfile;
using FinShark.Application.Auth.Services;
using FinShark.Application.Dtos;
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
            var updateRequest = new UpdateProfileRequestDto(
                request.UserName,
                request.FirstName,
                request.LastName
            );

            var result = await authService.UpdateProfileAsync(request.UserId, updateRequest);

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
