using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Application.Common.Security;

namespace RoomEnglish.Application.Roles.Commands.UpdateRole;

[Authorize(Roles = "Administrator")]
public record UpdateRoleCommand : IRequest<bool>
{
    public string RoleId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, bool>
{
    private readonly IIdentityService _identityService;

    public UpdateRoleCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<bool> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.UpdateRoleAsync(request.RoleId, request.Name, request.Description);
        return result.Succeeded;
    }
}
