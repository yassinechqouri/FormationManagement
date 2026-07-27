using FormationManagement.Domain.Common;

namespace FormationManagement.Domain.Entities;

/// <summary>A single lesson inside a module. Can host a video, a document, an exercise and a quiz.</summary>
public class Lesson : BaseEntity
{
    public int ModuleId { get; set; }
    public Module? Module { get; set; }

    public string Title { get; set; } = string.Empty;

    /// <summary>URL of the lesson video (external host or wwwroot/uploads/videos).</summary>
    public string? VideoUrl { get; set; }

    /// <summary>URL/path of a supporting document (PDF, slides, etc).</summary>
    public string? DocumentUrl { get; set; }

    /// <summary>Estimated duration in minutes.</summary>
    public int Duration { get; set; }

    // Navigation
    public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
    public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
}
