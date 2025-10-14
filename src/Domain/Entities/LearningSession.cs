namespace RoomEnglish.Domain.Entities;

public class LearningSession : BaseAuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public int TotalWords { get; set; } = 0;
    public int CorrectAnswers { get; set; } = 0;
    public int DurationMinutes { get; set; } = 0;
    public string SessionType { get; set; } = string.Empty; // "vocabulary", "dictation", "mixed"
    public int Score { get; set; } = 0;
    public bool IsCompleted { get; set; } = false;

    // Navigation properties
    public VocabularyCategory Category { get; set; } = null!;
}