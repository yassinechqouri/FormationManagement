using FormationManagement.Application.DTOs.Category;
using FormationManagement.Application.DTOs.Course;
using FormationManagement.Application.DTOs.Trainer;
using FormationManagement.Domain.Enums;

namespace FormationManagement.Web.ViewModels.Course;

/// <summary>Bound from the catalog page's search form + used to render the results grid and pager.</summary>
public class CourseCatalogViewModel
{
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public int? TrainerId { get; set; }
    public CourseLevel? Level { get; set; }
    public CourseSortOption SortBy { get; set; } = CourseSortOption.Newest;

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 9;

    public List<CourseDto> Courses { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }

    public List<CategoryDto> Categories { get; set; } = new();
    public List<TrainerDto> Trainers { get; set; } = new();
}
