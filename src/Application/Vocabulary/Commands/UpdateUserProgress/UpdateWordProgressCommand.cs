using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomEnglish.Application.Common.Interfaces;

namespace RoomEnglish.Application.Vocabulary.Commands.UpdateUserProgress;

public record UpdateWordProgressCommand : IRequest<Unit>
{
    public int WordId { get; init; }
    public bool IsCorrect { get; init; }
    public string? UserId { get; init; }
}

public class UpdateWordProgressCommandHandler : IRequestHandler<UpdateWordProgressCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ISender _sender;

    public UpdateWordProgressCommandHandler(IApplicationDbContext context, IUser user, ISender sender)
    {
        _context = context;
        _user = user;
        _sender = sender;
    }

    public async Task<Unit> Handle(UpdateWordProgressCommand request, CancellationToken cancellationToken)
    {
        var userId = request.UserId ?? _user.Id ?? throw new UnauthorizedAccessException();

        // Get or create word progress
        var wordProgress = await _context.UserWordProgress
            .FirstOrDefaultAsync(p => p.UserId == userId && p.WordId == request.WordId, cancellationToken);

        if (wordProgress == null)
        {
            wordProgress = new Domain.Entities.UserWordProgress
            {
                UserId = userId,
                WordId = request.WordId,
                StudiedTimes = 0,
                CorrectAnswers = 0,
                TotalAttempts = 0,
                IsMastered = false,
                MasteryLevel = 0,
                FirstStudiedAt = DateTime.UtcNow
            };
            _context.UserWordProgress.Add(wordProgress);
        }

        // Update statistics
        wordProgress.StudiedTimes++;
        wordProgress.TotalAttempts++;
        if (request.IsCorrect)
        {
            wordProgress.CorrectAnswers++;
        }

        var accuracyRate = wordProgress.TotalAttempts > 0
            ? (double)wordProgress.CorrectAnswers / wordProgress.TotalAttempts * 100
            : 0;

        // Calculate mastery level (0-5)
        wordProgress.MasteryLevel = (accuracyRate, wordProgress.StudiedTimes) switch
        {
            ( >= 95, >= 5) => 5,
            ( >= 85, >= 4) => 4,
            ( >= 75, >= 3) => 3,
            ( >= 65, >= 2) => 2,
            ( >= 50, >= 1) => 1,
            _ => 0
        };

        // Mark as mastered if accuracy >= 90% and studied >= 3 times
        wordProgress.IsMastered = accuracyRate >= 90 && wordProgress.StudiedTimes >= 3;

        wordProgress.LastStudiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Update category progress
        var word = await _context.VocabularyWords
            .FirstOrDefaultAsync(w => w.Id == request.WordId, cancellationToken);
        
        if (word != null)
        {
            await _sender.Send(new UpdateCategoryProgressCommand 
            { 
                CategoryId = word.CategoryId,
                UserId = userId
            }, cancellationToken);
        }

        return Unit.Value;
    }
}
