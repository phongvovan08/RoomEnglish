using System.Security.Cryptography;
using System.Text;
using RoomEnglish.Application.Common.Interfaces;
using RoomEnglish.Domain.Entities;

namespace RoomEnglish.Application.Audio.Commands;

/// <summary>
/// Saves audio data to cache database
/// </summary>
public record SaveAudioCacheCommand : IRequest<Unit>
{
    public string Text { get; init; } = string.Empty;
    public string Voice { get; init; } = string.Empty;
    public double Rate { get; init; } = 1.0;
    public string Provider { get; init; } = "openai";
    public byte[] AudioData { get; init; } = Array.Empty<byte>();
    public string MimeType { get; init; } = "audio/mpeg";
    public int? ExpiryDays { get; init; } = 365; // Default 30 days for database
}

public class SaveAudioCacheCommandHandler : IRequestHandler<SaveAudioCacheCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public SaveAudioCacheCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(SaveAudioCacheCommand request, CancellationToken cancellationToken)
    {
        var cacheKeyHash = GenerateCacheKeyHash(request.Text, request.Voice, request.Rate, request.Provider);

        // Check if already exists
        var existing = await _context.AudioCaches
            .FirstOrDefaultAsync(c => c.CacheKeyHash == cacheKeyHash, cancellationToken);

        if (existing != null)
        {
            // Update existing entry
            existing.AudioData = request.AudioData;
            existing.SizeBytes = request.AudioData.Length;
            existing.MimeType = request.MimeType;
            existing.LastAccessedAt = DateTime.UtcNow;
            existing.HitCount++;
            
            if (request.ExpiryDays.HasValue)
            {
                existing.ExpiresAt = DateTime.UtcNow.AddDays(request.ExpiryDays.Value);
            }
        }
        else
        {
            // Create new entry
            var cache = new AudioCache
            {
                Text = request.Text.Length > 2000 ? request.Text.Substring(0, 2000) : request.Text,
                Voice = request.Voice,
                Rate = request.Rate,
                Provider = request.Provider,
                AudioData = request.AudioData,
                MimeType = request.MimeType,
                SizeBytes = request.AudioData.Length,
                CacheKeyHash = cacheKeyHash,
                HitCount = 0,
                LastAccessedAt = DateTime.UtcNow,
                ExpiresAt = request.ExpiryDays.HasValue 
                    ? DateTime.UtcNow.AddDays(request.ExpiryDays.Value) 
                    : null
            };

            _context.AudioCaches.Add(cache);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    private static string GenerateCacheKeyHash(string text, string voice, double rate, string provider)
    {
        var cacheKey = $"{text}_{voice}_{rate:F2}_{provider}";
        var bytes = Encoding.UTF8.GetBytes(cacheKey);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
