using FormationManagement.Application.DTOs.Quiz;
using FormationManagement.Application.Interfaces;
using FormationManagement.Infrastructure.Identity;
using FormationManagement.Web.ViewModels.Course;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FormationManagement.Web.Controllers;

/// <summary>Displays a single lesson to an enrolled learner, including the AI avatar presentation and any quizzes/exercises.</summary>
[Authorize]
public class LessonController : Controller
{
    private readonly ICourseContentService _contentService;
    private readonly IQuizExerciseService _quizExerciseService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly ICourseService _courseService;
    private readonly IAIAvatarService _avatarService;
    private readonly UserManager<ApplicationUser> _userManager;

    public LessonController(
        ICourseContentService contentService,
        IQuizExerciseService quizExerciseService,
        IEnrollmentService enrollmentService,
        ICourseService courseService,
        IAIAvatarService avatarService,
        UserManager<ApplicationUser> userManager)
    {
        _contentService = contentService;
        _quizExerciseService = quizExerciseService;
        _enrollmentService = enrollmentService;
        _courseService = courseService;
        _avatarService = avatarService;
        _userManager = userManager;
    }

    // GET: /Lesson/View/5?courseId=3
    public async Task<IActionResult> View(int id, int courseId)
    {
        var userId = _userManager.GetUserId(User)!;

        // Authorization: must be enrolled in the course (Admins/Trainers bypass this via role check).
        var isPrivileged = User.IsInRole(Domain.Enums.ApplicationRoles.Administrator) || User.IsInRole(Domain.Enums.ApplicationRoles.Trainer);
        if (!isPrivileged && !await _enrollmentService.IsEnrolledAsync(userId, courseId))
            return Forbid();

        var lesson = await _contentService.GetLessonByIdAsync(id);
        if (lesson is null) return NotFound();

        var course = await _courseService.GetByIdAsync(courseId);
        if (course is null) return NotFound();

        var vm = new LessonViewModel
        {
            CourseId = courseId,
            CourseTitle = course.Title,
            Lesson = lesson,
            Exercises = (await _quizExerciseService.GetExercisesForLessonAsync(id)).ToList(),
            Quizzes = (await _quizExerciseService.GetQuizzesForLessonAsync(id)).ToList()
        };

        // Ask the AI avatar to present the lesson. The "script" is a simple
        // fallback built from the lesson title/duration when no document/video
        // description is available — in a fuller build this would come from a
        // dedicated "Script" field on Lesson.
        var script = $"Welcome to \"{lesson.Title}\". This lesson takes about {lesson.Duration} minutes to complete. " +
                     "Let's get started — feel free to ask me any questions as we go.";

        vm.AvatarPresentation = await _avatarService.PresentLessonAsync(lesson.Title, script);

        return View(vm);
    }

    /// <summary>
    /// AJAX endpoint the lesson page calls when the learner types a question
    /// to the avatar. Returns JSON so the front-end can update the avatar
    /// panel without a full page reload.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AskAvatar(int lessonId, string question)
    {
        var lesson = await _contentService.GetLessonByIdAsync(lessonId);
        if (lesson is null) return NotFound();

        var response = await _avatarService.AnswerQuestionAsync(question, lesson.Title);
        return Json(response);
    }

    // ---------------- Quiz taking ----------------

    [HttpGet]
    public async Task<IActionResult> TakeQuiz(int id)
    {
        var quiz = await _quizExerciseService.GetQuizByIdAsync(id);
        if (quiz is null) return NotFound();
        return View(quiz);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitQuiz(int quizId, Dictionary<int, int> answers)
    {
        var result = await _quizExerciseService.GradeSubmissionAsync(new QuizSubmissionDto
        {
            QuizId = quizId,
            SelectedAnswerIdsByQuestionId = answers ?? new Dictionary<int, int>()
        });

        return View("QuizResult", result);
    }
}
