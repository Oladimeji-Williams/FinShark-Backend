using FinShark.Application.Auth.Services;
using FinShark.Application.Dtos;
using FinShark.Application.Mappers;
using FinShark.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FinShark.Persistence.Services;

/// <summary>
/// Implementation of authentication service using ASP.NET Identity
/// </summary>
public sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IConfiguration configuration,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        try
        {
            // Check if user already exists by email
            var existingUserByEmail = await userManager.FindByEmailAsync(request.Email);
            if (existingUserByEmail != null)
            {
                return new AuthResponseDto(false, null, "User with this email already exists");
            }

            // Determine username (default to email if not provided)
            var userName = string.IsNullOrWhiteSpace(request.UserName) ? request.Email : request.UserName;

            // Check if username is already taken
            var existingUserByName = await userManager.FindByNameAsync(userName);
            if (existingUserByName != null)
            {
                return new AuthResponseDto(false, null, "Username is already taken");
            }

            // Create new user
            var user = new ApplicationUser
            {
                UserName = userName,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                EmailConfirmed = false
            };

            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                logger.LogWarning("User registration failed for {Email}: {Errors}", request.Email, string.Join(", ", errors));
                return new AuthResponseDto(false, null, "Registration failed", errors);
            }

            // Ensure default roles exist
            var defaultRoles = new[] { "User", "Admin" };
            foreach (var role in defaultRoles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole(role));
                    if (!roleResult.Succeeded)
                    {
                        var errors = roleResult.Errors.Select(e => e.Description);
                        return new AuthResponseDto(false, null, "Failed to create default roles", errors);
                    }
                }
            }

            // Assign default user role to new users
            var addRoleResult = await userManager.AddToRoleAsync(user, "User");
            if (!addRoleResult.Succeeded)
            {
                var errors = addRoleResult.Errors.Select(e => e.Description);
                logger.LogWarning("Failed to assign role to user {Email}: {Errors}", request.Email, string.Join(", ", errors));
                return new AuthResponseDto(false, null, "Registration failed", errors);
            }

            var userRoles = await userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, userRoles);
            logger.LogInformation("User registered successfully: {Email}", request.Email);
            return new AuthResponseDto(true, token, "Registration successful", null, UserMapper.ToDto(user, userRoles));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during user registration for {Email}", request.Email);
            return new AuthResponseDto(false, null, "An error occurred during registration");
        }
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        try
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return new AuthResponseDto(false, null, "Invalid email or password");
            }

            // Check if account is locked out
            if (await userManager.IsLockedOutAsync(user))
            {
                return new AuthResponseDto(false, null, "Account is locked out");
            }

            var result = await userManager.CheckPasswordAsync(user, request.Password);

            if (!result)
            {
                // Record failed attempt
                await userManager.AccessFailedAsync(user);

                // Check if account should be locked out
                if (await userManager.IsLockedOutAsync(user))
                {
                    logger.LogWarning("Account locked out due to multiple failed attempts: {Email}", request.Email);
                    return new AuthResponseDto(false, null, "Account is locked out due to multiple failed attempts");
                }

                return new AuthResponseDto(false, null, "Invalid email or password");
            }

            if (!user.EmailConfirmed)
            {
                return new AuthResponseDto(false, null, "Email not confirmed");
            }

            // Reset access failed count on successful login
            await userManager.ResetAccessFailedCountAsync(user);

            // Generate JWT token
            var userRoles = await userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, userRoles);

            logger.LogInformation("User logged in successfully: {Email}", request.Email);
            return new AuthResponseDto(true, token, "Login successful", null, UserMapper.ToDto(user, userRoles));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during user login for {Email}", request.Email);
            return new AuthResponseDto(false, null, "An error occurred during login");
        }
    }

    public async Task<AuthResponseDto> UpdateProfileAsync(string userId, UpdateProfileRequestDto request)
    {
        try
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new AuthResponseDto(false, null, "User not found");
            }

            if (!string.IsNullOrWhiteSpace(request.UserName) && request.UserName != user.UserName)
            {
                var existing = await userManager.FindByNameAsync(request.UserName);
                if (existing != null && existing.Id != user.Id)
                {
                    return new AuthResponseDto(false, null, "Username is already taken");
                }

                user.UserName = request.UserName;
            }

            if (request.FirstName != null)
            {
                user.FirstName = request.FirstName;
            }

            if (request.LastName != null)
            {
                user.LastName = request.LastName;
            }

            user.Modified = DateTime.UtcNow;

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = updateResult.Errors.Select(e => e.Description);
                logger.LogWarning("User profile update failed for {UserId}: {Errors}", user.Id, string.Join(", ", errors));
                return new AuthResponseDto(false, null, "Profile update failed", errors);
            }

            // Generate a new token to ensure claims stay in sync
            var updatedRoles = await userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, updatedRoles);

            logger.LogInformation("User profile updated successfully: {UserId}", user.Id);
            return new AuthResponseDto(true, token, "Profile updated successfully", null, UserMapper.ToDto(user, updatedRoles));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating profile for {UserId}", userId);
            return new AuthResponseDto(false, null, "An error occurred while updating profile");
        }
    }

    public async Task<UserDto> GetUserAsync(string userId)
    {
        try
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            var roles = await userManager.GetRolesAsync(user);
            return UserMapper.ToDto(user, roles);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user {UserId}", userId);
            throw;
        }
    }

    public async Task<AuthResponseDto> ChangePasswordAsync(string userId, ChangePasswordRequestDto request)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new AuthResponseDto(false, null, "User not found");
        }

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return new AuthResponseDto(false, null, "Failed to change password", errors);
        }

        var roles = await userManager.GetRolesAsync(user);
        var token = GenerateJwtToken(user, roles);
        return new AuthResponseDto(true, token, "Password changed successfully", null, UserMapper.ToDto(user, roles));
    }

    public async Task<string?> GenerateEmailConfirmationTokenAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        return await userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<AuthResponseDto> ResendEmailConfirmationAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return new AuthResponseDto(false, null, "User not found");
        }

        if (user.EmailConfirmed)
        {
            return new AuthResponseDto(false, null, "Email already confirmed");
        }

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var roles = await userManager.GetRolesAsync(user);
        return new AuthResponseDto(true, token, "Email confirmation token generated", null, UserMapper.ToDto(user, roles));
    }

    public async Task<AuthResponseDto> ConfirmEmailAsync(string userId, string token)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new AuthResponseDto(false, null, "User not found");
        }

        var result = await userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return new AuthResponseDto(false, null, "Email confirmation failed", errors);
        }

        var roles = await userManager.GetRolesAsync(user);
        return new AuthResponseDto(true, null, "Email confirmed successfully", null, UserMapper.ToDto(user, roles));
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = new List<UserDto>();
        foreach (var user in userManager.Users)
        {
            var roles = await userManager.GetRolesAsync(user);
            users.Add(UserMapper.ToDto(user, roles));
        }

        return users;
    }

    public async Task<AuthResponseDto> AssignRoleAsync(AssignRoleRequestDto request)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return new AuthResponseDto(false, null, "User not found");
        }

        if (!await roleManager.RoleExistsAsync(request.Role))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(request.Role));
            if (!roleResult.Succeeded)
            {
                var errors = roleResult.Errors.Select(e => e.Description);
                return new AuthResponseDto(false, null, "Failed to create role", errors);
            }
        }

        if (await userManager.IsInRoleAsync(user, request.Role))
        {
            return new AuthResponseDto(true, null, "User already has role", null, UserMapper.ToDto(user, await userManager.GetRolesAsync(user)));
        }

        var addRoleResult = await userManager.AddToRoleAsync(user, request.Role);
        if (!addRoleResult.Succeeded)
        {
            var errors = addRoleResult.Errors.Select(e => e.Description);
            return new AuthResponseDto(false, null, "Failed to assign role", errors);
        }

        var roles = await userManager.GetRolesAsync(user);
        return new AuthResponseDto(true, null, "Role assigned successfully", null, UserMapper.ToDto(user, roles));
    }

    private string GenerateJwtToken(ApplicationUser user, IEnumerable<string> roles)
    {
        var jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
        var jwtIssuer = configuration["Jwt:Issuer"] ?? "FinShark";
        var jwtAudience = configuration["Jwt:Audience"] ?? "FinSharkUsers";
        var jwtExpiryInMinutes = int.Parse(configuration["Jwt:ExpiryInMinutes"] ?? "60");

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.Email, user.Email!)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(jwtExpiryInMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}