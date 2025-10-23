using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomEnglish.Application.Common.Interfaces;

namespace RoomEnglish.Application.Vocabulary.Commands.UpdateUserProgress;

public record UpdateExampleProgressCommand : IRequest<Unit>
{
    public int ExampleId { get; init; }
    public int AccuracyPercentage { get; init; }
    public string? UserId { get; init; }
}

public class UpdateExampleProgressCommandHandler : IRequestHandler<UpdateExampleProgressCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ISender _sender;

    public UpdateExampleProgressCommandHandler(IApplicationDbContext context, IUser user, ISender sender)
    {
        _context = context;
        _user = user;
        _sender = sender;
    }

    public async Task<Unit> Handle(UpdateExampleProgressCommand request, CancellationToken cancellationToken)
    {
        var userId = request.UserId ?? _user.Id ?? throw new UnauthorizedAccessException();

        // Get or create example progress
        var exampleProgress = await _context.UserExampleProgress
            .FirstOrDefaultAsync(p => p.UserId == userId && p.ExampleId == request.ExampleId, cancellationToken);

        if (exampleProgress == null)
        {
            exampleProgress = new Domain.Entities.UserExampleProgress
            {
                UserId = userId,
                ExampleId = request.ExampleId,
                IsCompleted = false,
                TotalAttempts = 0,
                BestAccuracy = 0,
                FirstAttemptedAt = DateTime.UtcNow
            };
            _context.UserExampleProgress.Add(exampleProgress);
        }

        // Update statistics
        exampleProgress.TotalAttempts++;
        
        if (request.AccuracyPercentage > exampleProgress.BestAccuracy)
        {
            exampleProgress.BestAccuracy = request.AccuracyPercentage;
        }

        // Mark as completed if accuracy >= 80%
        if (request.AccuracyPercentage >= 80 && !exampleProgress.IsCompleted)
        {
            exampleProgress.IsCompleted = true;
            exampleProgress.CompletedAt = DateTime.UtcNow;
        }

        exampleProgress.LastAttemptedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Get the word ID and update word progress based on example completion
        var example = await _context.VocabularyExamples
            .FirstOrDefaultAsync(e => e.Id == request.ExampleId, cancellationToken);
        
        if (example != null)
        {
            await UpdateWordProgressFromExamples(example.WordId, userId, cancellationToken);
        }

        return Unit.Value;
    }

    private async Task UpdateWordProgressFromExamples(int wordId, string userId, CancellationToken cancellationToken)
    {
        // Get all examples for this word
        var totalExamples = await _context.VocabularyExamples
            .Where(e => e.WordId == wordId && e.IsActive)
            .CountAsync(cancellationToken);

        if (totalExamples == 0) return;

        // Get user's progress on these examples
        var exampleProgress = await _context.UserExampleProgress
            .Where(p => p.UserId == userId && p.Example.WordId == wordId)
            .ToListAsync(cancellationToken);

        var completedExamples = exampleProgress.Count(p => p.IsCompleted);
        var completionPercentage = (double)completedExamples / totalExamples * 100;

        // Get or create word progress
        var wordProgress = await _context.UserWordProgress
            .FirstOrDefaultAsync(p => p.UserId == userId && p.WordId == wordId, cancellationToken);

        if (wordProgress == null)
        {
            wordProgress = new Domain.Entities.UserWordProgress
            {
                UserId = userId,
                WordId = wordId,
                StudiedTimes = 0,
                CorrectAnswers = 0,
                TotalAttempts = 0,
                IsMastered = false,
                MasteryLevel = 0,
                FirstStudiedAt = DateTime.UtcNow
            };
            _context.UserWordProgress.Add(wordProgress);
        }

        // Update word mastery based on example completion
        wordProgress.StudiedTimes = exampleProgress.Count;
        
        // Calculate mastery level based on completion percentage
        wordProgress.MasteryLevel = completionPercentage switch
        {
            >= 95 => 5,
            >= 80 => 4,
            >= 60 => 3,
            >= 40 => 2,
            >= 20 => 1,
            _ => 0
        };

        // Mark as mastered if all examples are completed
        wordProgress.IsMastered = completedExamples == totalExamples;
        wordProgress.LastStudiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Trigger category progress update
        await _sender.Send(new UpdateCategoryProgressCommand
        {
            CategoryId = (await _context.VocabularyWords
                .Where(w => w.Id == wordId)
                .Select(w => w.CategoryId)
                .FirstOrDefaultAsync(cancellationToken)),
            UserId = userId
        }, cancellationToken);
    }
}
