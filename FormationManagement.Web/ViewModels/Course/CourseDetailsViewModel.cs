using FormationManagement.Application.DTOs.Course;
using FormationManagement.Application.DTOs.Module;

namespace FormationManagement.Web.ViewModels.Course;

public class CourseDetailsViewModel
{
    public CourseDto Course { get; set; } = null!;
    public List<ModuleDto> Modules { get; set; } = new();
    public bool IsEnrolled { get; set; }
    public decimal Progress { get; set; }
}
