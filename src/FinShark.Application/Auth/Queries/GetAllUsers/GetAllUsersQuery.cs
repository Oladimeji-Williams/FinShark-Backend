using FinShark.Application.Dtos;
using MediatorFlow.Core.Contracts;

namespace FinShark.Application.Auth.Queries.GetAllUsers;

public sealed record GetAllUsersQuery : IRequest<IEnumerable<UserDto>>;
