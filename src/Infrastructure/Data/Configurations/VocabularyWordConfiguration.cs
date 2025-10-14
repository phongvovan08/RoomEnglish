using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoomEnglish.Domain.Entities;

namespace RoomEnglish.Infrastructure.Data.Configurations;

public class VocabularyWordConfiguration : IEntityTypeConfiguration<VocabularyWord>
{
    public void Configure(EntityTypeBuilder<VocabularyWord> builder)
    {
        builder.Property(t => t.Word)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Phonetic)
            .HasMaxLength(300);

        builder.Property(t => t.PartOfSpeech)
            .HasMaxLength(50);

        builder.Property(t => t.Meaning)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(t => t.Definition)
            .HasMaxLength(2000);

        builder.Property(t => t.AudioUrl)
            .HasMaxLength(500);

        builder.HasIndex(x => new { x.Word, x.CategoryId })
            .IsUnique();

        builder.HasOne(d => d.Category)
            .WithMany(p => p.Words)
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed data for Basic Vocabulary category
        builder.HasData(
            new VocabularyWord
            {
                Id = 1,
                Word = "hello",
                Phonetic = "/həˈloʊ/",
                PartOfSpeech = "interjection",
                Meaning = "xin chào",
                Definition = "used as a greeting or to begin a phone conversation",
                DifficultyLevel = 1,
                CategoryId = 1,
                Created = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new VocabularyWord
            {
                Id = 2,
                Word = "beautiful",
                Phonetic = "/ˈbjuːtɪfəl/",
                PartOfSpeech = "adjective",
                Meaning = "đẹp",
                Definition = "pleasing the senses or mind aesthetically",
                DifficultyLevel = 1,
                CategoryId = 1,
                Created = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new VocabularyWord
            {
                Id = 3,
                Word = "restaurant",
                Phonetic = "/ˈrest(ə)rɑnt/",
                PartOfSpeech = "noun",
                Meaning = "nhà hàng",
                Definition = "a place where people pay to sit and eat meals",
                DifficultyLevel = 2,
                CategoryId = 2,
                Created = DateTime.UtcNow,
                CreatedBy = "System"
            }
        );
    }
}