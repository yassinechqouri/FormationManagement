namespace FormationManagement.Domain.Common;

/// <summary>
/// Base class for all domain entities.
/// Provides a strongly-typed primary key and audit metadata so that
/// every entity in the system is consistent and traceable.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>Primary key. EF Core will configure this as IDENTITY by convention.</summary>
    public int Id { get; set; }

    /// <summary>UTC timestamp of when the record was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp of the last update. Null if never updated.</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Soft-delete flag. Records are never physically removed from the
    /// database so that historical data (enrollments, progress, etc.)
    /// remains consistent even if a course/trainer is "deleted".
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}
