namespace RoomEnglish.Domain.Entities;

public class VocabularyExample : BaseAuditableEntity
{
    public string Sentence { get; set; } = string.Empty;
    public string Translation { get; set; } = string.Empty; // Vietnamese translation
    public string? AudioUrl { get; set; } // URL to example audio file
    public int DifficultyLevel { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;

    // Foreign Keys
    public int WordId { get; set; }
    
    // Navigation properties
    public VocabularyWord Word { get; set; } = null!;
    public IList<DictationResult> DictationResults { get; private set; } = new List<DictationResult>();
}