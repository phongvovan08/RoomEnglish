using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoomEnglish.Domain.Entities;

namespace RoomEnglish.Infrastructure.Data.Configurations;

public class VocabularyExampleConfiguration : IEntityTypeConfiguration<VocabularyExample>
{
    public void Configure(EntityTypeBuilder<VocabularyExample> builder)
    {
        builder.Property(t => t.Sentence)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(t => t.Translation)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(t => t.Grammar)
            .HasMaxLength(500);

        builder.Property(t => t.AudioUrl)
            .HasMaxLength(500);

        builder.HasOne(d => d.Word)
            .WithMany(p => p.Examples)
            .HasForeignKey(d => d.WordId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed data
        builder.HasData(
            new VocabularyExample
            {
                Id = 1,
                Sentence = "Hello, how are you today?",
                Translation = "Xin chào, hôm nay bạn thế nào?",
                Grammar = "'Hello' is an interjection used as a greeting, followed by a question form 'how are you'",
                DifficultyLevel = 1,
                DisplayOrder = 1,
                WordId = 1,
                Created = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new VocabularyExample
            {
                Id = 2,
                Sentence = "She is a beautiful woman.",
                Translation = "Cô ấy là một người phụ nữ xinh đẹp.",
                Grammar = "'Beautiful' is an adjective modifying the noun 'woman', placed before the noun in English",
                DifficultyLevel = 1,
                DisplayOrder = 1,
                WordId = 2,
                Created = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new VocabularyExample
            {
                Id = 3,
                Sentence = "The view from the mountain is beautiful.",
                Translation = "Cảnh từ trên núi rất đẹp.",
                Grammar = "'Beautiful' is a predicate adjective following the linking verb 'is', with prepositional phrase 'from the mountain'",
                DifficultyLevel = 2,
                DisplayOrder = 2,
                WordId = 2,
                Created = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new VocabularyExample
            {
                Id = 4,
                Sentence = "We had dinner at an Italian restaurant.",
                Translation = "Chúng tôi đã ăn tối tại một nhà hàng Ý.",
                Grammar = "'At' is a preposition showing location, followed by the noun phrase 'an Italian restaurant'",
                DifficultyLevel = 2,
                DisplayOrder = 1,
                WordId = 3,
                Created = DateTime.UtcNow,
                CreatedBy = "System"
            }
        );
    }
}