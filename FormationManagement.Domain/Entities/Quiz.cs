using FormationManagement.Domain.Common;

namespace FormationManagement.Domain.Entities;

/// <summary>A quiz attached to a lesson, made up of one or more questions.</summary>
public class Quiz : BaseEntity
{
    public int LessonId { get; set; }
    public Lesson? Lesson { get; set; }

    public string Title { get; set; } = string.Empty;

    // Navigation
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
