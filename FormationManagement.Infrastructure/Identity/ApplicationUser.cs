using Microsoft.AspNetCore.Identity;

namespace FormationManagement.Infrastructure.Identity;

/// <summary>
/// Extends the default Identity user with a couple of profile fields used
/// across the UI (navbar greeting, enrollment display name, etc.).
/// Kept in Infrastructure (not Domain) because it depends on IdentityUser.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    /// <summary>UTC timestamp the account was created, for admin "Manage Users" listing.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string FullName => $"{FirstName} {LastName}".Trim();
}
