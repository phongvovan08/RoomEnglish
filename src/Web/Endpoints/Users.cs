using Microsoft.AspNetCore.Routing;            // RouteGroupBuilder
using RoomEnglish.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using RoomEnglish.Application.Users.Commands.CreateUser;
using RoomEnglish.Application.Users.Commands.DeleteUser;
using RoomEnglish.Application.Users.Commands.UpdateUserRoles;
using RoomEnglish.Application.Users.Queries.GetAvailableRoles;
using RoomEnglish.Application.Users.Queries.GetUsers;

namespace RoomEnglish.Web.Endpoints;

public class Users : EndpointGroupBase
{
    // DTOs for requests
    public record CreateUserRequest(string Email, string Password, string? UserName, IList<string>? Roles);
    public record UpdateRolesRequest(string UserId, IList<string> Roles);
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
                displayName = user.DisplayName ?? user.UserName ?? user.Email,
                phone = user.PhoneNumber ?? "",
                bio = "",
                avatar = "",
                createdAt = DateTime.UtcNow.ToString("o"),
                lastLoginAt = DateTime.UtcNow.ToString("o"),
                roles = roles.ToArray()
            });
        });

        group.MapGet("/list", [Authorize(Roles = "Administrator")] async (
            ISender sender,
            [AsParameters] GetUsersQuery query) =>
        {
            return Results.Ok(await sender.Send(query));
        });

        group.MapGet("/roles", [Authorize(Roles = "Administrator")] async (ISender sender) =>
        {
            return Results.Ok(await sender.Send(new GetAvailableRolesQuery()));
        });

        // POST /api/users - Create new user
        group.MapPost("/", [Authorize(Roles = "Administrator")] async (
            ISender sender,
            CreateUserRequest request) =>
        {
            try
            {
                var command = new CreateUserCommand
                {
                    Email = request.Email,
                    Password = request.Password,
                    UserName = request.UserName,
                    Roles = request.Roles ?? new List<string>()
                };

                var userId = await sender.Send(command);
                return Results.Ok(new { success = true, userId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating user: {ex.Message}");
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: 500,
                    title: "Error creating user"
                );
            }
        });

        group.MapPut("/{userId}/roles", [Authorize(Roles = "Administrator")] async (
            ISender sender,
            string userId,
            UpdateRolesRequest request) =>
        {
            try
            {
                if (userId != request.UserId)
                {
                    return Results.BadRequest(new { error = "User ID mismatch" });
                }

                var command = new UpdateUserRolesCommand
                {
                    UserId = userId,
                    Roles = request.Roles ?? new List<string>()
                };

                var result = await sender.Send(command);
                return result ? Results.Ok(new { success = true }) : Results.BadRequest(new { error = "Failed to update roles" });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"❌ Error updating user roles for {userId}");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: 500,
                    title: "Error updating user roles"
                );
            }
        });

        // DELETE /api/users/{userId} - Delete user
        group.MapDelete("/{userId}", [Authorize(Roles = "Administrator")] async (
            ISender sender,
            string userId) =>
        {
            try
            {
                var command = new DeleteUserCommand { UserId = userId };
                var result = await sender.Send(command);
                
                return result
                    ? Results.Ok(new { success = true })
                    : Results.BadRequest(new { error = "Failed to delete user" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error deleting user {userId}: {ex.Message}");
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: 500,
                    title: "Error deleting user"
                );
            }
        });
    }
}
