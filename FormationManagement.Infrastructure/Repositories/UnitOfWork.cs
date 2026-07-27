using FormationManagement.Application.Common.Interfaces;
using FormationManagement.Domain.Entities;
using FormationManagement.Infrastructure.Persistence;

namespace FormationManagement.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public IGenericRepository<Category> Categories { get; }
    public IGenericRepository<Trainer> Trainers { get; }
    public IGenericRepository<Course> Courses { get; }
    public IGenericRepository<Domain.Entities.Module> Modules { get; }
    public IGenericRepository<Lesson> Lessons { get; }
    public IGenericRepository<Exercise> Exercises { get; }
    public IGenericRepository<Quiz> Quizzes { get; }
    public IGenericRepository<Question> Questions { get; }
    public IGenericRepository<Answer> Answers { get; }
    public IGenericRepository<Enrollment> Enrollments { get; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;

        // Each repository shares the same DbContext instance (and therefore
        // the same change tracker + transaction) so SaveChangesAsync commits
        // everything together.
        Categories = new GenericRepository<Category>(_context);
        Trainers = new GenericRepository<Trainer>(_context);
        Courses = new GenericRepository<Course>(_context);
        Modules = new GenericRepository<Domain.Entities.Module>(_context);
        Lessons = new GenericRepository<Lesson>(_context);
        Exercises = new GenericRepository<Exercise>(_context);
        Quizzes = new GenericRepository<Quiz>(_context);
        Questions = new GenericRepository<Question>(_context);
        Answers = new GenericRepository<Answer>(_context);
        Enrollments = new GenericRepository<Enrollment>(_context);
    }

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}
