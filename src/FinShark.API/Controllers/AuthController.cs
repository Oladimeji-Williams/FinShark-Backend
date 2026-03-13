using FinShark.Application.Auth.Commands.Login;
using FinShark.Application.Auth.Commands.Register;
using FinShark.Application.Auth.Commands.UpdateProfile;
using FinShark.Application.Auth.Services;
using FinShark.Application.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinShark.API.Controllers;

/// <summary>
/// Controller for authentication operations
/// </summary>
[ApiController]
[Route("api/auth")]
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
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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
}
