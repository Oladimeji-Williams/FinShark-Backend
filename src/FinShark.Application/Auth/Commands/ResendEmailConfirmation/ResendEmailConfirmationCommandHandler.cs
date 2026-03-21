using FinShark.Application.Auth.Commands.ResendEmailConfirmation;
using FinShark.Application.Auth.Services;
using FinShark.Application.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinShark.Application.Auth.Commands.ResendEmailConfirmation;

/// <summary>
/// Handler for resend email confirmation command
/// </summary>
public sealed class ResendEmailConfirmationCommandHandler(
    IAuthService authService,
    ILogger<ResendEmailConfirmationCommandHandler> logger)
    : IRequestHandler<ResendEmailConfirmationCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(ResendEmailConfirmationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await authService.ResendEmailConfirmationAsync(request.Email);

            if (result.Success)
            {
                logger.LogInformation("Resend email confirmation succeeded for {Email}", request.Email);
            }
            else
            {
                logger.LogWarning("Resend email confirmation failed for {Email}: {Message}", request.Email, result.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while resending email confirmation for {Email}", request.Email);
            return new AuthResponseDto(false, null, "An error occurred while resending confirmation email");
        }
    }
}
