using FormationManagement.Domain.Common;

namespace FormationManagement.Domain.Entities;

/// <summary>Course category (e.g. "Web Development", "Data Science").</summary>
public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
