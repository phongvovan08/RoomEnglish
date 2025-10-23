using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoomEnglish.Domain.Entities;

namespace RoomEnglish.Infrastructure.Data.Configurations;

public class UserCategoryProgressConfiguration : IEntityTypeConfiguration<UserCategoryProgress>
{
    public void Configure(EntityTypeBuilder<UserCategoryProgress> builder)
    {
        builder.Property(t => t.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.HasIndex(x => new { x.UserId, x.CategoryId })
            .IsUnique();

        builder.HasOne(d => d.Category)
            .WithMany()
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class UserExampleProgressConfiguration : IEntityTypeConfiguration<UserExampleProgress>
{
    public void Configure(EntityTypeBuilder<UserExampleProgress> builder)
    {
        builder.Property(t => t.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.HasIndex(x => new { x.UserId, x.ExampleId })
            .IsUnique();

        builder.HasOne(d => d.Example)
            .WithMany()
            .HasForeignKey(d => d.ExampleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
