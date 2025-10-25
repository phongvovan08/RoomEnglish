using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoomEnglish.Domain.Entities;

namespace RoomEnglish.Infrastructure.Data.Configurations;

public class AudioCacheConfiguration : IEntityTypeConfiguration<AudioCache>
{
    public void Configure(EntityTypeBuilder<AudioCache> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Text)
            .IsRequired()
            .HasMaxLength(2000); // Limit text length

        builder.Property(x => x.Voice)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Rate)
            .IsRequired();

        builder.Property(x => x.Provider)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.AudioData)
            .IsRequired();

        builder.Property(x => x.MimeType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.SizeBytes)
            .IsRequired();

        builder.Property(x => x.CacheKeyHash)
            .IsRequired()
            .HasMaxLength(64); // SHA256 hash

        builder.Property(x => x.HitCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.LastAccessedAt)
            .IsRequired();

        // Unique index on cache key hash for fast lookups
        builder.HasIndex(x => x.CacheKeyHash)
            .IsUnique()
            .HasDatabaseName("IX_AudioCache_CacheKeyHash");

        // Index for cleanup queries (find expired entries)
        builder.HasIndex(x => x.ExpiresAt)
            .HasDatabaseName("IX_AudioCache_ExpiresAt");

        // Index for finding least used entries (for cache eviction)
        builder.HasIndex(x => new { x.HitCount, x.LastAccessedAt })
            .HasDatabaseName("IX_AudioCache_HitCount_LastAccessedAt");
    }
}
