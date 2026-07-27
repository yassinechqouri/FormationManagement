using System.ComponentModel.DataAnnotations;

namespace FormationManagement.Application.DTOs.Category;

/// <summary>Read-only shape returned to controllers/views.</summary>
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CourseCount { get; set; }
}

/// <summary>Shape accepted for create/edit; validated with Data Annotations so both the Application service and MVC model binding get the same rules.</summary>
public class CategoryUpsertDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "The category name is required.")]
    [StringLength(150, ErrorMessage = "The name cannot exceed 150 characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "The description cannot exceed 1000 characters.")]
    public string? Description { get; set; }
}
