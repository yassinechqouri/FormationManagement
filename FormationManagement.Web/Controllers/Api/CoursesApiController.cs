using FormationManagement.Application.DTOs.Course;
using FormationManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FormationManagement.Web.Controllers.Api;

/// <summary>
/// Read-only REST API over the public course catalog. Kept separate from the
/// MVC CourseController (which returns Razor views) — this one returns JSON
/// for external clients, mobile apps, or the catalog page's client-side
/// filtering widgets.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class CoursesApiController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesApiController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    /// <summary>GET /api/courses?searchTerm=&categoryId=&trainerId=&level=&sortBy=&pageNumber=&pageSize=</summary>
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] CourseFilterDto filter)
    {
        filter.PublishedOnly = true; // the public API only ever exposes published courses
        var result = await _courseService.SearchAsync(filter);
        return Ok(result);
    }

    /// <summary>GET /api/courses/5</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var course = await _courseService.GetByIdAsync(id);
        if (course is null || !course.Published) return NotFound();
        return Ok(course);
    }
}
