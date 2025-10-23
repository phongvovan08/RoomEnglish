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

    public UpdateExampleProgressCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
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

        return Unit.Value;
    }
}
