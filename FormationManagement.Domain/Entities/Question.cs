using FormationManagement.Domain.Common;

namespace FormationManagement.Domain.Entities;

/// <summary>A single question inside a quiz. Assumed single-correct-answer, multiple-choice.</summary>
public class Question : BaseEntity
{
    public int QuizId { get; set; }
    public Quiz? Quiz { get; set; }

    public string QuestionText { get; set; } = string.Empty;

    // Navigation
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
}
