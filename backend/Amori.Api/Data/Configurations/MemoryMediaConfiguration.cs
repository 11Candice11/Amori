using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class MemoryMediaConfiguration : IEntityTypeConfiguration<MemoryMedia>
{
    public void Configure(EntityTypeBuilder<MemoryMedia> builder)
    {
        builder.ToTable("memory_media");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.MemoryId).HasColumnName("memory_id").IsRequired();
        builder.Property(m => m.FileKey).HasColumnName("file_key").HasMaxLength(500).IsRequired();
        builder.Property(m => m.MediaType).HasColumnName("media_type").IsRequired();
        builder.Property(m => m.DurationSeconds).HasColumnName("duration_seconds");
        builder.Property(m => m.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(m => m.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(m => m.MemoryId).HasDatabaseName("IX_memory_media_memory_id");

        builder.HasOne(m => m.Memory)
            .WithMany(mem => mem.Media)
            .HasForeignKey(m => m.MemoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
