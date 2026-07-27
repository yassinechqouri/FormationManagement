using FormationManagement.Domain.Common;

namespace FormationManagement.Domain.Entities;

/// <summary>A candidate answer for a Question. Exactly one Answer per Question should have IsCorrect = true.</summary>
public class Answer : BaseEntity
{
    public int QuestionId { get; set; }
    public Question? Question { get; set; }

    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
