using FinShark.Application.Auth.Commands.ChangePassword;
using FinShark.Application.Auth.Services;
using FinShark.Application.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinShark.Application.Auth.Commands.ChangePassword;

/// <summary>
/// Handler for changing user password.
/// </summary>
public sealed class ChangePasswordCommandHandler(
    IAuthService authService,
    ILogger<ChangePasswordCommandHandler> logger)
    : IRequestHandler<ChangePasswordCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var changePasswordRequest = new ChangePasswordRequestDto(request.CurrentPassword, request.NewPassword);
            var result = await authService.ChangePasswordAsync(request.UserId, changePasswordRequest);

            if (result.Success)
            {
                logger.LogInformation("User password changed successfully: {UserId}", request.UserId);
            }
            else
            {
                logger.LogWarning("User password change failed for {UserId}: {Message}", request.UserId, result.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error changing password for {UserId}", request.UserId);
            return new AuthResponseDto(false, null, "An error occurred while changing password");
        }
    }
}
