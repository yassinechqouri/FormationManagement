using FormationManagement.Domain.Enums;
using FormationManagement.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FormationManagement.Web.Controllers.Admin;

[Authorize(Roles = ApplicationRoles.Administrator)]
[Route("Admin/[controller]")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users.OrderByDescending(u => u.CreatedAt).ToList();

        var rows = new List<(ApplicationUser User, IList<string> Roles)>();
        foreach (var user in users)
            rows.Add((user, await _userManager.GetRolesAsync(user)));

        return View(rows);
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        ViewBag.CurrentRoles = await _userManager.GetRolesAsync(user);
        ViewBag.AllRoles = ApplicationRoles.All;
        return View(user);
    }

    [HttpPost("SetRole")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetRole(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, role);

        TempData["Success"] = $"{user.Email} is now a {role}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("ToggleLock")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLock(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        var isLocked = await _userManager.IsLockedOutAsync(user);

        await _userManager.SetLockoutEndDateAsync(user, isLocked ? null : DateTimeOffset.MaxValue);

        TempData["Success"] = isLocked ? $"{user.Email} was unlocked." : $"{user.Email} was locked out.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        await _userManager.DeleteAsync(user);
        TempData["Success"] = "User account was deleted.";
        return RedirectToAction(nameof(Index));
    }
}
