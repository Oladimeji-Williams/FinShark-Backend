using FinShark.Application.Auth.Services;
using FinShark.Application.Dtos;
using FinShark.Application.Mappers;
using MediatorFlow.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace FinShark.Application.Auth.Commands.Register;

/// <summary>
/// Handler for user registration
/// </summary>
public sealed class RegisterCommandHandler(
    IAuthService authService,
    ILogger<RegisterCommandHandler> logger) : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await authService.RegisterAsync(AuthMapper.ToRequest(request));

            if (result.Success)
            {
                logger.LogInformation("User registered successfully: {Email}", request.Email);
            }
            else
            {
                logger.LogWarning("User registration failed for {Email}: {Message}", request.Email, result.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during user registration for {Email}", request.Email);
            return new AuthResponseDto(false, null, "An error occurred during registration");
        }
    }
}
