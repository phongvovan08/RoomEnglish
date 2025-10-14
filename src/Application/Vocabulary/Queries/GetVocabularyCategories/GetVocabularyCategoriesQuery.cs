using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Application.Common.Models;
using RoomEnglish.Application.Common.Mappings;
using AutoMapper;

namespace RoomEnglish.Application.Vocabulary.Queries.GetVocabularyCategories;

public record GetVocabularyCategoriesQuery : IRequest<PaginatedList<VocabularyCategoryDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public bool IncludeInactive { get; init; } = false;
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

        return await query
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
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
                Created = x.Created.DateTime
            })
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}