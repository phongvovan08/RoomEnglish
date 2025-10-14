namespace RoomEnglish.Domain.Entities;

public class VocabularyWord : BaseAuditableEntity
{
    public string Word { get; set; } = string.Empty;
    public string Phonetic { get; set; } = string.Empty; // IPA phonetic notation
    public string PartOfSpeech { get; set; } = string.Empty; // noun, verb, adjective, etc.
    public string Meaning { get; set; } = string.Empty; // Vietnamese meaning
    public string Definition { get; set; } = string.Empty; // English definition
    public string? AudioUrl { get; set; } // URL to audio pronunciation file
    public int DifficultyLevel { get; set; } = 1; // 1 = Beginner, 2 = Intermediate, 3 = Advanced
    public bool IsActive { get; set; } = true;
    public int ViewCount { get; set; } = 0;
    public int CorrectCount { get; set; } = 0; // For statistics
    public int IncorrectCount { get; set; } = 0;

    // Foreign Keys
    public int CategoryId { get; set; }
    
    // Navigation properties
    public VocabularyCategory Category { get; set; } = null!;
    public IList<VocabularyExample> Examples { get; private set; } = new List<VocabularyExample>();
    public IList<UserWordProgress> UserProgress { get; private set; } = new List<UserWordProgress>();
}