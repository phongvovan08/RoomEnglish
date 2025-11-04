using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Application.Common.Security;

namespace RoomEnglish.Application.Users.Commands.DeleteUser;

[Authorize(Roles = "Administrator")]
public record DeleteUserCommand : IRequest<bool>
{
    public string UserId { get; init; } = string.Empty;
}

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
{
    private readonly IIdentityService _identityService;

    public DeleteUserCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.DeleteUserAsync(request.UserId);
        return result.Succeeded;
    }
}
