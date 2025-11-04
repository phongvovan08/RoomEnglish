using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Application.Common.Security;

namespace RoomEnglish.Application.Users.Commands.CreateUser;

[Authorize(Roles = "Administrator")]
public record CreateUserCommand : IRequest<string>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string? UserName { get; init; }
    public IList<string> Roles { get; init; } = new List<string>();
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, string>
{
    private readonly IIdentityService _identityService;

    public CreateUserCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<string> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Create user with email as username
        var (result, userId) = await _identityService.CreateUserAsync(
            request.UserName ?? request.Email, 
            request.Password
        );
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors);
            throw new Exception($"Failed to create user: {errors}");
        }

        // Assign roles if provided
        if (request.Roles != null && request.Roles.Count > 0)
        {
            var roleResult = await _identityService.UpdateUserRolesAsync(userId, request.Roles);
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors);
                throw new Exception($"User created but failed to assign roles: {errors}");
            }
        }

        return userId;
    }
}
