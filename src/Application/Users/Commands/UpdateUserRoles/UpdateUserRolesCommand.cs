using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Application.Common.Security;

namespace RoomEnglish.Application.Users.Commands.UpdateUserRoles;

[Authorize(Roles = "Administrator")]
public record UpdateUserRolesCommand : IRequest<bool>
{
    public string UserId { get; init; } = string.Empty;
    public IList<string> Roles { get; init; } = new List<string>();
}

public class UpdateUserRolesCommandHandler : IRequestHandler<UpdateUserRolesCommand, bool>
{
    private readonly IIdentityService _identityService;

    public UpdateUserRolesCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<bool> Handle(UpdateUserRolesCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.UpdateUserRolesAsync(request.UserId, request.Roles);
        return result.Succeeded;
    }
}
