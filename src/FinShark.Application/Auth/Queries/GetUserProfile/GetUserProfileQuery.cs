using FinShark.Application.Dtos;
using MediatorFlow.Core.Contracts;

namespace FinShark.Application.Auth.Queries.GetUserProfile;

public sealed record GetUserProfileQuery(string UserId) : IRequest<UserDto>;
