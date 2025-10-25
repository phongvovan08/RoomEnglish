using RoomEnglish.Application.Common.Interfaces;

namespace RoomEnglish.Application.Audio.Queries;

public class AudioCacheStatsDto
{
    public int TotalEntries { get; set; }
    public long TotalSizeBytes { get; set; }
    public int ExpiredEntries { get; set; }
    public DateTimeOffset? OldestEntry { get; set; }
    public DateTimeOffset? NewestEntry { get; set; }
    public int TotalHits { get; set; }
    public double AverageSizeKB { get; set; }
}

/// <summary>
/// Gets statistics about the audio cache
/// </summary>
public record GetAudioCacheStatsQuery : IRequest<AudioCacheStatsDto>;

public class GetAudioCacheStatsQueryHandler : IRequestHandler<GetAudioCacheStatsQuery, AudioCacheStatsDto>
{
    private readonly IApplicationDbContext _context;

    public GetAudioCacheStatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AudioCacheStatsDto> Handle(GetAudioCacheStatsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var stats = new AudioCacheStatsDto
        {
            TotalEntries = await _context.AudioCaches.CountAsync(cancellationToken),
            TotalSizeBytes = await _context.AudioCaches.SumAsync(c => (long)c.SizeBytes, cancellationToken),
            ExpiredEntries = await _context.AudioCaches
                .CountAsync(c => c.ExpiresAt != null && c.ExpiresAt < now, cancellationToken),
            OldestEntry = await _context.AudioCaches
                .OrderBy(c => c.Created)
                .Select(c => (DateTimeOffset?)c.Created)
                .FirstOrDefaultAsync(cancellationToken),
            NewestEntry = await _context.AudioCaches
                .OrderByDescending(c => c.Created)
                .Select(c => (DateTimeOffset?)c.Created)
                .FirstOrDefaultAsync(cancellationToken),
            TotalHits = await _context.AudioCaches.SumAsync(c => c.HitCount, cancellationToken)
        };

        if (stats.TotalEntries > 0)
        {
            stats.AverageSizeKB = Math.Round((double)stats.TotalSizeBytes / stats.TotalEntries / 1024, 2);
        }

        return stats;
    }
}
