using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Application.Common.Security;

namespace RoomEnglish.Application.Users.Queries.GetUsers;

[Authorize(Roles = "Administrator")]
public record GetUsersQuery : IRequest<UsersVm>
{
    public string? SearchTerm { get; init; }
    public string? Role { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, UsersVm>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public GetUsersQueryHandler(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<UsersVm> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _identityService.GetUsersAsync(
            request.SearchTerm,
            request.Role,
            request.PageNumber,
            request.PageSize
        );

        return new UsersVm
        {
            Users = users.Items,
            TotalCount = users.TotalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

public class UsersVm
{
    public IList<UserDto> Users { get; init; } = new List<UserDto>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}

public class UserDto
{
    public string Id { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? UserName { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public bool EmailConfirmed { get; init; }
    public IList<string> Roles { get; init; } = new List<string>();
    public DateTime? CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
}
