using FinShark.Application.Dtos;
using MediatR;

namespace FinShark.Application.Auth.Queries.GetAllUsers;

public sealed record GetAllUsersQuery : IRequest<IEnumerable<UserDto>>;
