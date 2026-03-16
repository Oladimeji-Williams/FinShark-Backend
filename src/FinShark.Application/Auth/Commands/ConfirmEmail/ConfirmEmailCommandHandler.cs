using FinShark.Application.Auth.Commands.ConfirmEmail;
using FinShark.Application.Auth.Services;
using FinShark.Application.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinShark.Application.Auth.Commands.ConfirmEmail;

/// <summary>
/// Handler for confirm email command.
/// </summary>
public sealed class ConfirmEmailCommandHandler(
    IAuthService authService,
    ILogger<ConfirmEmailCommandHandler> logger)
    : IRequestHandler<ConfirmEmailCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await authService.ConfirmEmailAsync(request.UserId, request.Token);

            if (result.Success)
            {
                logger.LogInformation("Email confirmed successfully for user {UserId}", request.UserId);
            }
            else
            {
                logger.LogWarning("Email confirmation failed for user {UserId}: {Message}", request.UserId, result.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while confirming email for user {UserId}", request.UserId);
            return new AuthResponseDto(false, null, "An error occurred while confirming email");
        }
    }
}