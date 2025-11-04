using RoomEnglish.Application.Common.Models;
using RoomEnglish.Application.Users.Queries.GetUsers;
using RoomEnglish.Application.Roles.Queries.GetRoles;

namespace RoomEnglish.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);

    Task<Result> DeleteUserAsync(string userId);

    Task<(IList<UserDto> Items, int TotalCount)> GetUsersAsync(
        string? searchTerm,
        string? role,
        int pageNumber,
        int pageSize);

    Task<Result> UpdateUserRolesAsync(string userId, IList<string> roles);

    Task<IList<RoleDto>> GetAllRolesAsync();

    // Role Management Methods
    Task<(IList<RoleDetailDto> Items, int TotalCount)> GetRolesWithDetailsAsync(
        string? searchTerm,
        int pageNumber,
        int pageSize);

    Task<(Result Result, string? RoleId)> CreateRoleAsync(string roleName, string description);

    Task<Result> UpdateRoleAsync(string roleId, string roleName, string description);

    Task<Result> DeleteRoleAsync(string roleId);
}

public class RoleDto
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
