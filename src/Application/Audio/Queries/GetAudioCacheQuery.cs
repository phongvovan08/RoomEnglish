using System.Security.Cryptography;
using System.Text;
using RoomEnglish.Application.Common.Interfaces;

namespace RoomEnglish.Application.Audio.Queries;

public class AudioCacheDto
{
    public byte[] AudioData { get; set; } = Array.Empty<byte>();
    public string MimeType { get; set; } = string.Empty;
    public int SizeBytes { get; set; }
}

/// <summary>
/// Gets cached audio from database
/// </summary>
public record GetAudioCacheQuery : IRequest<AudioCacheDto?>
{
    public string Text { get; init; } = string.Empty;
    public string Voice { get; init; } = string.Empty;
    public double Rate { get; init; } = 1.0;
    public string Provider { get; init; } = "openai";
}

public class GetAudioCacheQueryHandler : IRequestHandler<GetAudioCacheQuery, AudioCacheDto?>
{
    private readonly IApplicationDbContext _context;

    public GetAudioCacheQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AudioCacheDto?> Handle(GetAudioCacheQuery request, CancellationToken cancellationToken)
    {
        var cacheKeyHash = GenerateCacheKeyHash(request.Text, request.Voice, request.Rate, request.Provider);

        var cached = await _context.AudioCaches
            .Where(c => c.CacheKeyHash == cacheKeyHash)
            .Where(c => c.ExpiresAt == null || c.ExpiresAt > DateTime.UtcNow)
            .Select(c => new AudioCacheDto
            {
                AudioData = c.AudioData,
                MimeType = c.MimeType,
                SizeBytes = c.SizeBytes
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (cached != null)
        {
            // Update hit count and last accessed time (fire and forget)
            _ = Task.Run(async () =>
            {
                var entry = await _context.AudioCaches
                    .FirstOrDefaultAsync(c => c.CacheKeyHash == cacheKeyHash, CancellationToken.None);
                if (entry != null)
                {
                    entry.HitCount++;
                    entry.LastAccessedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync(CancellationToken.None);
                }
            }, cancellationToken);
        }

        return cached;
    }

    private static string GenerateCacheKeyHash(string text, string voice, double rate, string provider)
    {
        var cacheKey = $"{text}_{voice}_{rate:F2}_{provider}";
        var bytes = Encoding.UTF8.GetBytes(cacheKey);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
