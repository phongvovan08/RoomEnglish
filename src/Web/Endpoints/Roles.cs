using Microsoft.AspNetCore.Authorization;
using RoomEnglish.Application.Roles.Commands.CreateRole;
using RoomEnglish.Application.Roles.Commands.UpdateRole;
using RoomEnglish.Application.Roles.Commands.DeleteRole;
using RoomEnglish.Application.Roles.Queries.GetRoles;
using RoomEnglish.Domain.Constants;

namespace RoomEnglish.Web.Endpoints;

public class Roles : EndpointGroupBase
{
    // DTOs for requests
    public record CreateRoleRequest(string Name, string Description);
    public record UpdateRoleRequest(string RoleId, string Name, string Description);

    public override void Map(RouteGroupBuilder group)
    {
        group.WithTags("Roles");

        // GET /api/roles/list - Get paginated roles with search
        group.MapGet("/list", [Authorize(Roles = RoomEnglish.Domain.Constants.Roles.Administrator)] async (
            ISender sender,
            [AsParameters] GetRolesQuery query) =>
        {
            return Results.Ok(await sender.Send(query));
        });

        // POST /api/roles - Create new role
        group.MapPost("/", [Authorize(Roles = RoomEnglish.Domain.Constants.Roles.Administrator)] async (
            ISender sender,
            CreateRoleRequest request) =>
        {
            try
            {
                var command = new CreateRoleCommand
                {
                    Name = request.Name,
                    Description = request.Description
                };

                var roleId = await sender.Send(command);
                return Results.Ok(new { success = true, roleId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating role: {ex.Message}");
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: 500,
                    title: "Error creating role"
                );
            }
        });

        // PUT /api/roles/{roleId} - Update role
        group.MapPut("/{roleId}", [Authorize(Roles = RoomEnglish.Domain.Constants.Roles.Administrator)] async (
            ISender sender,
            string roleId,
            UpdateRoleRequest request) =>
        {
            try
            {
                if (roleId != request.RoleId)
                {
                    return Results.BadRequest(new { error = "Role ID mismatch" });
                }

                var command = new UpdateRoleCommand
                {
                    RoleId = roleId,
                    Name = request.Name,
                    Description = request.Description
                };

                var result = await sender.Send(command);
                return result
                    ? Results.Ok(new { success = true })
                    : Results.BadRequest(new { error = "Failed to update role" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error updating role {roleId}: {ex.Message}");
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: 500,
                    title: "Error updating role"
                );
            }
        });

        // DELETE /api/roles/{roleId} - Delete role
        group.MapDelete("/{roleId}", [Authorize(Roles = RoomEnglish.Domain.Constants.Roles.Administrator)] async (
            ISender sender,
            string roleId) =>
        {
            try
            {
                var command = new DeleteRoleCommand { RoleId = roleId };
                var result = await sender.Send(command);
                
                return result
                    ? Results.Ok(new { success = true })
                    : Results.BadRequest(new { error = "Failed to delete role" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error deleting role {roleId}: {ex.Message}");
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: 500,
                    title: "Error deleting role"
                );
            }
        });
    }
}
