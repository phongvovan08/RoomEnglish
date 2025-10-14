using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Application.Common.Exceptions;
using RoomEnglish.Application.Vocabulary.Queries;
using RoomEnglish.Domain.Entities;
using AutoMapper;

namespace RoomEnglish.Application.Vocabulary.Commands.CompleteLearningSession;

public record CompleteLearningSessionCommand : IRequest<LearningSessionDto>
{
    public int SessionId { get; init; }
    public int CorrectAnswers { get; init; }
    public int Score { get; init; }
}

public class CompleteLearningSessionCommandHandler : IRequestHandler<CompleteLearningSessionCommand, LearningSessionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly IMapper _mapper;

    public CompleteLearningSessionCommandHandler(
        IApplicationDbContext context,
        IUser currentUser,
        IMapper mapper)
    {
        _context = context;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<LearningSessionDto> Handle(CompleteLearningSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _context.LearningSessions
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == request.SessionId && x.UserId == _currentUser.Id, cancellationToken);

        if (session == null)
        {
            throw new NotFoundException(nameof(LearningSession), request.SessionId.ToString());
        }

        if (session.IsCompleted)
        {
            throw new InvalidOperationException("Session is already completed.");
        }

        // Update session
        session.CompletedAt = DateTime.UtcNow;
        session.CorrectAnswers = request.CorrectAnswers;
        session.Score = request.Score;
        session.IsCompleted = true;
        session.DurationMinutes = (int)(session.CompletedAt.Value - session.StartedAt).TotalMinutes;

        await _context.SaveChangesAsync(cancellationToken);

        return new LearningSessionDto
        {
            Id = session.Id,
            UserId = session.UserId,
            CategoryId = session.CategoryId,
            CategoryName = session.Category.Name,
            StartedAt = session.StartedAt,
            CompletedAt = session.CompletedAt,
            TotalWords = session.TotalWords,
            CorrectAnswers = session.CorrectAnswers,
            DurationMinutes = session.DurationMinutes,
            SessionType = session.SessionType,
            Score = session.Score,
            IsCompleted = session.IsCompleted
        };
    }
}