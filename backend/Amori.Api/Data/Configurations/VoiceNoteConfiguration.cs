using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class VoiceNoteConfiguration : IEntityTypeConfiguration<VoiceNote>
{
    public void Configure(EntityTypeBuilder<VoiceNote> builder)
    {
        builder.ToTable("voice_notes");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasColumnName("id");
        builder.Property(v => v.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(v => v.RelationshipId).HasColumnName("relationship_id").IsRequired();
        builder.Property(v => v.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(v => v.FileKey).HasColumnName("file_key").HasMaxLength(500).IsRequired();
        builder.Property(v => v.DurationSeconds).HasColumnName("duration_seconds").IsRequired();
        builder.Property(v => v.Category).HasColumnName("category").IsRequired();
        builder.Property(v => v.IsFavorite).HasColumnName("is_favorite").IsRequired();
        builder.Property(v => v.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(v => v.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(v => v.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(v => v.RelationshipId).HasDatabaseName("IX_voice_notes_relationship_id");
        builder.HasIndex(v => v.UserId).HasDatabaseName("IX_voice_notes_user_id");
        builder.HasIndex(v => v.IsDeleted).HasDatabaseName("IX_voice_notes_is_deleted");

        builder.HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Relationship)
            .WithMany()
            .HasForeignKey(v => v.RelationshipId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
