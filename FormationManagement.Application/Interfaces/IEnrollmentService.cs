using FormationManagement.Application.DTOs.Enrollment;

namespace FormationManagement.Application.Interfaces;

public interface IEnrollmentService
{
    Task<IReadOnlyList<EnrollmentDto>> GetAllAsync();
    Task<IReadOnlyList<EnrollmentDto>> GetRecentAsync(int count);
    Task<IReadOnlyList<EnrollmentDto>> GetForCourseAsync(int courseId);
    Task<IReadOnlyList<EnrollmentDto>> GetForLearnerAsync(string userId);
    Task<bool> IsEnrolledAsync(string userId, int courseId);
    Task<int> EnrollAsync(string userId, int courseId);
    Task UpdateProgressAsync(int enrollmentId, decimal progress);
}
