using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Application.Common.Models;
using RoomEnglish.Application.Users.Queries.GetUsers;
using RoomEnglish.Application.Roles.Queries.GetRoles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace RoomEnglish.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password)
    {
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = userName,
        };

        var result = await _userManager.CreateAsync(user, password);

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

        var result = await _authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<Result> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null ? await DeleteUserAsync(user) : Result.Success();
    }

    public async Task<Result> DeleteUserAsync(ApplicationUser user)
    {
        var result = await _userManager.DeleteAsync(user);

        return result.ToApplicationResult();
    }

    public async Task<(IList<UserDto> Items, int TotalCount)> GetUsersAsync(
        string? searchTerm,
        string? role,
        int pageNumber,
        int pageSize)
    {
        var query = _userManager.Users.AsQueryable();

        // Search by email or username
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(u => 
                u.Email!.Contains(searchTerm) || 
                u.UserName!.Contains(searchTerm));
        }

        // Filter by role
        if (!string.IsNullOrWhiteSpace(role))
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role);
            var userIds = usersInRole.Select(u => u.Id).ToHashSet();
            query = query.Where(u => userIds.Contains(u.Id));
        }

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderBy(u => u.Email)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var userDtos = new List<UserDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName,
                EmailConfirmed = user.EmailConfirmed,
                Roles = roles.ToList()
            });
        }

        return (userDtos, totalCount);
    }

    public async Task<Result> UpdateUserRolesAsync(string userId, IList<string> roles)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Result.Failure(new[] { "User not found" });
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        
        // Remove old roles
        if (currentRoles.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                return removeResult.ToApplicationResult();
            }
        }

        // Add new roles (only if there are roles to add)
        if (roles != null && roles.Count > 0)
        {
            var addResult = await _userManager.AddToRolesAsync(user, roles);
            return addResult.ToApplicationResult();
        }

        return Result.Success();
    }

    public async Task<IList<RoleDto>> GetAllRolesAsync()
    {
        // Role descriptions mapping
        var roleDescriptions = new Dictionary<string, string>
        {
            { "Administrator", "Toàn quyền quản trị hệ thống" },
            { "User", "Người dùng thông thường" },
            { "Moderator", "Người kiểm duyệt nội dung" },
            { "Teacher", "Giáo viên, quản lý khóa học" },
            { "Student", "Học viên" }
        };

        // Get all roles from AspNetRoles table
        var dbRoles = await _roleManager.Roles
            .OrderBy(r => r.Name)
            .ToListAsync();

        var roles = dbRoles.Select(r => new RoleDto
        {
            Name = r.Name ?? string.Empty,
            Description = roleDescriptions.TryGetValue(r.Name ?? string.Empty, out var desc)
                ? desc
                : $"Vai trò {r.Name}"
        }).ToList();

        return roles;
    }

    public async Task<(IList<RoleDetailDto> Items, int TotalCount)> GetRolesWithDetailsAsync(
        string? searchTerm,
        int pageNumber,
        int pageSize)
    {
        // Role descriptions mapping
        var roleDescriptions = new Dictionary<string, string>
        {
            { "Administrator", "Toàn quyền quản trị hệ thống" },
            { "User", "Người dùng thông thường" },
            { "Moderator", "Người kiểm duyệt nội dung" },
            { "Teacher", "Giáo viên, quản lý khóa học" },
            { "Student", "Học viên" }
        };

        var query = _roleManager.Roles.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(r => r.Name!.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync();

        // Get roles for current page
        var roles = await query
            .OrderBy(r => r.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Get user counts for each role
        var roleDetails = new List<RoleDetailDto>();
        foreach (var role in roles)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            roleDetails.Add(new RoleDetailDto
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty,
                NormalizedName = role.NormalizedName,
                Description = roleDescriptions.TryGetValue(role.Name ?? string.Empty, out var desc)
                    ? desc
                    : $"Vai trò {role.Name}",
                UserCount = usersInRole.Count,
                CreatedAt = null // IdentityRole doesn't have CreatedAt by default
            });
        }

        return (roleDetails, totalCount);
    }

    public async Task<(Result Result, string? RoleId)> CreateRoleAsync(string roleName, string description)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return (Result.Failure(new[] { "Role name is required" }), null);
        }

        // Check if role already exists
        var existingRole = await _roleManager.FindByNameAsync(roleName);
        if (existingRole != null)
        {
            return (Result.Failure(new[] { $"Role '{roleName}' already exists" }), null);
        }

        var role = new IdentityRole(roleName);
        var result = await _roleManager.CreateAsync(role);

        if (!result.Succeeded)
        {
            return (result.ToApplicationResult(), null);
        }

        return (Result.Success(), role.Id);
    }

    public async Task<Result> UpdateRoleAsync(string roleId, string roleName, string description)
    {
        if (string.IsNullOrWhiteSpace(roleId))
        {
            return Result.Failure(new[] { "Role ID is required" });
        }

        if (string.IsNullOrWhiteSpace(roleName))
        {
            return Result.Failure(new[] { "Role name is required" });
        }

        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
        {
            return Result.Failure(new[] { "Role not found" });
        }

        // Check if new name conflicts with existing role (excluding current role)
        var existingRole = await _roleManager.FindByNameAsync(roleName);
        if (existingRole != null && existingRole.Id != roleId)
        {
            return Result.Failure(new[] { $"Role '{roleName}' already exists" });
        }

        role.Name = roleName;
        var result = await _roleManager.UpdateAsync(role);

        return result.ToApplicationResult();
    }

    public async Task<Result> DeleteRoleAsync(string roleId)
    {
        if (string.IsNullOrWhiteSpace(roleId))
        {
            return Result.Failure(new[] { "Role ID is required" });
        }

        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
        {
            return Result.Failure(new[] { "Role not found" });
        }

        // Check if any users have this role
        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
        if (usersInRole.Count > 0)
        {
            return Result.Failure(new[] { $"Cannot delete role '{role.Name}' because it is assigned to {usersInRole.Count} user(s)" });
        }

        var result = await _roleManager.DeleteAsync(role);
        return result.ToApplicationResult();
    }
}
