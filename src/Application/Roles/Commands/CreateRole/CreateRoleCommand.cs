using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Application.Common.Security;

namespace RoomEnglish.Application.Roles.Commands.CreateRole;

[Authorize(Roles = "Administrator")]
public record CreateRoleCommand : IRequest<string>
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, string>
{
    private readonly IIdentityService _identityService;

    public CreateRoleCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<string> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var (result, roleId) = await _identityService.CreateRoleAsync(request.Name, request.Description);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors);
            throw new Exception($"Failed to create role: {errors}");
        }

        return roleId ?? string.Empty;
    }
}
