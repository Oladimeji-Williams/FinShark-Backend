namespace FinShark.Application.Dtos;

/// <summary>
/// Request DTO for user registration
/// </summary>
public sealed record RegisterRequestDto(
    string Email,
    string Password,
    string? UserName = null,
    string? FirstName = null,
    string? LastName = null
);

/// <summary>
/// Request DTO for updating a user's profile
/// </summary>
public sealed record UpdateProfileRequestDto(
    string? UserName = null,
    string? FirstName = null,
    string? LastName = null
);

/// <summary>
/// Request DTO for user login
/// </summary>
public sealed record LoginRequestDto(
    string Email,
    string Password
);

/// <summary>
/// Response DTO for authentication operations
/// </summary>
public sealed record AuthResponseDto(
    bool Success,
    string? Token = null,
    string? Message = null,
    IEnumerable<string>? Errors = null,
    UserDto? User = null
);

/// <summary>
/// Response DTO for user information
/// </summary>
public sealed record UserDto(
    string Id,
    string UserName,
    string Email,
    DateTime Created,
    string? FirstName = null,
    string? LastName = null,
    string? FullName = null,
    DateTime? Modified = null
);