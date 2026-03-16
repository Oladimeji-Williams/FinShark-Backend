using System.Security.Claims;

namespace FinShark.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string? GetUserId(this ClaimsPrincipal principal)
    {
        if (principal is null) return null;
        return principal.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public static string? GetUserName(this ClaimsPrincipal principal)
    {
        if (principal is null) return null;
        return principal.FindFirstValue(ClaimTypes.Name);
    }

    public static string? GetFirstName(this ClaimsPrincipal principal)
    {
        if (principal is null) return null;
        return principal.FindFirstValue(ClaimTypes.GivenName);
    }

    public static string? GetLastName(this ClaimsPrincipal principal)
    {
        if (principal is null) return null;
        return principal.FindFirstValue(ClaimTypes.Surname);
    }
}
