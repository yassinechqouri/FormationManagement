using FormationManagement.Domain.Entities;

namespace FormationManagement.Application.Common.Interfaces;

/// <summary>
/// Unit of Work: groups all repositories behind a single object so a service
/// method can touch several aggregates and commit them in one transaction
/// via a single <see cref="SaveChangesAsync"/> call.
/// </summary>
public interface IUnitOfWork
{
    IGenericRepository<Category> Categories { get; }
    IGenericRepository<Trainer> Trainers { get; }
    IGenericRepository<Course> Courses { get; }
    IGenericRepository<Domain.Entities.Module> Modules { get; }
    IGenericRepository<Lesson> Lessons { get; }
    IGenericRepository<Exercise> Exercises { get; }
    IGenericRepository<Quiz> Quizzes { get; }
    IGenericRepository<Question> Questions { get; }
    IGenericRepository<Answer> Answers { get; }
    IGenericRepository<Enrollment> Enrollments { get; }

    Task<int> SaveChangesAsync();
}
