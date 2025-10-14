using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoomEnglish.Domain.Entities;

namespace RoomEnglish.Infrastructure.Data.Configurations;

public class VocabularyCategoryConfiguration : IEntityTypeConfiguration<VocabularyCategory>
{
    public void Configure(EntityTypeBuilder<VocabularyCategory> builder)
    {
        builder.Property(t => t.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        builder.Property(t => t.Color)
            .HasMaxLength(50)
            .HasDefaultValue("Blue");

        builder.Property(t => t.IconName)
            .HasMaxLength(100)
            .HasDefaultValue("mdi:book");

        builder.HasIndex(x => x.Name)
            .IsUnique();

        // Seed data
        builder.HasData(
            new VocabularyCategory
            {
                Id = 1,
                Name = "Basic Vocabulary",
                Description = "Essential words for beginners",
                Color = "Blue",
                IconName = "mdi:book-alphabet",
                DisplayOrder = 1,
                Created = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new VocabularyCategory
            {
                Id = 2,
                Name = "Travel & Tourism",
                Description = "Words related to travel and tourism",
                Color = "Green",
                IconName = "mdi:airplane",
                DisplayOrder = 2,
                Created = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new VocabularyCategory
            {
                Id = 3,
                Name = "Business English",
                Description = "Professional vocabulary for business",
                Color = "Purple",
                IconName = "mdi:briefcase",
                DisplayOrder = 3,
                Created = DateTime.UtcNow,
                CreatedBy = "System"
            }
        );
    }
}