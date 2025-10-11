using Microsoft.AspNetCore.Routing;            // RouteGroupBuilder
using RoomEnglish.Infrastructure.Identity;

namespace RoomEnglish.Web.Endpoints;

public sealed class Users : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
       
        group.WithTags("Users");

        
        group.MapIdentityApi<ApplicationUser>();
    }
}
