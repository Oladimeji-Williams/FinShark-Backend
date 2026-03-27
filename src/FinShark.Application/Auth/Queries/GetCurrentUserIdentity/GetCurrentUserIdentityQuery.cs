using FinShark.Application.Dtos;
using MediatR;

namespace FinShark.Application.Auth.Queries.GetCurrentUserIdentity;

public sealed record GetCurrentUserIdentityQuery(string UserId) : IRequest<CurrentUserIdentityDto>;
