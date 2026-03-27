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
/// Request DTO for changing the user's password
/// </summary>
public sealed record ChangePasswordRequestDto(
    string CurrentPassword,
    string NewPassword
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
    UserDto? User = null,
    string? EmailConfirmationUrl = null,
    bool EmailSent = false
);

/// <summary>
/// Response DTO for user information
/// </summary>
public sealed record AssignRoleRequestDto(
    string UserId,
    string Role
);

public sealed record EmailConfirmationRequestDto(
    string UserId,
    string Token
);

public sealed record ResendEmailConfirmationRequestDto(
    string Email
);

public sealed record CurrentUserIdentityDto(
    string UserId,
    string UserName,
    string Email,
    IEnumerable<string> Roles,
    string? FirstName = null,
    string? LastName = null,
    string? FullName = null
);

public sealed record UserDto(
    string Id,
    string UserName,
    string Email,
    DateTime Created,
    IEnumerable<string> Roles,
    string? FirstName = null,
    string? LastName = null,
    string? FullName = null,
    DateTime? Modified = null
);
