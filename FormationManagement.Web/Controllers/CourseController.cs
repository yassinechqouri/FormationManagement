using FormationManagement.Application.DTOs.Course;
using FormationManagement.Application.Interfaces;
using FormationManagement.Web.ViewModels.Course;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FormationManagement.Infrastructure.Identity;
using FormationManagement.Domain.Enums;

namespace FormationManagement.Web.Controllers;

/// <summary>Public course catalog (browse/search/filter/sort/paginate) + learner enrollment actions.</summary>
public class CourseController : Controller
{
    private readonly ICourseService _courseService;
    private readonly ICategoryService _categoryService;
    private readonly ITrainerService _trainerService;
    private readonly ICourseContentService _contentService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly UserManager<ApplicationUser> _userManager;

    public CourseController(
        ICourseService courseService,
        ICategoryService categoryService,
        ITrainerService trainerService,
        ICourseContentService contentService,
        IEnrollmentService enrollmentService,
        UserManager<ApplicationUser> userManager)
    {
        _courseService = courseService;
        _categoryService = categoryService;
        _trainerService = trainerService;
        _contentService = contentService;
        _enrollmentService = enrollmentService;
        _userManager = userManager;
    }

    // GET: /Course?searchTerm=...&categoryId=...&trainerId=...&level=...&sortBy=...&pageNumber=...
    [AllowAnonymous]
    public async Task<IActionResult> Index(CourseCatalogViewModel filter)
    {
        var result = await _courseService.SearchAsync(new CourseFilterDto
        {
            SearchTerm = filter.SearchTerm,
            CategoryId = filter.CategoryId,
            TrainerId = filter.TrainerId,
            Level = filter.Level,
            PublishedOnly = true, // learners/anonymous visitors only ever see published courses
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

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var course = await _courseService.GetByIdAsync(id);
        if (course is null || !course.Published) return NotFound();

        var modules = await _contentService.GetModulesForCourseAsync(id);

        var vm = new CourseDetailsViewModel { Course = course, Modules = modules.ToList() };

        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = _userManager.GetUserId(User);
            if (userId != null)
            {
                vm.IsEnrolled = await _enrollmentService.IsEnrolledAsync(userId, id);
                if (vm.IsEnrolled)
                {
                    var enrollments = await _enrollmentService.GetForLearnerAsync(userId);
                    vm.Progress = enrollments.FirstOrDefault(e => e.CourseId == id)?.Progress ?? 0;
                }
            }
        }

        return View(vm);
    }

    [Authorize(Roles = ApplicationRoles.Learner)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int courseId)
    {
        var userId = _userManager.GetUserId(User)!;

        try
        {
            await _enrollmentService.EnrollAsync(userId, courseId);
            TempData["Success"] = "You have successfully enrolled in this course.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = courseId });
    }

    /// <summary>Learner's "My Courses" page.</summary>
    [Authorize(Roles = ApplicationRoles.Learner)]
    public async Task<IActionResult> MyCourses()
    {
        var userId = _userManager.GetUserId(User)!;
        var enrollments = await _enrollmentService.GetForLearnerAsync(userId);
        return View(enrollments);
    }
}
