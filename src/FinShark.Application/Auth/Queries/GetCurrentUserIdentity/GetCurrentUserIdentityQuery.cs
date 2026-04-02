using FinShark.Application.Dtos;
using MediatorFlow.Core.Contracts;

namespace FinShark.Application.Auth.Queries.GetCurrentUserIdentity;

public sealed record GetCurrentUserIdentityQuery(string UserId) : IRequest<CurrentUserIdentityDto>;
