using FinShark.Application.Auth.Services;
using FinShark.Application.Dtos;
using FinShark.Application.Mappers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinShark.Application.Auth.Queries.GetCurrentUserIdentity;

public sealed class GetCurrentUserIdentityQueryHandler(
    IAuthService authService,
    ILogger<GetCurrentUserIdentityQueryHandler> logger) : IRequestHandler<GetCurrentUserIdentityQuery, CurrentUserIdentityDto>
{
    public async Task<CurrentUserIdentityDto> Handle(GetCurrentUserIdentityQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving current user identity for {UserId}", request.UserId);
        var user = await authService.GetUserAsync(request.UserId);
        return AuthMapper.ToIdentityDto(user);
    }
}
