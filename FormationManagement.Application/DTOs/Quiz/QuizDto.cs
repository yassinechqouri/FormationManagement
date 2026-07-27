using System.ComponentModel.DataAnnotations;

namespace FormationManagement.Application.DTOs.Quiz;

public class ExerciseDto
{
    public int Id { get; set; }
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class ExerciseUpsertDto
{
    public int Id { get; set; }
    [Required] public int LessonId { get; set; }
    [Required, StringLength(250)] public string Title { get; set; } = string.Empty;
    [Required, StringLength(4000)] public string Description { get; set; } = string.Empty;
}

public class AnswerDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}

public class QuestionDto
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public List<AnswerDto> Answers { get; set; } = new();
}

public class QuizDto
{
    public int Id { get; set; }
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<QuestionDto> Questions { get; set; } = new();
}

public class QuizUpsertDto
{
    public int Id { get; set; }
    [Required] public int LessonId { get; set; }
    [Required, StringLength(250)] public string Title { get; set; } = string.Empty;
}

/// <summary>Answer options submitted together with a question — at least 2, exactly one marked correct.</summary>
public class AnswerInputDto
{
    [Required, StringLength(500)]
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}

public class QuestionUpsertDto
{
    public int Id { get; set; }
    [Required] public int QuizId { get; set; }
    [Required, StringLength(1000)] public string QuestionText { get; set; } = string.Empty;

    [MinLength(2, ErrorMessage = "Provide at least two answer options.")]
    public List<AnswerInputDto> Answers { get; set; } = new();
}

/// <summary>Learner's submitted choices when taking a quiz: QuestionId -> chosen AnswerId.</summary>
public class QuizSubmissionDto
{
    public int QuizId { get; set; }
    public Dictionary<int, int> SelectedAnswerIdsByQuestionId { get; set; } = new();
}

public class QuizResultDto
{
    public int QuizId { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public double ScorePercentage => TotalQuestions == 0 ? 0 : Math.Round(CorrectAnswers * 100.0 / TotalQuestions, 1);
}
