using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Application.Common.Security;

namespace RoomEnglish.Application.Vocabulary.Queries.GetReviewExamples;

[Authorize]
public record GetReviewExamplesQuery : IRequest<ReviewExamplesDto>
{
    public int Count { get; init; } = 20;
}

public class GetReviewExamplesQueryHandler : IRequestHandler<GetReviewExamplesQuery, ReviewExamplesDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetReviewExamplesQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<ReviewExamplesDto> Handle(GetReviewExamplesQuery request, CancellationToken cancellationToken)
    {
        var userId = _user.Id ?? throw new UnauthorizedAccessException();
        
        var studiedExamples = await _context.UserExampleProgress
            .Where(p => p.UserId == userId && p.LastAttemptedAt != null)
            .Include(p => p.Example)
                .ThenInclude(e => e.Word)
                .ThenInclude(w => w.Category)
            .Where(p => p.Example.IsActive && p.Example.Word.IsActive)
            .OrderBy(p => Guid.NewGuid()) // Random order
            .Take(request.Count)
            .ToListAsync(cancellationToken);

        var reviewItems = new List<ReviewExampleItemDto>();
        var processedWords = new HashSet<int>();

        foreach (var exampleProgress in studiedExamples)
        {
            var example = exampleProgress.Example;
            var word = example.Word;

            // Skip if we already have an example from this word (one example per word)
            if (processedWords.Contains(word.Id))
                continue;

            processedWords.Add(word.Id);

            // Get word progress
            var wordProgress = await _context.UserWordProgress
                .Where(p => p.UserId == userId && p.WordId == word.Id)
                .FirstOrDefaultAsync(cancellationToken);

            reviewItems.Add(new ReviewExampleItemDto
            {
                ExampleId = example.Id,
                WordId = word.Id,
                Word = word.Word,
                Phonetic = word.Phonetic,
                PartOfSpeech = word.PartOfSpeech,
                Meaning = word.Meaning,
                VietnameseMeaning = word.VietnameseMeaning,
                CategoryName = word.Category.Name,
                Sentence = example.Sentence,
                Translation = example.Translation,
                AudioUrl = example.AudioUrl,
                DifficultyLevel = example.DifficultyLevel,
                IsMastered = wordProgress?.IsMastered ?? false,
                StudiedTimes = wordProgress?.StudiedTimes ?? 0
            });
        }

        return new ReviewExamplesDto
        {
            Items = reviewItems,
            TotalCount = reviewItems.Count
        };
    }
}

public record ReviewExamplesDto
{
    public List<ReviewExampleItemDto> Items { get; init; } = new();
    public int TotalCount { get; init; }
}

public record ReviewExampleItemDto
{
    public int ExampleId { get; init; }
    public int WordId { get; init; }
    public string Word { get; init; } = string.Empty;
    public string? Phonetic { get; init; }
    public string? PartOfSpeech { get; init; }
    public string Meaning { get; init; } = string.Empty;
    public string? VietnameseMeaning { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string Sentence { get; init; } = string.Empty;
    public string? Translation { get; init; }
    public string? AudioUrl { get; init; }
    public int DifficultyLevel { get; init; }
    public bool IsMastered { get; init; }
    public int StudiedTimes { get; init; }
}
