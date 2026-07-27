using FormationManagement.Application.DTOs.Module;

namespace FormationManagement.Application.Interfaces;

/// <summary>Manages the Module -> Lesson tree of a course. Exercises/Quizzes hang off individual lessons (see IQuizExerciseService).</summary>
public interface ICourseContentService
{
    Task<IReadOnlyList<ModuleDto>> GetModulesForCourseAsync(int courseId);
    Task<ModuleDto?> GetModuleByIdAsync(int moduleId);
    Task<int> CreateModuleAsync(ModuleUpsertDto dto);
    Task UpdateModuleAsync(ModuleUpsertDto dto);
    Task DeleteModuleAsync(int moduleId);

    Task<LessonDto?> GetLessonByIdAsync(int lessonId);
    Task<int> CreateLessonAsync(LessonUpsertDto dto);
    Task UpdateLessonAsync(LessonUpsertDto dto);
    Task DeleteLessonAsync(int lessonId);
}
