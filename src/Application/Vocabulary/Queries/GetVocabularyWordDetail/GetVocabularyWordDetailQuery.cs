using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomEnglish.Application.Common.Exceptions;
using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Application.Common.Mappings;
using RoomEnglish.Domain.Entities;
using AutoMapper;

namespace RoomEnglish.Application.Vocabulary.Queries.GetVocabularyWordDetail;

public record GetVocabularyWordDetailQuery(int Id, string? UserId = null) : IRequest<VocabularyWordDto>;

public class GetVocabularyWordDetailQueryHandler : IRequestHandler<GetVocabularyWordDetailQuery, VocabularyWordDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetVocabularyWordDetailQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<VocabularyWordDto> Handle(GetVocabularyWordDetailQuery request, CancellationToken cancellationToken)
    {
        var word = await _context.VocabularyWords
            .Include(x => x.Category)
            .Include(x => x.Examples.Where(e => e.IsActive))
            .Include(x => x.UserProgress.Where(p => p.UserId == request.UserId))
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (word == null)
        {
            throw new NotFoundException(nameof(VocabularyWord), request.Id.ToString());
        }

        // Update view count
        word.ViewCount++;
        await _context.SaveChangesAsync(cancellationToken);

        return new VocabularyWordDto
        {
            Id = word.Id,
            Word = word.Word,
            Phonetic = word.Phonetic,
            PartOfSpeech = word.PartOfSpeech,
            Meaning = word.Meaning,
            Definition = word.Definition,
            AudioUrl = word.AudioUrl,
            DifficultyLevel = word.DifficultyLevel,
            IsActive = word.IsActive,
            ViewCount = word.ViewCount,
            CorrectCount = word.CorrectCount,
            IncorrectCount = word.IncorrectCount,
            CategoryId = word.CategoryId,
            CategoryName = word.Category.Name,
            Examples = word.Examples
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
            UserProgress = word.UserProgress.FirstOrDefault() != null ? new UserWordProgressDto
            {
                Id = word.UserProgress.First().Id,
                UserId = word.UserProgress.First().UserId,
                WordId = word.UserProgress.First().WordId,
                StudiedTimes = word.UserProgress.First().StudiedTimes,
                CorrectAnswers = word.UserProgress.First().CorrectAnswers,
                TotalAttempts = word.UserProgress.First().TotalAttempts,
                FirstStudiedAt = word.UserProgress.First().FirstStudiedAt,
                LastStudiedAt = word.UserProgress.First().LastStudiedAt,
                IsMastered = word.UserProgress.First().IsMastered,
                MasteryLevel = word.UserProgress.First().MasteryLevel
            } : null
        };
    }
}