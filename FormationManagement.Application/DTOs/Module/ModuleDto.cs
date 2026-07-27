using System.ComponentModel.DataAnnotations;

namespace FormationManagement.Application.DTOs.Module;

public class ModuleDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
    public List<LessonDto> Lessons { get; set; } = new();
}

public class ModuleUpsertDto
{
    public int Id { get; set; }

    [Required]
    public int CourseId { get; set; }

    [Required, StringLength(250)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Range(1, 1000)]
    public int Order { get; set; } = 1;
}

public class LessonDto
{
    public int Id { get; set; }
    public int ModuleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }
    public string? DocumentUrl { get; set; }
    public int Duration { get; set; }
    public int ExerciseCount { get; set; }
    public int QuizCount { get; set; }
}

public class LessonUpsertDto
{
    public int Id { get; set; }

    [Required]
    public int ModuleId { get; set; }

    [Required, StringLength(250)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? VideoUrl { get; set; }

    [StringLength(1000)]
    public string? DocumentUrl { get; set; }

    [Range(1, 1000)]
    public int Duration { get; set; } = 10;
}
