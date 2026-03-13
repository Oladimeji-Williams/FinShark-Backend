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
}
