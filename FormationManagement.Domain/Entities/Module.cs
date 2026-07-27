using FormationManagement.Domain.Common;

namespace FormationManagement.Domain.Entities;

/// <summary>A module (chapter) grouping lessons within a course.</summary>
public class Module : BaseEntity
{
    public int CourseId { get; set; }
    public Course? Course { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Display order within the course (ascending).</summary>
    public int Order { get; set; }

    // Navigation
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
