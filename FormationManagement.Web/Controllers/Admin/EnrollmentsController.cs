using FormationManagement.Application.Interfaces;
using FormationManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FormationManagement.Web.Controllers.Admin;

[Authorize(Roles = ApplicationRoles.Administrator)]
[Route("Admin/[controller]")]
public class EnrollmentsController : Controller
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly ICourseService _courseService;

    public EnrollmentsController(IEnrollmentService enrollmentService, ICourseService courseService)
    {
        _enrollmentService = enrollmentService;
        _courseService = courseService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(int? courseId)
    {
        var enrollments = courseId.HasValue
            ? await _enrollmentService.GetForCourseAsync(courseId.Value)
            : await _enrollmentService.GetAllAsync();

        ViewBag.SelectedCourseId = courseId;
        return View(enrollments);
    }
}
