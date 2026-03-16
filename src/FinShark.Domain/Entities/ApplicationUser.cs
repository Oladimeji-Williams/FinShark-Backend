using Microsoft.AspNetCore.Identity;

namespace FinShark.Domain.Entities;

/// <summary>
/// Domain representation of an application user
/// Inherits from IdentityUser for ASP.NET Identity compatibility
/// while keeping domain-specific properties
/// </summary>
public sealed class ApplicationUser : IdentityUser
{
    /// <summary>
    /// User's first name
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// User's last name
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// User's full name (computed property)
    /// </summary>
    public string? FullName => string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName)
        ? null
        : $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// When the user account was created
    /// </summary>
    public DateTime Created { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// When the user account was last modified
    /// </summary>
    public DateTime? Modified { get; set; }

    /// <summary>
    /// Navigation property for comments made by this user
    /// </summary>
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    /// <summary>
    /// Navigation property for the user's stock portfolio
    /// </summary>
    public ICollection<PortfolioItem> Portfolio { get; set; } = new List<PortfolioItem>();
}