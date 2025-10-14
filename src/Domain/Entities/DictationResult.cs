namespace RoomEnglish.Domain.Entities;

public class DictationResult : BaseAuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public int ExampleId { get; set; }
    public string UserInput { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int AccuracyPercentage { get; set; } = 0; // 0-100%
    public int TimeTakenSeconds { get; set; } = 0;
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public VocabularyExample Example { get; set; } = null!;
}