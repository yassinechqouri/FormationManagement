using FormationManagement.Domain.Common;

namespace FormationManagement.Domain.Entities;

/// <summary>A practical exercise attached to a lesson.</summary>
public class Exercise : BaseEntity
{
    public int LessonId { get; set; }
    public Lesson? Lesson { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
