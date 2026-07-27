using FormationManagement.Application.DTOs.Category;
using FormationManagement.Application.Interfaces;
using FormationManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FormationManagement.Web.Controllers.Admin;

[Authorize(Roles = ApplicationRoles.Administrator)]
[Route("Admin/[controller]")]
public class CategoriesController : Controller
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var categories = await _categoryService.GetAllAsync();
        return View(categories);
    }

    [HttpGet("Create")]
    public IActionResult Create() => View(new CategoryUpsertDto());

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryUpsertDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        await _categoryService.CreateAsync(dto);
        TempData["Success"] = $"Category \"{dto.Name}\" was created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category is null) return NotFound();

        return View(new CategoryUpsertDto { Id = category.Id, Name = category.Name, Description = category.Description });
    }

    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryUpsertDto dto)
    {
        if (id != dto.Id) return BadRequest();
        if (!ModelState.IsValid) return View(dto);

        await _categoryService.UpdateAsync(dto);
        TempData["Success"] = $"Category \"{dto.Name}\" was updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _categoryService.DeleteAsync(id);
        TempData["Success"] = "Category was deleted.";
        return RedirectToAction(nameof(Index));
    }
}
