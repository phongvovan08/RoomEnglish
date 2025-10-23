using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoomEnglish.Domain.Entities;

namespace RoomEnglish.Infrastructure.Data.Configurations;

public class UserLearningPositionConfiguration : IEntityTypeConfiguration<UserLearningPosition>
{
    public void Configure(EntityTypeBuilder<UserLearningPosition> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.GroupIndex)
            .IsRequired();

        builder.Property(x => x.LastExampleIndex)
            .IsRequired();

        builder.Property(x => x.LastAccessedAt)
            .IsRequired();

        // Composite index for efficient querying by user and word
        builder.HasIndex(x => new { x.UserId, x.WordId })
            .IsUnique()
            .HasDatabaseName("IX_UserLearningPosition_UserId_WordId");

        // Foreign key to VocabularyWord
        builder.HasOne(x => x.Word)
            .WithMany()
            .HasForeignKey(x => x.WordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
