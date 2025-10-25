using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Domain.Entities;

namespace RoomEnglish.Application.Audio.Commands;

/// <summary>
/// Clears expired and least used cache entries
/// </summary>
public record CleanupAudioCacheCommand : IRequest<int>
{
    public int? MaxCacheSizeMB { get; init; } = 100; // Default 100MB total
    public bool DeleteExpired { get; init; } = true;
}

public class CleanupAudioCacheCommandHandler : IRequestHandler<CleanupAudioCacheCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CleanupAudioCacheCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CleanupAudioCacheCommand request, CancellationToken cancellationToken)
    {
        int deletedCount = 0;

        // Delete expired entries
        if (request.DeleteExpired)
        {
            var expired = await _context.AudioCaches
                .Where(c => c.ExpiresAt != null && c.ExpiresAt < DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            _context.AudioCaches.RemoveRange(expired);
            deletedCount += expired.Count;
        }

        // Check total cache size
        if (request.MaxCacheSizeMB.HasValue)
        {
            var maxBytes = request.MaxCacheSizeMB.Value * 1024 * 1024;
            var currentSize = await _context.AudioCaches.SumAsync(c => c.SizeBytes, cancellationToken);

            if (currentSize > maxBytes)
            {
                // Delete least used entries (lowest hit count, oldest access)
                var excessBytes = currentSize - maxBytes;
                var toDelete = await _context.AudioCaches
                    .OrderBy(c => c.HitCount)
                    .ThenBy(c => c.LastAccessedAt)
                    .ToListAsync(cancellationToken);

                int bytesDeleted = 0;
                var entriesToRemove = new List<AudioCache>();

                foreach (var entry in toDelete)
                {
                    entriesToRemove.Add(entry);
                    bytesDeleted += entry.SizeBytes;
                    
                    if (bytesDeleted >= excessBytes)
                        break;
                }

                _context.AudioCaches.RemoveRange(entriesToRemove);
                deletedCount += entriesToRemove.Count;
            }
        }

        if (deletedCount > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return deletedCount;
    }
}
