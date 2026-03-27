using FinShark.Application.Dtos;
using MediatR;

namespace FinShark.Application.Auth.Queries.GetUserProfile;

public sealed record GetUserProfileQuery(string UserId) : IRequest<UserDto>;
