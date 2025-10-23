using RoomEnglish.Application.Common.Interfaces;

namespace RoomEnglish.Application.Vocabulary.Queries;

public class LearningPositionDto
{
    public int WordId { get; set; }
    public int GroupIndex { get; set; }
    public int LastExampleIndex { get; set; }
    public DateTime LastAccessedAt { get; set; }
}

/// <summary>
/// Gets user's saved learning position for a specific word
/// </summary>
public record GetLearningPositionQuery : IRequest<LearningPositionDto?>
{
    public int WordId { get; init; }
}

public class GetLearningPositionQueryHandler : IRequestHandler<GetLearningPositionQuery, LearningPositionDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetLearningPositionQueryHandler(
        IApplicationDbContext context,
        IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<LearningPositionDto?> Handle(GetLearningPositionQuery request, CancellationToken cancellationToken)
    {
        var userId = _user.Id;
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        var position = await _context.UserLearningPositions
            .Where(p => p.UserId == userId && p.WordId == request.WordId)
            .Select(p => new LearningPositionDto
            {
                WordId = p.WordId,
                GroupIndex = p.GroupIndex,
                LastExampleIndex = p.LastExampleIndex,
                LastAccessedAt = p.LastAccessedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        return position;
    }
}
