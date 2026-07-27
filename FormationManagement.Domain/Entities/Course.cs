using FormationManagement.Domain.Common;
using FormationManagement.Domain.Enums;

namespace FormationManagement.Domain.Entities;

/// <summary>A course offered in the catalog, owned by a single Trainer.</summary>
public class Course : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public int TrainerId { get; set; }
    public Trainer? Trainer { get; set; }

    public CourseLevel Level { get; set; } = CourseLevel.Beginner;

    /// <summary>Price in the app's base currency. 0 = free course.</summary>
    public decimal Price { get; set; }

    /// <summary>Total estimated duration, in minutes.</summary>
    public int Duration { get; set; }

    /// <summary>Relative path/URL to the course thumbnail (wwwroot/uploads/courses).</summary>
    public string? Thumbnail { get; set; }

    /// <summary>Whether the course is visible to learners in the public catalog.</summary>
    public bool Published { get; set; } = false;

    // Navigation
    public ICollection<Module> Modules { get; set; } = new List<Module>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
