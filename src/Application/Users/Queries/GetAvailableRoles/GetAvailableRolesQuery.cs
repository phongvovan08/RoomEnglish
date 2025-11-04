using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Application.Common.Security;

namespace RoomEnglish.Application.Users.Queries.GetAvailableRoles;

[Authorize(Roles = "Administrator")]
public record GetAvailableRolesQuery : IRequest<AvailableRolesVm>;

public class GetAvailableRolesQueryHandler : IRequestHandler<GetAvailableRolesQuery, AvailableRolesVm>
{
    private readonly IIdentityService _identityService;

    public GetAvailableRolesQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<AvailableRolesVm> Handle(GetAvailableRolesQuery request, CancellationToken cancellationToken)
    {
        // Get all roles from database via IIdentityService
        var roles = await _identityService.GetAllRolesAsync();

        return new AvailableRolesVm { Roles = roles };
    }
}

public class AvailableRolesVm
{
    public IList<RoleDto> Roles { get; init; } = new List<RoleDto>();
}
