using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Application.Common.Models;
using RoomEnglish.Application.Common.Mappings;
using AutoMapper;
using System.Linq;

namespace RoomEnglish.Application.Vocabulary.Queries.GetVocabularyCategories;

public record GetVocabularyCategoriesQuery : IRequest<PaginatedList<VocabularyCategoryDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public bool IncludeInactive { get; init; } = false;
    public string? SearchTerm { get; init; }
    public string? SortBy { get; init; }
    public string? SortOrder { get; init; } = "asc";
}

public class GetVocabularyCategoriesQueryHandler : IRequestHandler<GetVocabularyCategoriesQuery, PaginatedList<VocabularyCategoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetVocabularyCategoriesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<VocabularyCategoryDto>> Handle(GetVocabularyCategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.VocabularyCategories.AsQueryable();

        if (!request.IncludeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(x => 
                x.Name.ToLower().Contains(searchTerm) ||
                (x.Description != null && x.Description.ToLower().Contains(searchTerm)));
        }

        // Apply sorting
        query = ApplySorting(query, request.SortBy, request.SortOrder);

        return await query
            .Select(x => new VocabularyCategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Color = x.Color,
                IconName = x.IconName,
                IsActive = x.IsActive,
                DisplayOrder = x.DisplayOrder,
                WordCount = x.Words.Count(w => w.IsActive),
                VocabularyCount = x.Words.Count(w => w.IsActive), // Same as WordCount
                Created = x.Created.DateTime
            })
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }

    private static IQueryable<Domain.Entities.VocabularyCategory> ApplySorting(
        IQueryable<Domain.Entities.VocabularyCategory> query, 
        string? sortBy, 
        string? sortOrder)
    {
        var isDescending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy?.ToLower() switch
        {
            "name" => isDescending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "description" => isDescending ? query.OrderByDescending(x => x.Description) : query.OrderBy(x => x.Description),
            "vocabularycount" => isDescending ? query.OrderByDescending(x => x.Words.Count(w => w.IsActive)) : query.OrderBy(x => x.Words.Count(w => w.IsActive)),
            "createdat" => isDescending ? query.OrderByDescending(x => x.Created) : query.OrderBy(x => x.Created),
            _ => query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name) // Default sorting
        };
    }
}