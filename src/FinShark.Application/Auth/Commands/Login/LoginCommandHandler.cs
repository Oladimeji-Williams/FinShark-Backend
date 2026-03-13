using FinShark.Application.Auth.Commands.Login;
using FinShark.Application.Auth.Services;
using FinShark.Application.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinShark.Application.Auth.Commands.Login;

/// <summary>
/// Handler for user login
/// </summary>
public sealed class LoginCommandHandler(
    IAuthService authService,
    ILogger<LoginCommandHandler> logger) : IRequestHandler<LoginCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var loginRequest = new LoginRequestDto(request.Email, request.Password);

            var result = await authService.LoginAsync(loginRequest);

            if (result.Success)
            {
                logger.LogInformation("User logged in successfully: {Email}", request.Email);
            }
            else
            {
                logger.LogWarning("User login failed for {Email}: {Message}", request.Email, result.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during user login for {Email}", request.Email);
            return new AuthResponseDto(false, null, "An error occurred during login");
        }
    }
}