using FinShark.API.Extensions;
using FinShark.Application.Auth.Commands.AssignRole;
using FinShark.Application.Auth.Commands.ChangePassword;
using FinShark.Application.Auth.Commands.ConfirmEmail;
using FinShark.Application.Auth.Commands.Login;
using FinShark.Application.Auth.Commands.Register;
using FinShark.Application.Auth.Commands.ResendEmailConfirmation;
using FinShark.Application.Auth.Commands.TestSmtp;
using FinShark.Application.Auth.Commands.UpdateProfile;
using FinShark.Application.Auth.Queries.GetAllUsers;
using FinShark.Application.Auth.Queries.GetCurrentUserIdentity;
using FinShark.Application.Auth.Queries.GetUserProfile;
using FinShark.Application.Dtos;
using MediatorFlow.Core.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinShark.API.Controllers;

/// <summary>
/// Controller for authentication operations.
/// </summary>
[Route("api/auth")]
[Route("api/account")]
public sealed class AuthController(IMediator mediator) : ApiControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register(
        [FromBody] RegisterRequestDto registerRequestDto,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RegisterCommand(
                registerRequestDto.Email,
                registerRequestDto.Password,
                registerRequestDto.UserName,
                registerRequestDto.FirstName,
                registerRequestDto.LastName),
            cancellationToken);

        if (!result.Success)
        {
            var message = result.Message?.ToLowerInvariant() ?? string.Empty;
            if (message.Contains("user with this email already exists") || message.Contains("username is already taken"))
            {
                return Conflict(ApiResponse<AuthResponseDto>.FailureResponse(
                    result.Message ?? "Resource conflict during registration",
                    result.Errors ?? Array.Empty<string>()));
            }

            return BadRequest(ApiResponse<AuthResponseDto>.FailureResponse(
                result.Message ?? "Registration failed",
                result.Errors ?? Array.Empty<string>()));
        }

        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "User registered successfully"));
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login(
        [FromBody] LoginRequestDto loginRequestDto,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new LoginCommand(loginRequestDto.Email, loginRequestDto.Password), cancellationToken);

        if (!result.Success)
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.FailureResponse(result.Message ?? "Login failed"));
        }

        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Login successful"));
    }

    [HttpPost("resend-confirmation")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> ResendEmailConfirmation(
        [FromBody] ResendEmailConfirmationRequestDto resendEmailConfirmationRequestDto,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ResendEmailConfirmationCommand(resendEmailConfirmationRequestDto.Email), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.FailureResponse(
                result.Message ?? "Failed to send confirmation token",
                result.Errors ?? Array.Empty<string>()));
        }

        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Email confirmation token created"));
    }

    [HttpGet("confirm-email")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> ConfirmEmail(
        [FromQuery] string userId,
        [FromQuery] string token,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ConfirmEmailCommand(userId, token), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.FailureResponse(
                result.Message ?? "Email confirmation failed",
                result.Errors ?? Array.Empty<string>()));
        }

        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Email confirmed successfully"));
    }

    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetProfile(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<UserDto>.FailureResponse("User not authenticated"));
        }

        var user = await mediator.Send(new GetUserProfileQuery(userId), cancellationToken);
        return Ok(ApiResponse<UserDto>.SuccessResponse(user, "Profile retrieved successfully"));
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CurrentUserIdentityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CurrentUserIdentityDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<CurrentUserIdentityDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CurrentUserIdentityDto>>> GetMe(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<CurrentUserIdentityDto>.FailureResponse("User not authenticated"));
        }

        var identity = await mediator.Send(new GetCurrentUserIdentityQuery(userId), cancellationToken);
        return Ok(ApiResponse<CurrentUserIdentityDto>.SuccessResponse(identity, "User identity retrieved successfully"));
    }

    [HttpGet("admin/users")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserDto>>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserDto>>), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserDto>>>> GetAllUsers(CancellationToken cancellationToken)
    {
        var users = await mediator.Send(new GetAllUsersQuery(), cancellationToken);
        return Ok(ApiResponse<IEnumerable<UserDto>>.SuccessResponse(users, "Users retrieved successfully"));
    }

    [HttpPost("admin/assign-role")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> AssignRole(
        [FromBody] AssignRoleRequestDto assignRoleRequestDto,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new AssignRoleCommand(assignRoleRequestDto.UserId, assignRoleRequestDto.Role),
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.FailureResponse(
                result.Message ?? "Failed to assign role",
                result.Errors ?? Array.Empty<string>()));
        }

        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, result.Message ?? "Role assigned successfully"));
    }

    [HttpPatch("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> UpdateProfile(
        [FromBody] UpdateProfileRequestDto updateProfileRequestDto,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.FailureResponse("User not authenticated"));
        }

        var result = await mediator.Send(
            new UpdateProfileCommand(
                userId,
                updateProfileRequestDto.UserName,
                updateProfileRequestDto.FirstName,
                updateProfileRequestDto.LastName),
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.FailureResponse(
                result.Message ?? "Profile update failed",
                result.Errors ?? Array.Empty<string>()));
        }

        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Profile updated successfully"));
    }

    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> ChangePassword(
        [FromBody] ChangePasswordRequestDto changePasswordRequestDto,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.FailureResponse("User not authenticated"));
        }

        var result = await mediator.Send(
            new ChangePasswordCommand(
                userId,
                changePasswordRequestDto.CurrentPassword,
                changePasswordRequestDto.NewPassword),
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.FailureResponse(
                result.Message ?? "Password change failed",
                result.Errors ?? Array.Empty<string>()));
        }

        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Password changed successfully"));
    }

    [HttpGet("smtp-test")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<string>>> TestSmtp(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new TestSmtpCommand(), cancellationToken);
        return Ok(ApiResponse<string>.SuccessResponse(result, "SMTP test sent successfully"));
    }
}
