namespace RoomEnglish.Domain.Entities;

/// <summary>
/// Stores cached audio files from Text-to-Speech API to reduce repeated API calls
/// </summary>
public class AudioCache : BaseAuditableEntity
{
    /// <summary>
    /// Text content that was converted to speech
    /// </summary>
    public string Text { get; set; } = string.Empty;
    
    /// <summary>
    /// Voice name used for synthesis (e.g., "alloy", "nova")
    /// </summary>
    public string Voice { get; set; } = string.Empty;
    
    /// <summary>
    /// Speech rate/speed used (0.5 to 2.0)
    /// </summary>
    public double Rate { get; set; } = 1.0;
    
    /// <summary>
    /// TTS provider (openai, webspeech, etc.)
    /// </summary>
    public string Provider { get; set; } = "openai";
    
    /// <summary>
    /// Audio data stored as byte array (MP3 format)
    /// </summary>
    public byte[] AudioData { get; set; } = Array.Empty<byte>();
    
    /// <summary>
    /// MIME type of audio (e.g., "audio/mpeg")
    /// </summary>
    public string MimeType { get; set; } = "audio/mpeg";
    
    /// <summary>
    /// Size of audio file in bytes
    /// </summary>
    public int SizeBytes { get; set; }
    
    /// <summary>
    /// Hash of the cache key for faster lookups
    /// </summary>
    public string CacheKeyHash { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of times this cached audio has been used
    /// </summary>
    public int HitCount { get; set; } = 0;
    
    /// <summary>
    /// When this cache entry was last accessed
    /// </summary>
    public DateTime LastAccessedAt { get; set; }
    
    /// <summary>
    /// When this cache should expire (optional)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}
