using FormationManagement.Domain.Common;

namespace FormationManagement.Domain.Entities;

/// <summary>Links a learner (Identity user) to a course they've enrolled in, tracking progress.</summary>
public class Enrollment : BaseEntity
{
    /// <summary>FK to AspNetUsers.Id of the enrolled learner.</summary>
    public string UserId { get; set; } = string.Empty;

    public int CourseId { get; set; }
    public Course? Course { get; set; }

    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

    /// <summary>Completion percentage, 0-100.</summary>
    public decimal Progress { get; set; } = 0;
}
