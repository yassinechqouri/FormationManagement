using FormationManagement.Application.DTOs.Quiz;

namespace FormationManagement.Application.Interfaces;

public interface IQuizExerciseService
{
    // Exercises
    Task<IReadOnlyList<ExerciseDto>> GetExercisesForLessonAsync(int lessonId);
    Task<int> CreateExerciseAsync(ExerciseUpsertDto dto);
    Task UpdateExerciseAsync(ExerciseUpsertDto dto);
    Task DeleteExerciseAsync(int exerciseId);

    // Quizzes
    Task<IReadOnlyList<QuizDto>> GetQuizzesForLessonAsync(int lessonId);
    Task<QuizDto?> GetQuizByIdAsync(int quizId);
    Task<int> CreateQuizAsync(QuizUpsertDto dto);
    Task DeleteQuizAsync(int quizId);

    // Questions & Answers
    Task<int> CreateQuestionAsync(QuestionUpsertDto dto);
    Task UpdateQuestionAsync(QuestionUpsertDto dto);
    Task DeleteQuestionAsync(int questionId);

    // Taking a quiz
    Task<QuizResultDto> GradeSubmissionAsync(QuizSubmissionDto submission);
}
