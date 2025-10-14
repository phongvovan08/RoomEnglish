namespace RoomEnglish.Domain.Entities;

public class UserWordProgress : BaseAuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public int WordId { get; set; }
    public int StudiedTimes { get; set; } = 0;
    public int CorrectAnswers { get; set; } = 0;
    public int TotalAttempts { get; set; } = 0;
    public DateTime? FirstStudiedAt { get; set; }
    public DateTime? LastStudiedAt { get; set; }
    public bool IsMastered { get; set; } = false;
    public int MasteryLevel { get; set; } = 0; // 0-100%

    // Navigation properties
    public VocabularyWord Word { get; set; } = null!;
}