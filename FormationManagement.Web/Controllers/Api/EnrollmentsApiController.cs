using FormationManagement.Application.Interfaces;
using FormationManagement.Domain.Enums;
using FormationManagement.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FormationManagement.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnrollmentsApiController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly UserManager<ApplicationUser> _userManager;

    public EnrollmentsApiController(IEnrollmentService enrollmentService, UserManager<ApplicationUser> userManager)
    {
        _enrollmentService = enrollmentService;
        _userManager = userManager;
    }

    /// <summary>GET /api/enrollments/mine — the current learner's own enrollments.</summary>
    [HttpGet("mine")]
    [Authorize(Roles = ApplicationRoles.Learner)]
    public async Task<IActionResult> Mine()
    {
        var userId = _userManager.GetUserId(User)!;
        var enrollments = await _enrollmentService.GetForLearnerAsync(userId);
        return Ok(enrollments);
    }

    /// <summary>POST /api/enrollments {"courseId": 5}</summary>
    [HttpPost]
    [Authorize(Roles = ApplicationRoles.Learner)]
    public async Task<IActionResult> Enroll([FromBody] EnrollRequest request)
    {
        var userId = _userManager.GetUserId(User)!;

        try
        {
            var id = await _enrollmentService.EnrollAsync(userId, request.CourseId);
            return CreatedAtAction(nameof(Mine), new { id });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>PATCH /api/enrollments/5/progress {"progress": 42.5}</summary>
    [HttpPatch("{enrollmentId:int}/progress")]
    [Authorize(Roles = ApplicationRoles.Learner)]
    public async Task<IActionResult> UpdateProgress(int enrollmentId, [FromBody] UpdateProgressRequest request)
    {
        await _enrollmentService.UpdateProgressAsync(enrollmentId, request.Progress);
        return NoContent();
    }

    public record EnrollRequest(int CourseId);
    public record UpdateProgressRequest(decimal Progress);
}
