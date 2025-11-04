using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Application.Common.Security;

namespace RoomEnglish.Application.TodoLists.Commands.PurgeTodoLists;

[Authorize(Roles = RoomEnglish.Domain.Constants.Roles.Administrator)]
[Authorize(Policy = RoomEnglish.Domain.Constants.Policies.CanPurge)]
public record PurgeTodoListsCommand : IRequest;

public class PurgeTodoListsCommandHandler : IRequestHandler<PurgeTodoListsCommand>
{
    private readonly IApplicationDbContext _context;

    public PurgeTodoListsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(PurgeTodoListsCommand request, CancellationToken cancellationToken)
    {
        _context.TodoLists.RemoveRange(_context.TodoLists);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
