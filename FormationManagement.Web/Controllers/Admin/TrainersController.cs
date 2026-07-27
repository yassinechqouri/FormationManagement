using FormationManagement.Application.DTOs.Trainer;
using FormationManagement.Application.Interfaces;
using FormationManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FormationManagement.Web.Controllers.Admin;

[Authorize(Roles = ApplicationRoles.Administrator)]
[Route("Admin/[controller]")]
public class TrainersController : Controller
{
    private readonly ITrainerService _trainerService;
    private readonly IUserDirectoryService _userDirectory;

    public TrainersController(ITrainerService trainerService, IUserDirectoryService userDirectory)
    {
        _trainerService = trainerService;
        _userDirectory = userDirectory;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var trainers = await _trainerService.GetAllAsync();
        return View(trainers);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        // Only Identity accounts already holding the Trainer role can be
        // turned into a Trainer profile — keeps the two in sync.
        await PopulateEligibleUsers();
        return View(new TrainerUpsertDto());
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TrainerUpsertDto dto)
    {
        if (!ModelState.IsValid)
        {
            await PopulateEligibleUsers();
            return View(dto);
        }

        await _trainerService.CreateAsync(dto);
        TempData["Success"] = $"Trainer profile for \"{dto.FirstName} {dto.LastName}\" was created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var trainer = await _trainerService.GetByIdAsync(id);
        if (trainer is null) return NotFound();

        return View(new TrainerUpsertDto
        {
            Id = trainer.Id,
            FirstName = trainer.FirstName,
            LastName = trainer.LastName,
            Email = trainer.Email,
            Phone = trainer.Phone,
            Biography = trainer.Biography,
            Photo = trainer.Photo
        });
    }

    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TrainerUpsertDto dto)
    {
        if (id != dto.Id) return BadRequest();
        if (!ModelState.IsValid) return View(dto);

        await _trainerService.UpdateAsync(dto);
        TempData["Success"] = "Trainer profile was updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _trainerService.DeleteAsync(id);
        TempData["Success"] = "Trainer was deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateEligibleUsers()
    {
        var trainerUsers = await _userDirectory.GetByRoleAsync(ApplicationRoles.Trainer);
        ViewBag.EligibleUsers = trainerUsers;
    }
}
