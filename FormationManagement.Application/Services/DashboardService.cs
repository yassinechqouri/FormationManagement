using FormationManagement.Application.Common.Interfaces;
using FormationManagement.Application.DTOs.Dashboard;
using FormationManagement.Application.Interfaces;

namespace FormationManagement.Application.Services;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync();
}

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserDirectoryService _userDirectory;
    private readonly IEnrollmentService _enrollmentService;

    public DashboardService(IUnitOfWork unitOfWork, IUserDirectoryService userDirectory, IEnrollmentService enrollmentService)
    {
        _unitOfWork = unitOfWork;
        _userDirectory = userDirectory;
        _enrollmentService = enrollmentService;
    }

    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        var totalUsers = await _userDirectory.CountAsync();
        var totalCourses = await _unitOfWork.Courses.CountAsync();
        var publishedCourses = await _unitOfWork.Courses.CountAsync(c => c.Published);
        var totalTrainers = await _unitOfWork.Trainers.CountAsync();
        var totalEnrollments = await _unitOfWork.Enrollments.CountAsync();
        var recentEnrollments = await _enrollmentService.GetRecentAsync(10);

        return new DashboardStatsDto
        {
            TotalUsers = totalUsers,
            TotalCourses = totalCourses,
            TotalTrainers = totalTrainers,
            TotalEnrollments = totalEnrollments,
            PublishedCourses = publishedCourses,
            RecentEnrollments = recentEnrollments.ToList()
        };
    }
}
