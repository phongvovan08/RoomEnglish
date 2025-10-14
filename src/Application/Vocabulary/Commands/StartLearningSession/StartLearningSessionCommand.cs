using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Application.Common.Exceptions;
using RoomEnglish.Application.Vocabulary.Queries;
using RoomEnglish.Domain.Entities;
using AutoMapper;

namespace RoomEnglish.Application.Vocabulary.Commands.StartLearningSession;

public record StartLearningSessionCommand : IRequest<LearningSessionDto>
{
    public int CategoryId { get; init; }
    public string SessionType { get; init; } = "vocabulary"; // vocabulary, dictation, mixed
    public int? MaxWords { get; init; }
}

public class StartLearningSessionCommandHandler : IRequestHandler<StartLearningSessionCommand, LearningSessionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly IMapper _mapper;

    public StartLearningSessionCommandHandler(
        IApplicationDbContext context,
        IUser currentUser,
        IMapper mapper)
    {
        _context = context;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<LearningSessionDto> Handle(StartLearningSessionCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.VocabularyCategories
            .FirstOrDefaultAsync(x => x.Id == request.CategoryId, cancellationToken);

        if (category == null)
        {
            throw new NotFoundException(nameof(VocabularyCategory), request.CategoryId.ToString());
        }

        var userId = _currentUser.Id!;

        // End any active sessions for this user
        var activeSessions = await _context.LearningSessions
            .Where(x => x.UserId == userId && !x.IsCompleted)
            .ToListAsync(cancellationToken);

        foreach (var session in activeSessions)
        {
            session.IsCompleted = true;
            session.CompletedAt = DateTime.UtcNow;
        }

        // Count available words in category
        var availableWordsCount = await _context.VocabularyWords
            .CountAsync(x => x.CategoryId == request.CategoryId && x.IsActive, cancellationToken);

        var totalWords = request.MaxWords ?? Math.Min(availableWordsCount, 10);

        // Create new session
        var learningSession = new LearningSession
        {
            UserId = userId,
            CategoryId = request.CategoryId,
            SessionType = request.SessionType,
            TotalWords = totalWords,
            StartedAt = DateTime.UtcNow
        };

        _context.LearningSessions.Add(learningSession);
        await _context.SaveChangesAsync(cancellationToken);

        return new LearningSessionDto
        {
            Id = learningSession.Id,
            UserId = learningSession.UserId,
            CategoryId = learningSession.CategoryId,
            CategoryName = category.Name,
            StartedAt = learningSession.StartedAt,
            CompletedAt = learningSession.CompletedAt,
            TotalWords = learningSession.TotalWords,
            CorrectAnswers = learningSession.CorrectAnswers,
            DurationMinutes = learningSession.DurationMinutes,
            SessionType = learningSession.SessionType,
            Score = learningSession.Score,
            IsCompleted = learningSession.IsCompleted
        };
    }
}