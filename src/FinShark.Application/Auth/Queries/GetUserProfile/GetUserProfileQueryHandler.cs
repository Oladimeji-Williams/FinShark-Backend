using FinShark.Application.Auth.Services;
using FinShark.Application.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinShark.Application.Auth.Queries.GetUserProfile;

public sealed class GetUserProfileQueryHandler(
    IAuthService authService,
    ILogger<GetUserProfileQueryHandler> logger) : IRequestHandler<GetUserProfileQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving user profile for {UserId}", request.UserId);
        return await authService.GetUserAsync(request.UserId);
    }
}
