namespace FormationManagement.Application.DTOs.Enrollment;

public class EnrollmentDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string LearnerName { get; set; } = string.Empty;
    public string LearnerEmail { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
    public decimal Progress { get; set; }
}
