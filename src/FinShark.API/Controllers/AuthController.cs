using FinShark.Application.Auth.Commands.ConfirmEmail;
using FinShark.Application.Auth.Commands.Login;
using FinShark.Application.Auth.Commands.Register;
using FinShark.Application.Auth.Commands.ResendEmailConfirmation;
using FinShark.Application.Auth.Commands.TestSmtp;
using FinShark.Application.Auth.Commands.UpdateProfile;
using FinShark.Application.Auth.Services;
using FinShark.Application.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinShark.API.Extensions;
using System.Security.Claims;

namespace FinShark.API.Controllers;

/// <summary>
/// Controller for authentication operations
/// </summary>
[ApiController]
[Route("api/auth")]
[Route("api/account")]
public sealed class AuthController(IMediator mediator, IAuthService authService) : ControllerBase
{
    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with JWT token</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register(
        [FromBody] RegisterRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(
            request.Email,
            request.Password,
            request.UserName,
            request.FirstName,
            request.LastName);

        var result = await mediator.Send(command, cancellationToken);

        if (!result.Success)
        {
            var message = result.Message?.ToLowerInvariant() ?? string.Empty;
            if (message.Contains("user with this email already exists") || message.Contains("username is already taken"))
            {
                return Conflict(ApiResponse<AuthResponseDto>.FailureResponse(result.Message ?? "Resource conflict during registration", result.Errors ?? Array.Empty<string>()));
            }

            return BadRequest(ApiResponse<AuthResponseDto>.FailureResponse(
                result.Message ?? "Registration failed",
                result.Errors ?? Array.Empty<string>()));
        }

        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "User registered successfully"));
    }

    /// <summary>
    /// Authenticate a user and return JWT token
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with JWT token</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password);

        var result = await mediator.Send(command, cancellationToken);

        if (!result.Success)
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.FailureResponse(
                result.Message ?? "Login failed"));
        }

        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Login successful"));
    }

    /// <summary>
    /// Resend email confirmation token
    /// </summary>
    [HttpPost("resend-confirmation")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> ResendEmailConfirmation(
        [FromBody] ResendEmailConfirmationRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ResendEmailConfirmationCommand(request.Email), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.FailureResponse(result.Message ?? "Failed to send confirmation token", result.Errors ?? Array.Empty<string>()));
        }

        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Email confirmation token created"));
    }

    /// <summary>
    /// Confirm user email
    /// </summary>
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
            return BadRequest(ApiResponse<AuthResponseDto>.FailureResponse(result.Message ?? "Email confirmation failed", result.Errors ?? Array.Empty<string>()));
        }

        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Email confirmed successfully"));
    }

    /// <summary>
    /// Get the current user's profile
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User profile information</returns>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetProfile(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<UserDto>.FailureResponse("User not authenticated"));
        }

        try
        {
            var user = await authService.GetUserAsync(userId);
            return Ok(ApiResponse<UserDto>.SuccessResponse(user, "Profile retrieved successfully"));
        }
        catch (Exception)
        {
            return NotFound(ApiResponse<UserDto>.FailureResponse("User not found"));
        }
    }

    /// <summary>
    /// Get authenticated user claims for id and username
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public ActionResult<ApiResponse<object>> GetMe()
    {
        var userId = User.GetUserId();
        var userName = User.GetUserName();
        var firstName = User.GetFirstName();
        var lastName = User.GetLastName();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<object>.FailureResponse("User not authenticated"));
        }

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            UserId = userId,
            UserName = userName,
            FirstName = firstName,
            LastName = lastName
        }, "User identity retrieved successfully"));
    }

    /// <summary>
    /// Get all users (admin only)
    /// </summary>
    [HttpGet("admin/users")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserDto>>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserDto>>), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserDto>>>> GetAllUsers(CancellationToken cancellationToken)
    {
        var users = await authService.GetAllUsersAsync();
        return Ok(ApiResponse<IEnumerable<UserDto>>.SuccessResponse(users, "Users retrieved successfully"));
    }

    /// <summary>
    /// Assign a role to a user (admin only)
    /// </summary>
    [HttpPost("admin/assign-role")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> AssignRole([FromBody] AssignRoleRequestDto request)
    {
        var result = await authService.AssignRoleAsync(request);

        if (!result.Success)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.FailureResponse(result.Message ?? "Failed to assign role", result.Errors ?? Array.Empty<string>()));
        }

        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, result.Message ?? "Role assigned successfully"));
    }

    /// <summary>
    /// Update the current user's profile (optional fields)
    /// </summary>
    /// <param name="request">Profile update details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with updated user info</returns>
    [HttpPatch("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> UpdateProfile(
        [FromBody] UpdateProfileRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.FailureResponse("User not authenticated"));
        }

        var command = new UpdateProfileCommand(userId, request.UserName, request.FirstName, request.LastName);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.FailureResponse(
                result.Message ?? "Profile update failed",
                result.Errors ?? Array.Empty<string>()));
        }

        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Profile updated successfully"));
    }

    /// <summary>
    /// Test SMTP connectivity (dev only)
    /// </summary>
    [HttpGet("smtp-test")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<string>>> TestSmtp(CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new TestSmtpCommand(), cancellationToken);
            return Ok(ApiResponse<string>.SuccessResponse(result, "SMTP test sent successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.FailureResponse($"SMTP test failed: {ex.Message}"));
        }
    }
}
