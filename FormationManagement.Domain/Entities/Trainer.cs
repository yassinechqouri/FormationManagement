using FormationManagement.Domain.Common;

namespace FormationManagement.Domain.Entities;

/// <summary>
/// A trainer profile. Linked 1-to-1 with an Identity user (role = Trainer)
/// via <see cref="ApplicationUserId"/>. Kept as a plain string FK here so the
/// Domain project has zero dependency on ASP.NET Core Identity.
/// </summary>
public class Trainer : BaseEntity
{
    /// <summary>FK to AspNetUsers.Id for the trainer's login account.</summary>
    public string ApplicationUserId { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Biography { get; set; }

    /// <summary>Relative path/URL to the profile photo (stored under wwwroot/uploads/trainers).</summary>
    public string? Photo { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    // Navigation
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
