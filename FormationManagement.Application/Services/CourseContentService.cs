using FormationManagement.Application.Common.Interfaces;
using FormationManagement.Application.DTOs.Module;
using FormationManagement.Application.Interfaces;
using DomainModule = FormationManagement.Domain.Entities.Module;
using DomainLesson = FormationManagement.Domain.Entities.Lesson;

namespace FormationManagement.Application.Services;

public class CourseContentService : ICourseContentService
{
    private readonly IUnitOfWork _unitOfWork;

    public CourseContentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ModuleDto>> GetModulesForCourseAsync(int courseId)
    {
        var modules = await _unitOfWork.Modules.FindAsync(
            filter: m => m.CourseId == courseId,
            orderBy: q => q.OrderBy(m => m.Order),
            includeProperties: "Lessons,Lessons.Exercises,Lessons.Quizzes");

        return modules.Select(ToDto).ToList();
    }

    public async Task<ModuleDto?> GetModuleByIdAsync(int moduleId)
    {
        var module = await _unitOfWork.Modules.FirstOrDefaultAsync(m => m.Id == moduleId, includeProperties: "Lessons,Lessons.Exercises,Lessons.Quizzes");
        return module is null ? null : ToDto(module);
    }

    public async Task<int> CreateModuleAsync(ModuleUpsertDto dto)
    {
        var entity = new DomainModule
        {
            CourseId = dto.CourseId,
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            Order = dto.Order
        };

        await _unitOfWork.Modules.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateModuleAsync(ModuleUpsertDto dto)
    {
        var entity = await _unitOfWork.Modules.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Module {dto.Id} not found.");

        entity.Title = dto.Title.Trim();
        entity.Description = dto.Description?.Trim();
        entity.Order = dto.Order;

        _unitOfWork.Modules.Update(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteModuleAsync(int moduleId)
    {
        var entity = await _unitOfWork.Modules.GetByIdAsync(moduleId)
            ?? throw new KeyNotFoundException($"Module {moduleId} not found.");

        entity.IsDeleted = true;
        _unitOfWork.Modules.Update(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<LessonDto?> GetLessonByIdAsync(int lessonId)
    {
        var lesson = await _unitOfWork.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId, includeProperties: "Exercises,Quizzes");
        return lesson is null ? null : ToDto(lesson);
    }

    public async Task<int> CreateLessonAsync(LessonUpsertDto dto)
    {
        var entity = new DomainLesson
        {
            ModuleId = dto.ModuleId,
            Title = dto.Title.Trim(),
            VideoUrl = dto.VideoUrl,
            DocumentUrl = dto.DocumentUrl,
            Duration = dto.Duration
        };

        await _unitOfWork.Lessons.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateLessonAsync(LessonUpsertDto dto)
    {
        var entity = await _unitOfWork.Lessons.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Lesson {dto.Id} not found.");

        entity.Title = dto.Title.Trim();
        entity.VideoUrl = dto.VideoUrl;
        entity.DocumentUrl = dto.DocumentUrl;
        entity.Duration = dto.Duration;

        _unitOfWork.Lessons.Update(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteLessonAsync(int lessonId)
    {
        var entity = await _unitOfWork.Lessons.GetByIdAsync(lessonId)
            ?? throw new KeyNotFoundException($"Lesson {lessonId} not found.");

        entity.IsDeleted = true;
        _unitOfWork.Lessons.Update(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    private static ModuleDto ToDto(DomainModule m) => new()
    {
        Id = m.Id,
        CourseId = m.CourseId,
        Title = m.Title,
        Description = m.Description,
        Order = m.Order,
        Lessons = m.Lessons.OrderBy(l => l.Id).Select(ToDto).ToList()
    };

    private static LessonDto ToDto(DomainLesson l) => new()
    {
        Id = l.Id,
        ModuleId = l.ModuleId,
        Title = l.Title,
        VideoUrl = l.VideoUrl,
        DocumentUrl = l.DocumentUrl,
        Duration = l.Duration,
        ExerciseCount = l.Exercises.Count,
        QuizCount = l.Quizzes.Count
    };
}
