using RoomEnglish.Application.Common.Interfaces;

namespace RoomEnglish.Application.Vocabulary.Commands;

/// <summary>
/// Clears user's saved learning position for a specific word
/// Used when user completes all examples or manually resets
/// </summary>
public record ClearLearningPositionCommand : IRequest<Unit>
{
    public int WordId { get; init; }
}

public class ClearLearningPositionCommandHandler : IRequestHandler<ClearLearningPositionCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public ClearLearningPositionCommandHandler(
        IApplicationDbContext context,
        IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<Unit> Handle(ClearLearningPositionCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.Id ?? throw new UnauthorizedAccessException();

        var position = await _context.UserLearningPositions
            .FirstOrDefaultAsync(p => 
                p.UserId == userId && 
                p.WordId == request.WordId, 
                cancellationToken);

        if (position != null)
        {
            _context.UserLearningPositions.Remove(position);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
