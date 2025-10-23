namespace RoomEnglish.Domain.Entities;

/// <summary>
/// Tracks user's learning progress for each vocabulary category
/// </summary>
public class UserCategoryProgress : BaseAuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    
    /// <summary>
    /// Total words studied in this category
    /// </summary>
    public int WordsStudied { get; set; } = 0;
    
    /// <summary>
    /// Total words mastered in this category
    /// </summary>
    public int WordsMastered { get; set; } = 0;
    
    /// <summary>
    /// Total examples completed in this category
    /// </summary>
    public int ExamplesCompleted { get; set; } = 0;
    
    /// <summary>
    /// Overall completion percentage (0-100)
    /// </summary>
    public int CompletionPercentage { get; set; } = 0;
    
    /// <summary>
    /// Average accuracy rate across all attempts
    /// </summary>
    public double AverageAccuracy { get; set; } = 0;
    
    /// <summary>
    /// Total time spent studying this category (in minutes)
    /// </summary>
    public int TotalMinutesSpent { get; set; } = 0;
    
    /// <summary>
    /// Number of learning sessions completed
    /// </summary>
    public int SessionsCompleted { get; set; } = 0;
    
    /// <summary>
    /// When user first started this category
    /// </summary>
    public DateTime? FirstStudiedAt { get; set; }
    
    /// <summary>
    /// When user last studied this category
    /// </summary>
    public DateTime? LastStudiedAt { get; set; }
    
    /// <summary>
    /// Whether user has mastered this category (80%+ completion)
    /// </summary>
    public bool IsMastered { get; set; } = false;

    // Navigation properties
    public VocabularyCategory Category { get; set; } = null!;
}
