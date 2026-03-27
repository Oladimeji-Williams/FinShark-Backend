using FinShark.Application.Auth.Commands.AssignRole;
using FinShark.Application.Auth.Commands.ChangePassword;
using FinShark.Application.Auth.Commands.Login;
using FinShark.Application.Auth.Commands.Register;
using FinShark.Application.Auth.Commands.UpdateProfile;
using FinShark.Application.Dtos;

namespace FinShark.Application.Mappers;

public static class AuthMapper
{
    public static RegisterRequestDto ToRequest(RegisterCommand command)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));

        return new RegisterRequestDto(
            command.Email,
            command.Password,
            command.UserName,
            command.FirstName,
            command.LastName);
    }

    public static LoginRequestDto ToRequest(LoginCommand command)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));

        return new LoginRequestDto(command.Email, command.Password);
    }

    public static UpdateProfileRequestDto ToRequest(UpdateProfileCommand command)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));

        return new UpdateProfileRequestDto(
            command.UserName,
            command.FirstName,
            command.LastName);
    }

    public static ChangePasswordRequestDto ToRequest(ChangePasswordCommand command)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));

        return new ChangePasswordRequestDto(command.CurrentPassword, command.NewPassword);
    }

    public static AssignRoleRequestDto ToRequest(AssignRoleCommand command)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));

        return new AssignRoleRequestDto(command.UserId, command.Role);
    }

    public static CurrentUserIdentityDto ToIdentityDto(UserDto user)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        return new CurrentUserIdentityDto(
            user.Id,
            user.UserName,
            user.Email,
            user.Roles,
            user.FirstName,
            user.LastName,
            user.FullName);
    }
}
