using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Application.Common.Security;

namespace RoomEnglish.Application.Roles.Queries.GetRoles;

[Authorize(Roles = "Administrator")]
public record GetRolesQuery : IRequest<RolesVm>
{
    public string? SearchTerm { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, RolesVm>
{
    private readonly IIdentityService _identityService;

    public GetRolesQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<RolesVm> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _identityService.GetRolesWithDetailsAsync(
            request.SearchTerm,
            request.PageNumber,
            request.PageSize
        );

        return new RolesVm
        {
            Roles = roles.Items,
            TotalCount = roles.TotalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

public class RolesVm
{
    public IList<RoleDetailDto> Roles { get; init; } = new List<RoleDetailDto>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}

public class RoleDetailDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? NormalizedName { get; init; }
    public string Description { get; init; } = string.Empty;
    public int UserCount { get; init; }
    public DateTime? CreatedAt { get; init; }
}
