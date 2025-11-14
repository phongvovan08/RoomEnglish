using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Application.Common.Models;
using RoomEnglish.Application.Common.Mappings;
using RoomEnglish.Application.Vocabulary.Queries;

namespace RoomEnglish.Application.Vocabulary.Queries.GetVocabularyExamples;


public record GetVocabularyExamplesQuery : IRequest<PaginatedList<VocabularyExampleDto>>
{
    public int? VocabularyId { get; init; }
    public string? SearchTerm { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public bool IncludeInactive { get; init; } = false;
    public string? SortBy { get; init; }
    public string? SortOrder { get; init; } = "asc";
}

public class GetVocabularyExamplesQueryHandler : IRequestHandler<GetVocabularyExamplesQuery, PaginatedList<VocabularyExampleDto>>
{
    private readonly IApplicationDbContext _context;

    public GetVocabularyExamplesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<VocabularyExampleDto>> Handle(GetVocabularyExamplesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.VocabularyExamples
            .Include(x => x.Word)
            .AsQueryable();

        // Apply filters
        if (!request.IncludeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        if (request.VocabularyId.HasValue)
        {
            query = query.Where(x => x.WordId == request.VocabularyId.Value);
        }

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(x => 
                x.Sentence.Contains(request.SearchTerm) ||
                x.Translation.Contains(request.SearchTerm) ||
                (x.Grammar != null && x.Grammar.Contains(request.SearchTerm)));
        }

        // Apply sorting
        query = ApplySorting(query, request.SortBy, request.SortOrder);

        return await query
            .Select(x => new VocabularyExampleDto
            {
                Id = x.Id,
                Sentence = x.Sentence,
                Phonetic = x.Phonetic,
                Translation = x.Translation,
                Grammar = x.Grammar,
                AudioUrl = x.AudioUrl,
                DifficultyLevel = x.DifficultyLevel,
                IsActive = x.IsActive,
                DisplayOrder = x.DisplayOrder,
                WordId = x.WordId
            })
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }

    private static IQueryable<Domain.Entities.VocabularyExample> ApplySorting(
        IQueryable<Domain.Entities.VocabularyExample> query, 
        string? sortBy, 
        string? sortOrder)
    {
        if (string.IsNullOrEmpty(sortBy))
        {
            return query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id);
        }

        var isDescending = sortOrder?.ToLower() == "desc";

        return sortBy.ToLower() switch
        {
            "sentence" => isDescending 
                ? query.OrderByDescending(x => x.Sentence) 
                : query.OrderBy(x => x.Sentence),
            "translation" => isDescending 
                ? query.OrderByDescending(x => x.Translation) 
                : query.OrderBy(x => x.Translation),
            "grammar" => isDescending 
                ? query.OrderByDescending(x => x.Grammar) 
                : query.OrderBy(x => x.Grammar),
            "difficultylevel" => isDescending 
                ? query.OrderByDescending(x => x.DifficultyLevel) 
                : query.OrderBy(x => x.DifficultyLevel),
            "displayorder" => isDescending 
                ? query.OrderByDescending(x => x.DisplayOrder) 
                : query.OrderBy(x => x.DisplayOrder),
            "createdat" => isDescending 
                ? query.OrderByDescending(x => x.Created) 
                : query.OrderBy(x => x.Created),
            _ => query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id)
        };
    }
}
