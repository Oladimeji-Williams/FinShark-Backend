using FinShark.Application.Auth.Services;
using FinShark.Application.Dtos;
using MediatorFlow.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace FinShark.Application.Auth.Queries.GetAllUsers;

public sealed class GetAllUsersQueryHandler(
    IAuthService authService,
    ILogger<GetAllUsersQueryHandler> logger) : IRequestHandler<GetAllUsersQuery, IEnumerable<UserDto>>
{
    public async Task<IEnumerable<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving all users");
        return await authService.GetAllUsersAsync();
    }
}
