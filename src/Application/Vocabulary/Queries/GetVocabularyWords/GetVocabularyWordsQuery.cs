using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Application.Common.Models;
using RoomEnglish.Application.Common.Mappings;
using AutoMapper;

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
}

public class GetVocabularyWordsQueryHandler : IRequestHandler<GetVocabularyWordsQuery, PaginatedList<VocabularyWordDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetVocabularyWordsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
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

        return await query
            .OrderBy(x => x.Word)
            .Select(x => new VocabularyWordDto
            {
                Id = x.Id,
                Word = x.Word,
                Phonetic = x.Phonetic,
                PartOfSpeech = x.PartOfSpeech,
                Meaning = x.Meaning,
                Definition = x.Definition,
                AudioUrl = x.AudioUrl,
                DifficultyLevel = x.DifficultyLevel,
                IsActive = x.IsActive,
                ViewCount = x.ViewCount,
                CorrectCount = x.CorrectCount,
                IncorrectCount = x.IncorrectCount,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name,
                Examples = x.Examples
                    .Where(e => e.IsActive)
                    .OrderBy(e => e.DisplayOrder)
                    .Select(e => new VocabularyExampleDto
                    {
                        Id = e.Id,
                        Sentence = e.Sentence,
                        Translation = e.Translation,
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
    }
}