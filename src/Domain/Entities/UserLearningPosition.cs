namespace RoomEnglish.Domain.Entities;

/// <summary>
/// Tracks user's current learning position in example groups
/// Allows resuming from last position when returning to practice
/// </summary>
public class UserLearningPosition : BaseAuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public int WordId { get; set; }
    
    /// <summary>
    /// Group index (0-based) user is currently working on
    /// Each group contains 10 examples
    /// </summary>
    public int GroupIndex { get; set; }
    
    /// <summary>
    /// Absolute example index (0-based) within the word's example list
    /// </summary>
    public int LastExampleIndex { get; set; }
    
    /// <summary>
    /// When this position was last updated
    /// </summary>
    public DateTime LastAccessedAt { get; set; }

    // Navigation properties
    public VocabularyWord Word { get; set; } = null!;
}
