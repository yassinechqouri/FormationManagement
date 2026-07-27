using FormationManagement.Application.Common.Interfaces;
using FormationManagement.Application.DTOs.Category;
using FormationManagement.Application.Interfaces;
using DomainCategory = FormationManagement.Domain.Entities.Category;

namespace FormationManagement.Application.Services;

/// <summary>Business logic for managing course categories. Used only by Administrators.</summary>
public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync()
    {
        var categories = await _unitOfWork.Categories.FindAsync(includeProperties: "Courses");

        return categories
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                CourseCount = c.Courses.Count
            })
            .OrderBy(c => c.Name)
            .ToList();
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category is null) return null;

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }

    public async Task<int> CreateAsync(CategoryUpsertDto dto)
    {
        var entity = new DomainCategory
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim()
        };

        await _unitOfWork.Categories.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateAsync(CategoryUpsertDto dto)
    {
        var entity = await _unitOfWork.Categories.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Category {dto.Id} not found.");

        entity.Name = dto.Name.Trim();
        entity.Description = dto.Description?.Trim();

        _unitOfWork.Categories.Update(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _unitOfWork.Categories.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Category {id} not found.");

        // Soft delete: keeps historical Course->Category references intact.
        entity.IsDeleted = true;
        _unitOfWork.Categories.Update(entity);
        await _unitOfWork.SaveChangesAsync();
    }
}
