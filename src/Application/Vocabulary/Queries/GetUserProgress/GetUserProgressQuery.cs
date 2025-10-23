using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomEnglish.Application.Common.Interfaces;

namespace RoomEnglish.Application.Vocabulary.Queries.GetUserProgress;

public record GetUserProgressQuery : IRequest<UserProgressDto>
{
    public string? UserId { get; init; }
    public int? CategoryId { get; init; }
}

public class GetUserProgressQueryHandler : IRequestHandler<GetUserProgressQuery, UserProgressDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetUserProgressQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<UserProgressDto> Handle(GetUserProgressQuery request, CancellationToken cancellationToken)
    {
        var userId = request.UserId ?? _user.Id ?? throw new UnauthorizedAccessException();

        var categoryProgress = await _context.UserCategoryProgress
            .Where(p => p.UserId == userId)
            .Where(p => !request.CategoryId.HasValue || p.CategoryId == request.CategoryId.Value)
            .Select(p => new UserCategoryProgressDto
            {
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                CompletionPercentage = p.CompletionPercentage,
                WordsStudied = p.WordsStudied,
                WordsMastered = p.WordsMastered,
                AverageAccuracy = p.AverageAccuracy,
                LastStudiedAt = p.LastStudiedAt
            })
            .ToListAsync(cancellationToken);

        var wordProgress = await _context.UserWordProgress
            .Where(p => p.UserId == userId)
            .Where(p => !request.CategoryId.HasValue || p.Word.CategoryId == request.CategoryId.Value)
            .Select(p => new UserWordProgressDto
            {
                WordId = p.WordId,
                Word = p.Word.Word,
                StudiedTimes = p.StudiedTimes,
                CorrectAnswers = p.CorrectAnswers,
                TotalAttempts = p.TotalAttempts,
                IsMastered = p.IsMastered,
                MasteryLevel = p.MasteryLevel,
                LastStudiedAt = p.LastStudiedAt
            })
            .ToListAsync(cancellationToken);

        var exampleProgress = await _context.UserExampleProgress
            .Where(p => p.UserId == userId)
            .Where(p => !request.CategoryId.HasValue || p.Example.Word.CategoryId == request.CategoryId.Value)
            .Select(p => new UserExampleProgressDto
            {
                ExampleId = p.ExampleId,
                WordId = p.Example.WordId,
                IsCompleted = p.IsCompleted,
                TotalAttempts = p.TotalAttempts,
                BestAccuracy = p.BestAccuracy,
                LastAttemptedAt = p.LastAttemptedAt
            })
            .ToListAsync(cancellationToken);

        return new UserProgressDto
        {
            UserId = userId,
            CategoryProgress = categoryProgress,
            WordProgress = wordProgress,
            ExampleProgress = exampleProgress
        };
    }
}

public class UserProgressDto
{
    public string UserId { get; init; } = string.Empty;
    public IList<UserCategoryProgressDto> CategoryProgress { get; init; } = new List<UserCategoryProgressDto>();
    public IList<UserWordProgressDto> WordProgress { get; init; } = new List<UserWordProgressDto>();
    public IList<UserExampleProgressDto> ExampleProgress { get; init; } = new List<UserExampleProgressDto>();
}

public class UserCategoryProgressDto
{
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public int CompletionPercentage { get; init; }
    public int WordsStudied { get; init; }
    public int WordsMastered { get; init; }
    public double AverageAccuracy { get; init; }
    public DateTime? LastStudiedAt { get; init; }
}

public class UserWordProgressDto
{
    public int WordId { get; init; }
    public string Word { get; init; } = string.Empty;
    public int StudiedTimes { get; init; }
    public int CorrectAnswers { get; init; }
    public int TotalAttempts { get; init; }
    public bool IsMastered { get; init; }
    public int MasteryLevel { get; init; }
    public double AccuracyRate { get; init; }
    public DateTime? LastStudiedAt { get; init; }
}

public class UserExampleProgressDto
{
    public int ExampleId { get; init; }
    public int WordId { get; init; }
    public bool IsCompleted { get; init; }
    public int TotalAttempts { get; init; }
    public int BestAccuracy { get; init; }
    public DateTime? LastAttemptedAt { get; init; }
}
