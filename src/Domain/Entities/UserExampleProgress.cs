namespace RoomEnglish.Domain.Entities;

/// <summary>
/// Tracks user's progress for each vocabulary example (dictation practice)
/// </summary>
public class UserExampleProgress : BaseAuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public int ExampleId { get; set; }
    
    /// <summary>
    /// Number of times user attempted this example
    /// </summary>
    public int TotalAttempts { get; set; } = 0;
    
    /// <summary>
    /// Number of correct completions
    /// </summary>
    public int CorrectAttempts { get; set; } = 0;
    
    /// <summary>
    /// Best accuracy percentage achieved (0-100)
    /// </summary>
    public int BestAccuracy { get; set; } = 0;
    
    /// <summary>
    /// Average accuracy across all attempts
    /// </summary>
    public double AverageAccuracy { get; set; } = 0;
    
    /// <summary>
    /// Fastest time to complete (in seconds)
    /// </summary>
    public int FastestTimeSeconds { get; set; } = 0;
    
    /// <summary>
    /// Total time spent on this example (in seconds)
    /// </summary>
    public int TotalTimeSeconds { get; set; } = 0;
    
    /// <summary>
    /// Whether user has completed this example with 80%+ accuracy
    /// </summary>
    public bool IsCompleted { get; set; } = false;
    
    /// <summary>
    /// Whether user has mastered this example (90%+ accuracy)
    /// </summary>
    public bool IsMastered { get; set; } = false;
    
    /// <summary>
    /// When user first attempted this example
    /// </summary>
    public DateTime? FirstAttemptedAt { get; set; }
    
    /// <summary>
    /// When user last attempted this example
    /// </summary>
    public DateTime? LastAttemptedAt { get; set; }
    
    /// <summary>
    /// When user completed this example (first 80%+ accuracy)
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public VocabularyExample Example { get; set; } = null!;
}
