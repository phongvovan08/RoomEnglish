namespace RoomEnglish.Domain.Entities;

public class VocabularyCategory : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = "Blue"; // For UI theming
    public string IconName { get; set; } = "mdi:book"; // Icon for category
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;

    // Navigation properties
    public IList<VocabularyWord> Words { get; private set; } = new List<VocabularyWord>();
}