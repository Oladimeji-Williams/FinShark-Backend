using FinShark.Application.Dtos;
using FinShark.Domain.Entities;

namespace FinShark.Application.Mappers;

/// <summary>
/// Provides manual mapping helpers for application users.
/// </summary>
public static class UserMapper
{
    public static UserDto ToDto(ApplicationUser user, IEnumerable<string> roles)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        if (roles == null) throw new ArgumentNullException(nameof(roles));

        return new UserDto(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.Created,
            roles,
            user.FirstName,
            user.LastName,
            user.FullName,
            user.Modified
        );
    }

    public static UserDto ToDto(ApplicationUser user)
    {
        return ToDto(user, Array.Empty<string>());
    }
}
