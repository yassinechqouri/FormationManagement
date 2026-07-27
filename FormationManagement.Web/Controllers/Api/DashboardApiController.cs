using FormationManagement.Application.Services;
using FormationManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FormationManagement.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = ApplicationRoles.Administrator)]
public class DashboardApiController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardApiController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>GET /api/dashboard/stats — same data as the Admin dashboard view, as JSON.</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> Stats()
    {
        var stats = await _dashboardService.GetStatsAsync();
        return Ok(stats);
    }
}
