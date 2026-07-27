namespace FormationManagement.Application.Interfaces;

public class UserSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IReadOnlyList<string> Roles { get; set; } = new List<string>();
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Thin abstraction over ASP.NET Identity's UserManager, so Application-layer
/// services (e.g. EnrollmentService, DashboardService) can resolve basic user
/// info without the Application project depending on Identity/Infrastructure.
/// Implemented in Infrastructure (see IdentityUserDirectoryService).
/// </summary>
public interface IUserDirectoryService
{
    Task<UserSummaryDto?> GetByIdAsync(string userId);
    Task<IReadOnlyDictionary<string, UserSummaryDto>> GetManyByIdAsync(IEnumerable<string> userIds);
    Task<IReadOnlyList<UserSummaryDto>> GetAllAsync();
    Task<IReadOnlyList<UserSummaryDto>> GetByRoleAsync(string role);
    Task<int> CountAsync();
}
