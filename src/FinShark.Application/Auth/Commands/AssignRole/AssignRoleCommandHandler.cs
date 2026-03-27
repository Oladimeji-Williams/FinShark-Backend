using FinShark.Application.Auth.Services;
using FinShark.Application.Dtos;
using FinShark.Application.Mappers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinShark.Application.Auth.Commands.AssignRole;

public sealed class AssignRoleCommandHandler(
    IAuthService authService,
    ILogger<AssignRoleCommandHandler> logger) : IRequestHandler<AssignRoleCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await authService.AssignRoleAsync(AuthMapper.ToRequest(request));

            if (result.Success)
            {
                logger.LogInformation("Assigned role {Role} to user {UserId}", request.Role, request.UserId);
            }
            else
            {
                logger.LogWarning("Failed to assign role {Role} to user {UserId}: {Message}", request.Role, request.UserId, result.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error assigning role {Role} to user {UserId}", request.Role, request.UserId);
            return new AuthResponseDto(false, null, "An error occurred while assigning role");
        }
    }
}
