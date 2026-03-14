using FinShark.Application.Dtos;

namespace FinShark.Application.Auth.Services;

/// <summary>
/// Abstraction for authentication operations.
/// Implementations should live in infrastructure/persistence layers.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user and returns authentication details.
    /// </summary>
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);

    /// <summary>
    /// Logs in a user and returns authentication details.
    /// </summary>
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);

    /// <summary>
    /// Updates a user's profile information.
    /// </summary>
    Task<AuthResponseDto> UpdateProfileAsync(string userId, UpdateProfileRequestDto request);

    /// <summary>
    /// Gets a user's profile information.
    /// </summary>
    Task<UserDto> GetUserAsync(string userId);

    /// <summary>
    /// Gets all users (admin only).
    /// </summary>
    Task<IEnumerable<UserDto>> GetAllUsersAsync();

    /// <summary>
    /// Assigns a role to a user (admin only).
    /// </summary>
    Task<AuthResponseDto> AssignRoleAsync(AssignRoleRequestDto request);

    /// <summary>
    /// Changes a user's password.
    /// </summary>
    Task<AuthResponseDto> ChangePasswordAsync(string userId, ChangePasswordRequestDto request);

    /// <summary>
    /// Generates an email confirmation token for the user.
    /// </summary>
    Task<string?> GenerateEmailConfirmationTokenAsync(string userId);

    /// <summary>
    /// Sends an email confirmation token to a user.
    /// </summary>
    Task<AuthResponseDto> ResendEmailConfirmationAsync(string email);

    /// <summary>
    /// Confirms a user's email with token.
    /// </summary>
    Task<AuthResponseDto> ConfirmEmailAsync(string userId, string token);
}
