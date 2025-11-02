using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Application.Common.Models;
using RoomEnglish.Application.Common.Mappings;
using AutoMapper;
using System.Linq;
using RoomEnglish.Application.Vocabulary.Commands.TranslateVietnameseMeaning;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace RoomEnglish.Application.Vocabulary.Queries.GetVocabularyWords;

public record GetVocabularyWordsQuery : IRequest<PaginatedList<VocabularyWordDto>>
{
    public int? CategoryId { get; init; }
    public int? DifficultyLevel { get; init; }
    public string? SearchTerm { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public bool IncludeInactive { get; init; } = false;
    public bool IncludeExamples { get; init; } = false;
    public bool IncludeUserProgress { get; init; } = false;
    public string? UserId { get; init; }
    public string? SortBy { get; init; }
    public string? SortOrder { get; init; } = "asc";
    public bool? AutoTranslate { get; init; } // Auto-translate missing Vietnamese meanings (default: false to save tokens)
}

public class GetVocabularyWordsQueryHandler : IRequestHandler<GetVocabularyWordsQuery, PaginatedList<VocabularyWordDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<GetVocabularyWordsQueryHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GetVocabularyWordsQueryHandler(
        IApplicationDbContext context, 
        IMapper mapper,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<GetVocabularyWordsQueryHandler> logger,
        IHostEnvironment environment)
    {
        _context = context;
        _mapper = mapper;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _environment = environment;
    }

    public async Task<PaginatedList<VocabularyWordDto>> Handle(GetVocabularyWordsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.VocabularyWords
            .Include(x => x.Category)
            .AsQueryable();

        if (request.IncludeExamples)
        {
            query = query.Include(x => x.Examples.Where(e => e.IsActive));
        }

        if (request.IncludeUserProgress && !string.IsNullOrEmpty(request.UserId))
        {
            query = query.Include(x => x.UserProgress.Where(p => p.UserId == request.UserId));
        }

        if (!request.IncludeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == request.CategoryId.Value);
        }

