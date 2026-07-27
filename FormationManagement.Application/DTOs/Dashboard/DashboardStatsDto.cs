using FormationManagement.Application.DTOs.Enrollment;

namespace FormationManagement.Application.DTOs.Dashboard;

public class DashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalCourses { get; set; }
    public int TotalTrainers { get; set; }
    public int TotalEnrollments { get; set; }
    public int PublishedCourses { get; set; }
    public List<EnrollmentDto> RecentEnrollments { get; set; } = new();
}
