using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Domain.Entities;

namespace RoomEnglish.Application.Vocabulary.Commands;

/// <summary>
/// Updates or creates user's learning position for a specific word and group
/// </summary>
public record SaveLearningPositionCommand : IRequest<Unit>
{
    public int WordId { get; init; }
    public int GroupIndex { get; init; }
    public int LastExampleIndex { get; init; }
}

public class SaveLearningPositionCommandHandler : IRequestHandler<SaveLearningPositionCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public SaveLearningPositionCommandHandler(
        IApplicationDbContext context,
        IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<Unit> Handle(SaveLearningPositionCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.Id ?? throw new UnauthorizedAccessException();

        var position = await _context.UserLearningPositions
            .FirstOrDefaultAsync(p => 
                p.UserId == userId && 
                p.WordId == request.WordId, 
                cancellationToken);

        if (position == null)
        {
            // Create new position
            position = new UserLearningPosition
            {
                UserId = userId,
                WordId = request.WordId,
                GroupIndex = request.GroupIndex,
                LastExampleIndex = request.LastExampleIndex,
                LastAccessedAt = DateTime.UtcNow
            };
            _context.UserLearningPositions.Add(position);
        }
        else
        {
            // Update existing position
            position.GroupIndex = request.GroupIndex;
            position.LastExampleIndex = request.LastExampleIndex;
            position.LastAccessedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
