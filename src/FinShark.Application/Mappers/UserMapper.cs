using FinShark.Application.Dtos;
using FinShark.Domain.Entities;

namespace FinShark.Application.Mappers;

/// <summary>
/// Provides manual mapping helpers for application users.
/// </summary>
public static class UserMapper
{
    public static UserDto ToDto(ApplicationUser user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        return new UserDto(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.Created,
            user.FirstName,
            user.LastName,
            user.FullName,
            user.Modified
        );
    }
}
