using FormationManagement.Application.DTOs.Module;
using FormationManagement.Application.DTOs.Quiz;
using FormationManagement.Application.Interfaces;

namespace FormationManagement.Web.ViewModels.Course;

public class LessonViewModel
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public LessonDto Lesson { get; set; } = null!;
    public List<ExerciseDto> Exercises { get; set; } = new();
    public List<QuizDto> Quizzes { get; set; } = new();

    /// <summary>Populated once the learner opens the page — the AI avatar's lesson presentation.</summary>
    public AvatarResponse? AvatarPresentation { get; set; }
}
