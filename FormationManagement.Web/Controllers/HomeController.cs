using System.Diagnostics;
using FormationManagement.Application.DTOs.Course;
using FormationManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FormationManagement.Web.Controllers;

public class HomeController : Controller
{
    private readonly ICourseService _courseService;

    public HomeController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    /// <summary>Public landing page: shows a handful of published courses to entice sign-up.</summary>
    public async Task<IActionResult> Index()
    {
        var featured = await _courseService.SearchAsync(new CourseFilterDto
        {
            PublishedOnly = true,
            SortBy = CourseSortOption.MostPopular,
            PageNumber = 1,
            PageSize = 6
        });

        return View(featured.Items);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(Activity.Current?.Id ?? HttpContext.TraceIdentifier);
    }
}
