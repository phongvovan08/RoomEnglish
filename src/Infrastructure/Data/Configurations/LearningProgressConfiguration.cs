using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoomEnglish.Domain.Entities;

namespace RoomEnglish.Infrastructure.Data.Configurations;

public class UserWordProgressConfiguration : IEntityTypeConfiguration<UserWordProgress>
{
    public void Configure(EntityTypeBuilder<UserWordProgress> builder)
    {
        builder.Property(t => t.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.HasIndex(x => new { x.UserId, x.WordId })
            .IsUnique();

        builder.HasOne(d => d.Word)
            .WithMany(p => p.UserProgress)
            .HasForeignKey(d => d.WordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DictationResultConfiguration : IEntityTypeConfiguration<DictationResult>
{
    public void Configure(EntityTypeBuilder<DictationResult> builder)
    {
        builder.Property(t => t.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(t => t.UserInput)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(t => t.CorrectAnswer)
            .HasMaxLength(1000)
            .IsRequired();

        builder.HasOne(d => d.Example)
            .WithMany(p => p.DictationResults)
            .HasForeignKey(d => d.ExampleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class LearningSessionConfiguration : IEntityTypeConfiguration<LearningSession>
{
    public void Configure(EntityTypeBuilder<LearningSession> builder)
    {
        builder.Property(t => t.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(t => t.SessionType)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasOne(d => d.Category)
            .WithMany()
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}