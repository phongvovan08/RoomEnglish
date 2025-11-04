using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Application.Common.Security;

namespace RoomEnglish.Application.Roles.Commands.DeleteRole;

[Authorize(Roles = "Administrator")]
public record DeleteRoleCommand : IRequest<bool>
{
    public string RoleId { get; init; } = string.Empty;
}

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, bool>
{
    private readonly IIdentityService _identityService;

    public DeleteRoleCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<bool> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.DeleteRoleAsync(request.RoleId);
        return result.Succeeded;
    }
}
