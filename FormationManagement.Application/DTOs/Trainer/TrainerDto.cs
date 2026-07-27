using System.ComponentModel.DataAnnotations;

namespace FormationManagement.Application.DTOs.Trainer;

public class TrainerDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Biography { get; set; }
    public string? Photo { get; set; }
    public int CourseCount { get; set; }
}

public class TrainerUpsertDto
{
    public int Id { get; set; }

    /// <summary>Only set on create — links the new Trainer profile to an existing Identity account that has the Trainer role.</summary>
    public string? ApplicationUserId { get; set; }

    [Required, StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Phone, StringLength(30)]
    public string? Phone { get; set; }

    [StringLength(2000)]
    public string? Biography { get; set; }

    public string? Photo { get; set; }
}
