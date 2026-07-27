using FormationManagement.Application.DTOs.Category;

namespace FormationManagement.Application.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync();
    Task<CategoryDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(CategoryUpsertDto dto);
    Task UpdateAsync(CategoryUpsertDto dto);
    Task DeleteAsync(int id);
}
