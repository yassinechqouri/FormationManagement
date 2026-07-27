using System.ComponentModel.DataAnnotations;
using FormationManagement.Domain.Enums;

namespace FormationManagement.Application.DTOs.Course;

public class CourseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int TrainerId { get; set; }
    public string TrainerName { get; set; } = string.Empty;
    public CourseLevel Level { get; set; }
    public decimal Price { get; set; }
    public int Duration { get; set; }
    public string? Thumbnail { get; set; }
    public bool Published { get; set; }
    public int EnrollmentCount { get; set; }
    public int ModuleCount { get; set; }
}

public class CourseUpsertDto
{
    public int Id { get; set; }

    [Required, StringLength(250)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(4000)]
    public string Description { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Please choose a category.")]
    public int CategoryId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please choose a trainer.")]
    public int TrainerId { get; set; }

    public CourseLevel Level { get; set; } = CourseLevel.Beginner;

    [Range(0, 100000, ErrorMessage = "Price must be a positive amount.")]
    public decimal Price { get; set; }

    [Range(1, 10000, ErrorMessage = "Duration must be expressed in minutes.")]
    public int Duration { get; set; }

    public string? Thumbnail { get; set; }

    public bool Published { get; set; }
}

/// <summary>
/// Everything the catalog / admin course-list screens need: free-text search,
/// category & trainer filters, sorting and paging — matches the "SEARCH" and
/// "PAGINATION" requirements in one reusable object.
/// </summary>
public class CourseFilterDto
{
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public int? TrainerId { get; set; }
    public CourseLevel? Level { get; set; }

    /// <summary>Only applied on the public catalog (learners should only ever see published courses).</summary>
    public bool? PublishedOnly { get; set; }

    public CourseSortOption SortBy { get; set; } = CourseSortOption.Newest;

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 9;
}

public enum CourseSortOption
{
    Newest,
    Oldest,
    TitleAsc,
    TitleDesc,
    PriceAsc,
    PriceDesc,
    MostPopular
}
