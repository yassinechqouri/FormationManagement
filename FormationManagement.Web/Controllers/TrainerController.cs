using FormationManagement.Application.DTOs.Course;
using FormationManagement.Application.Interfaces;
using FormationManagement.Domain.Enums;
using FormationManagement.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FormationManagement.Web.Controllers;

/// <summary>Trainer's own workspace: their courses, lesson/quiz content (via CourseContentController), and enrolled students.</summary>
[Authorize(Roles = ApplicationRoles.Trainer)]
[Route("Trainer")]
public class TrainerController : Controller
{
    private readonly ITrainerService _trainerService;
    private readonly ICourseService _courseService;
    private readonly ICategoryService _categoryService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly UserManager<ApplicationUser> _userManager;

    public TrainerController(
        ITrainerService trainerService,
        ICourseService courseService,
        ICategoryService categoryService,
        IEnrollmentService enrollmentService,
        UserManager<ApplicationUser> userManager)
    {
        _trainerService = trainerService;
        _courseService = courseService;
        _categoryService = categoryService;
        _enrollmentService = enrollmentService;
        _userManager = userManager;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var trainer = await GetCurrentTrainerAsync();
        if (trainer is null) return View("NoProfile");

        var courses = await _courseService.GetByTrainerAsync(trainer.Id);
        ViewBag.Trainer = trainer;
        return View(courses);
    }

    [HttpGet("Courses/Create")]
    public async Task<IActionResult> CreateCourse()
    {
        var trainer = await GetCurrentTrainerAsync();
        if (trainer is null) return View("NoProfile");

        ViewBag.Categories = await _categoryService.GetAllAsync();
        return View(new CourseUpsertDto { TrainerId = trainer.Id });
    }

    [HttpPost("Courses/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCourse(CourseUpsertDto dto)
    {
        var trainer = await GetCurrentTrainerAsync();
        if (trainer is null) return View("NoProfile");

        dto.TrainerId = trainer.Id; // a Trainer can only ever create courses under their own profile

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _categoryService.GetAllAsync();
            return View(dto);
        }

        var id = await _courseService.CreateAsync(dto);
        TempData["Success"] = $"Course \"{dto.Title}\" was created. Add modules and lessons below.";
        return RedirectToAction("Content", "Courses", new { area = "", courseId = id });
    }

    [HttpGet("Courses/Edit/{id:int}")]
    public async Task<IActionResult> EditCourse(int id)
    {
        var trainer = await GetCurrentTrainerAsync();
        var course = await _courseService.GetByIdAsync(id);

        if (trainer is null || course is null || course.TrainerId != trainer.Id)
            return Forbid(); // ownership check: a Trainer may only edit their own courses

        ViewBag.Categories = await _categoryService.GetAllAsync();

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

    [HttpPost("Courses/Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCourse(int id, CourseUpsertDto dto)
    {
        var trainer = await GetCurrentTrainerAsync();
        var course = await _courseService.GetByIdAsync(id);

        if (trainer is null || course is null || course.TrainerId != trainer.Id)
            return Forbid();

        dto.Id = id;
        dto.TrainerId = trainer.Id;

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _categoryService.GetAllAsync();
            return View(dto);
        }

        await _courseService.UpdateAsync(dto);
        TempData["Success"] = "Course was updated.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Learners enrolled across all of the trainer's own courses.</summary>
    [HttpGet("Students")]
    public async Task<IActionResult> Students()
    {
        var trainer = await GetCurrentTrainerAsync();
        if (trainer is null) return View("NoProfile");

        var courses = await _courseService.GetByTrainerAsync(trainer.Id);
        var allEnrollments = new List<Application.DTOs.Enrollment.EnrollmentDto>();

        foreach (var course in courses)
            allEnrollments.AddRange(await _enrollmentService.GetForCourseAsync(course.Id));

        return View(allEnrollments.OrderByDescending(e => e.EnrollmentDate).ToList());
    }

    private async Task<Application.DTOs.Trainer.TrainerDto?> GetCurrentTrainerAsync()
    {
        var userId = _userManager.GetUserId(User);
        if (userId is null) return null;
        return await _trainerService.GetByUserIdAsync(userId);
    }
}
