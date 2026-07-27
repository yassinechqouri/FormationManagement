using FormationManagement.Application.Common.Models;
using FormationManagement.Application.DTOs.Course;

namespace FormationManagement.Application.Interfaces;

public interface ICourseService
{
    Task<PagedResult<CourseDto>> SearchAsync(CourseFilterDto filter);
    Task<CourseDto?> GetByIdAsync(int id);
    Task<IReadOnlyList<CourseDto>> GetByTrainerAsync(int trainerId);
    Task<int> CreateAsync(CourseUpsertDto dto);
    Task UpdateAsync(CourseUpsertDto dto);
    Task DeleteAsync(int id);
    Task SetPublishedAsync(int id, bool published);
}
