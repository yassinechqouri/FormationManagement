using FormationManagement.Application.Interfaces;
using FormationManagement.Domain.Enums;
using FormationManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FormationManagement.Web.Controllers.Admin;

[Authorize(Roles = ApplicationRoles.Administrator)]
[Route("Admin/[controller]")]
public class ReportsController : Controller
{
    private readonly IUserDirectoryService _userDirectory;
    private readonly ICourseService _courseService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly ExportService _exportService;

    public ReportsController(
        IUserDirectoryService userDirectory,
        ICourseService courseService,
        IEnrollmentService enrollmentService,
        ExportService exportService)
    {
        _userDirectory = userDirectory;
        _courseService = courseService;
        _enrollmentService = enrollmentService;
        _exportService = exportService;
    }

    [HttpGet("")]
    public IActionResult Index() => View();

    [HttpGet("Users/Excel")]
    public async Task<IActionResult> UsersExcel()
    {
        var users = await _userDirectory.GetAllAsync();
        var bytes = _exportService.BuildUsersExcel(users);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"users-{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    [HttpGet("Users/Pdf")]
    public async Task<IActionResult> UsersPdf()
    {
        var users = await _userDirectory.GetAllAsync();
        var bytes = _exportService.BuildUsersPdf(users);
        return File(bytes, "application/pdf", $"users-{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    [HttpGet("Courses/Excel")]
    public async Task<IActionResult> CoursesExcel()
    {
        var courses = await _courseService.SearchAsync(new Application.DTOs.Course.CourseFilterDto { PageSize = int.MaxValue });
        var bytes = _exportService.BuildCoursesExcel(courses.Items);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"courses-{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    [HttpGet("Courses/Pdf")]
    public async Task<IActionResult> CoursesPdf()
    {
        var courses = await _courseService.SearchAsync(new Application.DTOs.Course.CourseFilterDto { PageSize = int.MaxValue });
        var bytes = _exportService.BuildCoursesPdf(courses.Items);
        return File(bytes, "application/pdf", $"courses-{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    [HttpGet("Enrollments/Excel")]
    public async Task<IActionResult> EnrollmentsExcel()
    {
        var enrollments = await _enrollmentService.GetAllAsync();
        var bytes = _exportService.BuildEnrollmentsExcel(enrollments);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"enrollments-{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    [HttpGet("Enrollments/Pdf")]
    public async Task<IActionResult> EnrollmentsPdf()
    {
        var enrollments = await _enrollmentService.GetAllAsync();
        var bytes = _exportService.BuildEnrollmentsPdf(enrollments);
        return File(bytes, "application/pdf", $"enrollments-{DateTime.UtcNow:yyyyMMdd}.pdf");
    }
}
