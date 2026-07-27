using System.Linq;
using FormationManagement.Application.Interfaces;
using FormationManagement.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace FormationManagement.Infrastructure.Services;

public class IdentityUserDirectoryService : IUserDirectoryService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityUserDirectoryService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserSummaryDto?> GetByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user is null ? null : await ToDtoAsync(user);
    }

    public async Task<IReadOnlyDictionary<string, UserSummaryDto>> GetManyByIdAsync(IEnumerable<string> userIds)
    {
        var result = new Dictionary<string, UserSummaryDto>();
        foreach (var id in userIds.Distinct())
        {
            var dto = await GetByIdAsync(id);
            if (dto != null) result[id] = dto;
        }
        return result;
    }

    public async Task<IReadOnlyList<UserSummaryDto>> GetAllAsync()
    {
        var users = _userManager.Users.ToList();
        var result = new List<UserSummaryDto>();
        foreach (var user in users)
            result.Add(await ToDtoAsync(user));
        return result;
    }

    public async Task<IReadOnlyList<UserSummaryDto>> GetByRoleAsync(string role)
    {
        var users = await _userManager.GetUsersInRoleAsync(role);
        var result = new List<UserSummaryDto>();
        foreach (var user in users)
            result.Add(await ToDtoAsync(user));
        return result;
    }

    public Task<int> CountAsync() => Task.FromResult(_userManager.Users.Count());

    private async Task<UserSummaryDto> ToDtoAsync(ApplicationUser user) => new()
    {
        Id = user.Id,
        FullName = string.IsNullOrWhiteSpace(user.FullName) ? user.Email ?? user.UserName ?? "Unknown" : user.FullName,
        Email = user.Email ?? string.Empty,
        Roles = (await _userManager.GetRolesAsync(user)).ToList(),
        CreatedAt = user.CreatedAt
    };
}
