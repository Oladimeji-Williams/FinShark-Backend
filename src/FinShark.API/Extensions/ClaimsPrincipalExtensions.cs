using System.Security.Claims;

namespace FinShark.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string? GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        if (claimsPrincipal is null) return null;
        return claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public static string? GetUserName(this ClaimsPrincipal claimsPrincipal)
    {
        if (claimsPrincipal is null) return null;
        return claimsPrincipal.FindFirstValue(ClaimTypes.Name);
    }

    public static string? GetFirstName(this ClaimsPrincipal claimsPrincipal)
    {
        if (claimsPrincipal is null) return null;
        return claimsPrincipal.FindFirstValue(ClaimTypes.GivenName);
    }

    public static string? GetLastName(this ClaimsPrincipal claimsPrincipal)
    {
        if (claimsPrincipal is null) return null;
        return claimsPrincipal.FindFirstValue(ClaimTypes.Surname);
    }
}