        if (request.DifficultyLevel.HasValue)
        {
            query = query.Where(x => x.DifficultyLevel == request.DifficultyLevel.Value);
        }

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(x => 
                x.Word.Contains(request.SearchTerm) ||
                x.Meaning.Contains(request.SearchTerm) ||
                x.Definition.Contains(request.SearchTerm));
        }

        // Apply sorting
        query = ApplySorting(query, request.SortBy, request.SortOrder);

        var result = await query
            .Select(x => new VocabularyWordDto
            {
                Id = x.Id,
                Word = x.Word,
                Phonetic = x.Phonetic,
                PartOfSpeech = x.PartOfSpeech,
                Meaning = x.Meaning,
                Definition = x.Definition,
                VietnameseMeaning = x.VietnameseMeaning,
                AudioUrl = x.AudioUrl,
                DifficultyLevel = x.DifficultyLevel,
                IsActive = x.IsActive,
                ViewCount = x.ViewCount,
                CorrectCount = x.CorrectCount,
                IncorrectCount = x.IncorrectCount,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name,
                ExampleCount = x.Examples.Count(e => e.IsActive),
                CompletedExampleCount = request.UserId != null 
                    ? x.Examples.Count(e => e.IsActive && e.UserProgress.Any(p => p.UserId == request.UserId && p.IsCompleted))
                    : 0,
                ExampleCompletionPercentage = x.Examples.Count(e => e.IsActive) > 0 && request.UserId != null
                    ? (double)x.Examples.Count(e => e.IsActive && e.UserProgress.Any(p => p.UserId == request.UserId && p.IsCompleted)) 
                      / x.Examples.Count(e => e.IsActive) * 100
                    : 0,
                Examples = x.Examples
                    .Where(e => e.IsActive)
                    .OrderBy(e => e.DisplayOrder)
                    .Select(e => new VocabularyExampleDto
                    {
                        Id = e.Id,
                        Sentence = e.Sentence,
                        Translation = e.Translation,
                        Grammar = e.Grammar,
                        AudioUrl = e.AudioUrl,
                        DifficultyLevel = e.DifficultyLevel,
                        IsActive = e.IsActive,
                        DisplayOrder = e.DisplayOrder,
                        WordId = e.WordId
                    }).ToList(),
                UserProgress = x.UserProgress
                    .Where(p => p.UserId == request.UserId)
                    .Select(p => new UserWordProgressDto
                    {
                        Id = p.Id,
                        UserId = p.UserId,
                        WordId = p.WordId,
                        StudiedTimes = p.StudiedTimes,
                        CorrectAnswers = p.CorrectAnswers,
                        TotalAttempts = p.TotalAttempts,
                        FirstStudiedAt = p.FirstStudiedAt,
                        LastStudiedAt = p.LastStudiedAt,
                        IsMastered = p.IsMastered,
                        MasteryLevel = p.MasteryLevel
                    }).FirstOrDefault()
            })
            .PaginatedListAsync(request.PageNumber, request.PageSize);

        // Background translation for words missing Vietnamese meaning
        // Default: true in Production, false in Development to save tokens
        var shouldAutoTranslate = request.AutoTranslate ?? _environment.IsProduction();
        
        if (shouldAutoTranslate)
        {
            if (!_environment.IsProduction())
            {
                _logger.LogInformation("⚠️ Auto-translate requested but skipped: Not in Production environment (Current: {Environment})", 
                    _environment.EnvironmentName);
            }
            else
            {
                _ = Task.Run(async () => 
                {
                    try
                    {
                        // Create new scope for background task to avoid ObjectDisposedException
                        using var scope = _serviceScopeFactory.CreateScope();
                        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
                        var logger = scope.ServiceProvider.GetRequiredService<ILogger<GetVocabularyWordsQueryHandler>>();

                        var wordsNeedingTranslation = result.Items
                            .Where(w => string.IsNullOrEmpty(w.VietnameseMeaning) && !string.IsNullOrEmpty(w.Definition))
                            .Take(10) // Limit to 10 words per request to avoid overwhelming ChatGPT
                            .ToList();

                        if (wordsNeedingTranslation.Any())
                        {
                            logger.LogInformation("🌐 [Production] Auto-translating {Count} words with missing Vietnamese meanings", 
                                wordsNeedingTranslation.Count);

                            foreach (var word in wordsNeedingTranslation)
                            {
                                var translateCommand = new TranslateVietnameseMeaningCommand
                                {
                                    WordId = word.Id,
                                    TranslateAll = false
                                };

                                await sender.Send(translateCommand);
                            }

                            logger.LogInformation("✅ [Production] Background translation completed for {Count} words", 
                                wordsNeedingTranslation.Count);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "⚠️ Background translation failed but query succeeded");
                    }
                });
            }
        }

        return result;
    }

    private static IQueryable<Domain.Entities.VocabularyWord> ApplySorting(
        IQueryable<Domain.Entities.VocabularyWord> query, 
        string? sortBy, 
        string? sortOrder)
    {
        var isDescending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy?.ToLower() switch
        {
            "word" => isDescending ? query.OrderByDescending(x => x.Word) : query.OrderBy(x => x.Word),
            "definition" => isDescending ? query.OrderByDescending(x => x.Definition) : query.OrderBy(x => x.Definition),
            "pronunciation" => isDescending ? query.OrderByDescending(x => x.Phonetic) : query.OrderBy(x => x.Phonetic),
            "examplecount" => isDescending ? query.OrderByDescending(x => x.Examples.Count(e => e.IsActive)) : query.OrderBy(x => x.Examples.Count(e => e.IsActive)),
            "createdat" => isDescending ? query.OrderByDescending(x => x.Created) : query.OrderBy(x => x.Created),
            _ => query.OrderBy(x => x.Word) // Default sorting by word
        };
    }
}
