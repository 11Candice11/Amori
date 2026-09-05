using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class MemoryConfiguration : IEntityTypeConfiguration<Memory>
{
    public void Configure(EntityTypeBuilder<Memory> builder)
    {
        builder.ToTable("memories");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.RelationshipId).HasColumnName("relationship_id").IsRequired();
        builder.Property(m => m.CreatedByUserId).HasColumnName("created_by_user_id").IsRequired();
        builder.Property(m => m.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(m => m.Description).HasColumnName("description").HasMaxLength(4000);
        builder.Property(m => m.MemoryDate).HasColumnName("memory_date");
        builder.Property(m => m.Location).HasColumnName("location").HasMaxLength(300);
        builder.Property(m => m.Latitude).HasColumnName("latitude");
        builder.Property(m => m.Longitude).HasColumnName("longitude");
        builder.Property(m => m.Tags).HasColumnName("tags").HasColumnType("jsonb").IsRequired();
        builder.Property(m => m.IsFavorite).HasColumnName("is_favorite").IsRequired();
        builder.Property(m => m.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(m => m.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(m => m.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(m => m.RelationshipId).HasDatabaseName("IX_memories_relationship_id");
        builder.HasIndex(m => m.CreatedByUserId).HasDatabaseName("IX_memories_created_by_user_id");
        builder.HasIndex(m => m.IsDeleted).HasDatabaseName("IX_memories_is_deleted");

        builder.HasOne(m => m.Relationship)
            .WithMany()
            .HasForeignKey(m => m.RelationshipId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.CreatedBy)
            .WithMany()
            .HasForeignKey(m => m.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(m => m.Media)
            .WithOne(mm => mm.Memory)
            .HasForeignKey(mm => mm.MemoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
