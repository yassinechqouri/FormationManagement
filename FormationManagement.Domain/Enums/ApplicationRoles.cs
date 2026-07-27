namespace FormationManagement.Domain.Enums;

/// <summary>
/// Centralised role names. Using constants (instead of magic strings)
/// avoids typos in [Authorize(Roles = "...")] attributes throughout the app.
/// </summary>
public static class ApplicationRoles
{
    public const string Administrator = "Administrator";
    public const string Trainer = "Trainer";
    public const string Learner = "Learner";

    public static readonly string[] All = { Administrator, Trainer, Learner };
}
