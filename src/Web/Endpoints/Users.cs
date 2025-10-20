using Microsoft.AspNetCore.Routing;            // RouteGroupBuilder
using RoomEnglish.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace RoomEnglish.Web.Endpoints;

public sealed class Users : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
       
        group.WithTags("Users");

        
        group.MapIdentityApi<ApplicationUser>();

        group.MapGet("/me", [Authorize] async (HttpContext http, UserManager<ApplicationUser> userManager) =>
        {
            var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return Results.NotFound();

            var roles = await userManager.GetRolesAsync(user);

            return Results.Ok(new
            {
                id = user.Id,
                email = user.Email,
                firstName = "",
                lastName = "",
                displayName = user.UserName ?? user.Email,
                phone = user.PhoneNumber ?? "",
                bio = "",
                avatar = "",
                createdAt = DateTime.UtcNow.ToString("o"),
                lastLoginAt = DateTime.UtcNow.ToString("o"),
                roles = roles.ToArray()
            });
        });
    }
}
