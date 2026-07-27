using FormationManagement.Application.DTOs.Course;
using FormationManagement.Application.Interfaces;
using FormationManagement.Domain.Enums;
using FormationManagement.Web.ViewModels.Course;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FormationManagement.Web.Controllers.Admin;

[Authorize] // per-action [Authorize(Roles=...)] below narrows this further
[Route("Admin/[controller]")]
public class CoursesController : Controller
{
    private readonly ICourseService _courseService;
    private readonly ICategoryService _categoryService;
    private readonly ITrainerService _trainerService;
    private readonly ICourseContentService _contentService;

    public CoursesController(
        ICourseService courseService,
        ICategoryService categoryService,
        ITrainerService trainerService,
        ICourseContentService contentService)
    {
        _courseService = courseService;
        _categoryService = categoryService;
        _trainerService = trainerService;
        _contentService = contentService;
    }

    [Authorize(Roles = ApplicationRoles.Administrator)]
    [HttpGet("")]
    public async Task<IActionResult> Index(CourseCatalogViewModel filter)
    {
        // Admin sees ALL courses (published or not) — PublishedOnly stays null.
        var result = await _courseService.SearchAsync(new CourseFilterDto
        {
            SearchTerm = filter.SearchTerm,
            CategoryId = filter.CategoryId,
            TrainerId = filter.TrainerId,
            Level = filter.Level,
            SortBy = filter.SortBy,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        });

        filter.Courses = result.Items.ToList();
        filter.TotalCount = result.TotalCount;
        filter.TotalPages = result.TotalPages;
        filter.Categories = (await _categoryService.GetAllAsync()).ToList();
        filter.Trainers = (await _trainerService.GetAllAsync()).ToList();

        return View(filter);
    }

    [Authorize(Roles = ApplicationRoles.Administrator)]
    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        return View(new CourseUpsertDto());
    }

    [Authorize(Roles = ApplicationRoles.Administrator)]
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseUpsertDto dto)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns();
            return View(dto);
        }

        var id = await _courseService.CreateAsync(dto);
        TempData["Success"] = $"Course \"{dto.Title}\" was created.";
        return RedirectToAction(nameof(Content), new { courseId = id });
    }

    [Authorize(Roles = ApplicationRoles.Administrator)]
    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var course = await _courseService.GetByIdAsync(id);
        if (course is null) return NotFound();

        await PopulateDropdowns();

        return View(new CourseUpsertDto
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            CategoryId = course.CategoryId,
            TrainerId = course.TrainerId,
            Level = course.Level,
            Price = course.Price,
            Duration = course.Duration,
            Thumbnail = course.Thumbnail,
            Published = course.Published
        });
    }

    [Authorize(Roles = ApplicationRoles.Administrator)]
    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CourseUpsertDto dto)
    {
        if (id != dto.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            await PopulateDropdowns();
            return View(dto);
        }

        await _courseService.UpdateAsync(dto);
        TempData["Success"] = $"Course \"{dto.Title}\" was updated.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = ApplicationRoles.Administrator)]
    [HttpPost("Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _courseService.DeleteAsync(id);
        TempData["Success"] = "Course was deleted.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = ApplicationRoles.Administrator)]
    [HttpPost("TogglePublish/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TogglePublish(int id, bool published)
    {
        await _courseService.SetPublishedAsync(id, published);
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Content management screen: modules/lessons tree for a course (shared with Trainer's own-course management).</summary>
    [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Trainer}")]
    [HttpGet("Content/{courseId:int}")]
    public async Task<IActionResult> Content(int courseId)
    {
        var course = await _courseService.GetByIdAsync(courseId);
        if (course is null) return NotFound();

        var modules = await _contentService.GetModulesForCourseAsync(courseId);
        ViewBag.Course = course;
        return View(modules);
    }

    private async Task PopulateDropdowns()
    {
        ViewBag.Categories = await _categoryService.GetAllAsync();
        ViewBag.Trainers = await _trainerService.GetAllAsync();
    }
}
