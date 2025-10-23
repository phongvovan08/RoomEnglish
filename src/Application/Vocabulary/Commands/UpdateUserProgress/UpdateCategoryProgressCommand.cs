using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomEnglish.Application.Common.Interfaces;

namespace RoomEnglish.Application.Vocabulary.Commands.UpdateUserProgress;

public record UpdateCategoryProgressCommand : IRequest<Unit>
{
    public int CategoryId { get; init; }
    public string? UserId { get; init; }
}

public class UpdateCategoryProgressCommandHandler : IRequestHandler<UpdateCategoryProgressCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public UpdateCategoryProgressCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<Unit> Handle(UpdateCategoryProgressCommand request, CancellationToken cancellationToken)
    {
        var userId = request.UserId ?? _user.Id ?? throw new UnauthorizedAccessException();

        // Get or create category progress
        var categoryProgress = await _context.UserCategoryProgress
            .FirstOrDefaultAsync(p => p.UserId == userId && p.CategoryId == request.CategoryId, cancellationToken);

        if (categoryProgress == null)
        {
            categoryProgress = new Domain.Entities.UserCategoryProgress
            {
                UserId = userId,
                CategoryId = request.CategoryId,
                CompletionPercentage = 0,
                WordsStudied = 0,
                WordsMastered = 0,
                FirstStudiedAt = DateTime.UtcNow
            };
            _context.UserCategoryProgress.Add(categoryProgress);
        }

        // Calculate statistics from word progress
        var wordProgress = await _context.UserWordProgress
            .Where(p => p.UserId == userId && p.Word.CategoryId == request.CategoryId)
            .ToListAsync(cancellationToken);

        var totalWords = await _context.VocabularyWords
            .CountAsync(w => w.CategoryId == request.CategoryId && w.IsActive, cancellationToken);

        categoryProgress.WordsStudied = wordProgress.Count;
        categoryProgress.WordsMastered = wordProgress.Count(p => p.IsMastered);
        categoryProgress.CompletionPercentage = totalWords > 0 
            ? (int)((double)wordProgress.Count / totalWords * 100)
            : 0;

        // Calculate average accuracy
        if (wordProgress.Any())
        {
            var avgAccuracy = wordProgress.Average(p => 
                p.TotalAttempts > 0 ? (double)p.CorrectAnswers / p.TotalAttempts * 100 : 0);
            var masteredPercentage = totalWords > 0 ? (double)categoryProgress.WordsMastered / totalWords * 100 : 0;
            
            categoryProgress.AverageAccuracy = (avgAccuracy + masteredPercentage) / 2;
            categoryProgress.IsMastered = categoryProgress.CompletionPercentage >= 80;
        }

        categoryProgress.LastStudiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
