using FormationManagement.Application.Common.Interfaces;
using FormationManagement.Application.DTOs.Quiz;
using FormationManagement.Application.Interfaces;
using DomainExercise = FormationManagement.Domain.Entities.Exercise;
using DomainQuiz = FormationManagement.Domain.Entities.Quiz;
using DomainQuestion = FormationManagement.Domain.Entities.Question;
using DomainAnswer = FormationManagement.Domain.Entities.Answer;

namespace FormationManagement.Application.Services;

public class QuizExerciseService : IQuizExerciseService
{
    private readonly IUnitOfWork _unitOfWork;

    public QuizExerciseService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // ---------- Exercises ----------

    public async Task<IReadOnlyList<ExerciseDto>> GetExercisesForLessonAsync(int lessonId)
    {
        var exercises = await _unitOfWork.Exercises.FindAsync(filter: e => e.LessonId == lessonId);
        return exercises.Select(e => new ExerciseDto { Id = e.Id, LessonId = e.LessonId, Title = e.Title, Description = e.Description }).ToList();
    }

    public async Task<int> CreateExerciseAsync(ExerciseUpsertDto dto)
    {
        var entity = new DomainExercise { LessonId = dto.LessonId, Title = dto.Title.Trim(), Description = dto.Description.Trim() };
        await _unitOfWork.Exercises.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateExerciseAsync(ExerciseUpsertDto dto)
    {
        var entity = await _unitOfWork.Exercises.GetByIdAsync(dto.Id) ?? throw new KeyNotFoundException($"Exercise {dto.Id} not found.");
        entity.Title = dto.Title.Trim();
        entity.Description = dto.Description.Trim();
        _unitOfWork.Exercises.Update(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteExerciseAsync(int exerciseId)
    {
        var entity = await _unitOfWork.Exercises.GetByIdAsync(exerciseId) ?? throw new KeyNotFoundException($"Exercise {exerciseId} not found.");
        entity.IsDeleted = true;
        _unitOfWork.Exercises.Update(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    // ---------- Quizzes ----------

    public async Task<IReadOnlyList<QuizDto>> GetQuizzesForLessonAsync(int lessonId)
    {
        var quizzes = await _unitOfWork.Quizzes.FindAsync(filter: q => q.LessonId == lessonId, includeProperties: "Questions,Questions.Answers");
        return quizzes.Select(ToDto).ToList();
    }

    public async Task<QuizDto?> GetQuizByIdAsync(int quizId)
    {
        var quiz = await _unitOfWork.Quizzes.FirstOrDefaultAsync(q => q.Id == quizId, includeProperties: "Questions,Questions.Answers");
        return quiz is null ? null : ToDto(quiz);
    }

    public async Task<int> CreateQuizAsync(QuizUpsertDto dto)
    {
        var entity = new DomainQuiz { LessonId = dto.LessonId, Title = dto.Title.Trim() };
        await _unitOfWork.Quizzes.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity.Id;
    }

    public async Task DeleteQuizAsync(int quizId)
    {
        var entity = await _unitOfWork.Quizzes.GetByIdAsync(quizId) ?? throw new KeyNotFoundException($"Quiz {quizId} not found.");
        entity.IsDeleted = true;
        _unitOfWork.Quizzes.Update(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    // ---------- Questions & Answers ----------

    public async Task<int> CreateQuestionAsync(QuestionUpsertDto dto)
    {
        ValidateSingleCorrectAnswer(dto);

        var entity = new DomainQuestion
        {
            QuizId = dto.QuizId,
            QuestionText = dto.QuestionText.Trim(),
            Answers = dto.Answers.Select(a => new DomainAnswer { Text = a.Text.Trim(), IsCorrect = a.IsCorrect }).ToList()
        };

        await _unitOfWork.Questions.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateQuestionAsync(QuestionUpsertDto dto)
    {
        ValidateSingleCorrectAnswer(dto);

        var entity = await _unitOfWork.Questions.FirstOrDefaultAsync(q => q.Id == dto.Id, includeProperties: "Answers")
            ?? throw new KeyNotFoundException($"Question {dto.Id} not found.");

        entity.QuestionText = dto.QuestionText.Trim();

        // Simplest consistent strategy: replace all answers rather than trying to diff/merge them.
        foreach (var existingAnswer in entity.Answers.ToList())
            _unitOfWork.Answers.Remove(existingAnswer);

        entity.Answers = dto.Answers.Select(a => new DomainAnswer { QuestionId = entity.Id, Text = a.Text.Trim(), IsCorrect = a.IsCorrect }).ToList();

        _unitOfWork.Questions.Update(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteQuestionAsync(int questionId)
    {
        var entity = await _unitOfWork.Questions.GetByIdAsync(questionId) ?? throw new KeyNotFoundException($"Question {questionId} not found.");
        entity.IsDeleted = true;
        _unitOfWork.Questions.Update(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    // ---------- Taking a quiz ----------

    public async Task<QuizResultDto> GradeSubmissionAsync(QuizSubmissionDto submission)
    {
        var quiz = await _unitOfWork.Quizzes.FirstOrDefaultAsync(q => q.Id == submission.QuizId, includeProperties: "Questions,Questions.Answers")
            ?? throw new KeyNotFoundException($"Quiz {submission.QuizId} not found.");

        var correctCount = 0;

        foreach (var question in quiz.Questions)
        {
            if (!submission.SelectedAnswerIdsByQuestionId.TryGetValue(question.Id, out var selectedAnswerId))
                continue; // unanswered = incorrect, doesn't throw

            var correctAnswer = question.Answers.FirstOrDefault(a => a.IsCorrect);
            if (correctAnswer != null && correctAnswer.Id == selectedAnswerId)
                correctCount++;
        }

        return new QuizResultDto
        {
            QuizId = quiz.Id,
            TotalQuestions = quiz.Questions.Count,
            CorrectAnswers = correctCount
        };
    }

    private static void ValidateSingleCorrectAnswer(QuestionUpsertDto dto)
    {
        if (dto.Answers.Count(a => a.IsCorrect) != 1)
            throw new InvalidOperationException("Exactly one answer must be marked as correct.");
    }

    private static QuizDto ToDto(DomainQuiz q) => new()
    {
        Id = q.Id,
        LessonId = q.LessonId,
        Title = q.Title,
        Questions = q.Questions.Select(qs => new QuestionDto
        {
            Id = qs.Id,
            QuizId = qs.QuizId,
            QuestionText = qs.QuestionText,
            Answers = qs.Answers.Select(a => new AnswerDto { Id = a.Id, Text = a.Text, IsCorrect = a.IsCorrect }).ToList()
        }).ToList()
    };
}
