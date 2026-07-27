using FormationManagement.Application.Common.Interfaces;
using FormationManagement.Application.DTOs.Enrollment;
using FormationManagement.Application.Interfaces;
using DomainEnrollment = FormationManagement.Domain.Entities.Enrollment;

namespace FormationManagement.Application.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserDirectoryService _userDirectory;

    public EnrollmentService(IUnitOfWork unitOfWork, IUserDirectoryService userDirectory)
    {
        _unitOfWork = unitOfWork;
        _userDirectory = userDirectory;
    }

    public async Task<IReadOnlyList<EnrollmentDto>> GetAllAsync()
    {
        var enrollments = await _unitOfWork.Enrollments.FindAsync(
            orderBy: q => q.OrderByDescending(e => e.EnrollmentDate),
            includeProperties: "Course");

        return await ToDtosAsync(enrollments);
    }

    public async Task<IReadOnlyList<EnrollmentDto>> GetRecentAsync(int count)
    {
        var enrollments = await _unitOfWork.Enrollments.FindAsync(
            orderBy: q => q.OrderByDescending(e => e.EnrollmentDate),
            includeProperties: "Course",
            take: count);

        return await ToDtosAsync(enrollments);
    }

    public async Task<IReadOnlyList<EnrollmentDto>> GetForCourseAsync(int courseId)
    {
        var enrollments = await _unitOfWork.Enrollments.FindAsync(
            filter: e => e.CourseId == courseId,
            orderBy: q => q.OrderByDescending(e => e.EnrollmentDate),
            includeProperties: "Course");

        return await ToDtosAsync(enrollments);
    }

    public async Task<IReadOnlyList<EnrollmentDto>> GetForLearnerAsync(string userId)
    {
        var enrollments = await _unitOfWork.Enrollments.FindAsync(
            filter: e => e.UserId == userId,
            orderBy: q => q.OrderByDescending(e => e.EnrollmentDate),
            includeProperties: "Course");

        return await ToDtosAsync(enrollments);
    }

    public async Task<bool> IsEnrolledAsync(string userId, int courseId)
    {
        var existing = await _unitOfWork.Enrollments.FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);
        return existing != null;
    }

    public async Task<int> EnrollAsync(string userId, int courseId)
    {
        if (await IsEnrolledAsync(userId, courseId))
            throw new InvalidOperationException("You are already enrolled in this course.");

        var entity = new DomainEnrollment
        {
            UserId = userId,
            CourseId = courseId,
            EnrollmentDate = DateTime.UtcNow,
            Progress = 0
        };

        await _unitOfWork.Enrollments.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateProgressAsync(int enrollmentId, decimal progress)
    {
        var entity = await _unitOfWork.Enrollments.GetByIdAsync(enrollmentId)
            ?? throw new KeyNotFoundException($"Enrollment {enrollmentId} not found.");

        entity.Progress = Math.Clamp(progress, 0, 100);
        _unitOfWork.Enrollments.Update(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<IReadOnlyList<EnrollmentDto>> ToDtosAsync(IReadOnlyList<DomainEnrollment> enrollments)
    {
        var users = await _userDirectory.GetManyByIdAsync(enrollments.Select(e => e.UserId));

        return enrollments.Select(e =>
        {
            users.TryGetValue(e.UserId, out var user);

            return new EnrollmentDto
            {
                Id = e.Id,
                UserId = e.UserId,
                LearnerName = user?.FullName ?? "Unknown learner",
                LearnerEmail = user?.Email ?? string.Empty,
                CourseId = e.CourseId,
                CourseTitle = e.Course?.Title ?? string.Empty,
                EnrollmentDate = e.EnrollmentDate,
                Progress = e.Progress
            };
        }).ToList();
    }
}
