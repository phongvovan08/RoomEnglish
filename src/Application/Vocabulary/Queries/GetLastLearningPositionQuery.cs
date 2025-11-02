using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Application.Common.Security;

namespace RoomEnglish.Application.Vocabulary.Queries;

/// <summary>
/// Get user's most recent learning position across all words
/// </summary>
[Authorize]
public record GetLastLearningPositionQuery : IRequest<LastLearningPositionDto?>;

public class GetLastLearningPositionQueryHandler : IRequestHandler<GetLastLearningPositionQuery, LastLearningPositionDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetLastLearningPositionQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<LastLearningPositionDto?> Handle(GetLastLearningPositionQuery request, CancellationToken cancellationToken)
    {
        var userId = _user.Id ?? throw new UnauthorizedAccessException();

        var position = await _context.UserLearningPositions
            .Include(p => p.Word)
                .ThenInclude(w => w.Category)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.LastAccessedAt)
            .Select(p => new LastLearningPositionDto
            {
                WordId = p.WordId,
                WordText = p.Word.Word,
                CategoryId = p.Word.CategoryId,
                CategoryName = p.Word.Category.Name,
                GroupIndex = p.GroupIndex,
                LastExampleIndex = p.LastExampleIndex,
                LastAccessedAt = p.LastAccessedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        return position;
    }
}

public record LastLearningPositionDto
{
    public int WordId { get; init; }
    public string WordText { get; init; } = string.Empty;
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public int GroupIndex { get; init; }
    public int LastExampleIndex { get; init; }
    public DateTime LastAccessedAt { get; init; }
}
