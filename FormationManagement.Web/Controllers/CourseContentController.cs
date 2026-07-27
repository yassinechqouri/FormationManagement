using FormationManagement.Application.DTOs.Module;
using FormationManagement.Application.DTOs.Quiz;
using FormationManagement.Application.Interfaces;
using FormationManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FormationManagement.Web.Controllers;

/// <summary>
/// Manages Modules -> Lessons -> Exercises/Quizzes/Questions. Shared between
/// Administrator (any course) and Trainer (their own courses) — the Trainer's
/// menu links here with the same actions; ownership is enforced in
/// TrainerController before linking here, since a Module/Lesson doesn't carry
/// TrainerId directly (only its parent Course does).
/// </summary>
[Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Trainer}")]
[Route("Content")]
public class CourseContentController : Controller
{
    private readonly ICourseContentService _contentService;
    private readonly IQuizExerciseService _quizExerciseService;

    public CourseContentController(ICourseContentService contentService, IQuizExerciseService quizExerciseService)
    {
        _contentService = contentService;
        _quizExerciseService = quizExerciseService;
    }

    // ---------------- Modules ----------------

    [HttpGet("Module/Create/{courseId:int}")]
    public IActionResult CreateModule(int courseId) => View(new ModuleUpsertDto { CourseId = courseId });

    [HttpPost("Module/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateModule(ModuleUpsertDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        await _contentService.CreateModuleAsync(dto);
        TempData["Success"] = $"Module \"{dto.Title}\" was added.";
        return RedirectToAction("Content", "Courses", new { area = "", courseId = dto.CourseId });
    }

    [HttpGet("Module/Edit/{id:int}")]
    public async Task<IActionResult> EditModule(int id)
    {
        var module = await _contentService.GetModuleByIdAsync(id);
        if (module is null) return NotFound();
        return View(new ModuleUpsertDto { Id = module.Id, CourseId = module.CourseId, Title = module.Title, Description = module.Description, Order = module.Order });
    }

    [HttpPost("Module/Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditModule(int id, ModuleUpsertDto dto)
    {
        if (id != dto.Id) return BadRequest();
        if (!ModelState.IsValid) return View(dto);
        await _contentService.UpdateModuleAsync(dto);
        return RedirectToAction("Content", "Courses", new { area = "", courseId = dto.CourseId });
    }

    [HttpPost("Module/Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteModule(int id, int courseId)
    {
        await _contentService.DeleteModuleAsync(id);
        return RedirectToAction("Content", "Courses", new { area = "", courseId });
    }

    // ---------------- Lessons ----------------

    [HttpGet("Lesson/Create/{moduleId:int}")]
    public async Task<IActionResult> CreateLesson(int moduleId)
    {
        var module = await _contentService.GetModuleByIdAsync(moduleId);
        ViewBag.CourseId = module?.CourseId ?? 0;
        return View(new LessonUpsertDto { ModuleId = moduleId });
    }

    [HttpPost("Lesson/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLesson(LessonUpsertDto dto, int courseId)
    {
        if (!ModelState.IsValid) return View(dto);
        await _contentService.CreateLessonAsync(dto);
        TempData["Success"] = $"Lesson \"{dto.Title}\" was added.";
        return RedirectToAction("Content", "Courses", new { area = "", courseId });
    }

    [HttpGet("Lesson/Edit/{id:int}")]
    public async Task<IActionResult> EditLesson(int id)
    {
        var lesson = await _contentService.GetLessonByIdAsync(id);
        if (lesson is null) return NotFound();

        var module = await _contentService.GetModuleByIdAsync(lesson.ModuleId);
        ViewBag.CourseId = module?.CourseId ?? 0;

        return View(new LessonUpsertDto { Id = lesson.Id, ModuleId = lesson.ModuleId, Title = lesson.Title, VideoUrl = lesson.VideoUrl, DocumentUrl = lesson.DocumentUrl, Duration = lesson.Duration });
    }

    [HttpPost("Lesson/Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditLesson(int id, LessonUpsertDto dto, int courseId)
    {
        if (id != dto.Id) return BadRequest();
        if (!ModelState.IsValid) return View(dto);
        await _contentService.UpdateLessonAsync(dto);
        return RedirectToAction("Content", "Courses", new { area = "", courseId });
    }

    [HttpPost("Lesson/Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLesson(int id, int courseId)
    {
        await _contentService.DeleteLessonAsync(id);
        return RedirectToAction("Content", "Courses", new { area = "", courseId });
    }

    // ---------------- Exercises ----------------

    [HttpPost("Exercise/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateExercise(ExerciseUpsertDto dto, int courseId)
    {
        await _quizExerciseService.CreateExerciseAsync(dto);
        return RedirectToAction("Content", "Courses", new { area = "", courseId });
    }

    [HttpPost("Exercise/Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteExercise(int id, int courseId)
    {
        await _quizExerciseService.DeleteExerciseAsync(id);
        return RedirectToAction("Content", "Courses", new { area = "", courseId });
    }

    // ---------------- Quizzes & Questions ----------------

    [HttpPost("Quiz/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateQuiz(QuizUpsertDto dto, int courseId)
    {
        await _quizExerciseService.CreateQuizAsync(dto);
        return RedirectToAction("Content", "Courses", new { area = "", courseId });
    }

    [HttpPost("Quiz/Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuiz(int id, int courseId)
    {
        await _quizExerciseService.DeleteQuizAsync(id);
        return RedirectToAction("Content", "Courses", new { area = "", courseId });
    }

    [HttpGet("Question/Create/{quizId:int}")]
    public IActionResult CreateQuestion(int quizId)
    {
        var dto = new QuestionUpsertDto { QuizId = quizId, Answers = new List<AnswerInputDto> { new(), new() } };
        return View(dto);
    }

    [HttpPost("Question/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateQuestion(QuestionUpsertDto dto, int courseId)
    {
        if (!ModelState.IsValid) return View(dto);

        try
        {
            await _quizExerciseService.CreateQuestionAsync(dto);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }

        return RedirectToAction("Content", "Courses", new { area = "", courseId });
    }

    [HttpPost("Question/Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuestion(int id, int courseId)
    {
        await _quizExerciseService.DeleteQuestionAsync(id);
        return RedirectToAction("Content", "Courses", new { area = "", courseId });
    }
}
