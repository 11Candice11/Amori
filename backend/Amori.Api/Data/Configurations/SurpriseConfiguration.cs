using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class SurpriseConfiguration : IEntityTypeConfiguration<Surprise>
{
    public void Configure(EntityTypeBuilder<Surprise> builder)
    {
        builder.ToTable("surprises");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.RelationshipId).HasColumnName("relationship_id").IsRequired();
        builder.Property(s => s.CreatedByUserId).HasColumnName("created_by_user_id").IsRequired();
        builder.Property(s => s.RecipientUserId).HasColumnName("recipient_user_id").IsRequired();
        builder.Property(s => s.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(s => s.MessageText).HasColumnName("message_text").HasMaxLength(2000);
        builder.Property(s => s.ImageKey).HasColumnName("image_key").HasMaxLength(500);
        builder.Property(s => s.VoiceNoteKey).HasColumnName("voice_note_key").HasMaxLength(500);
        builder.Property(s => s.ScheduledDate).HasColumnName("scheduled_date");
        builder.Property(s => s.OpenedAt).HasColumnName("opened_at");
        builder.Property(s => s.IsFavorite).HasColumnName("is_favorite").IsRequired();
        builder.Property(s => s.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(s => s.RelationshipId).HasDatabaseName("IX_surprises_relationship_id");
        builder.HasIndex(s => s.RecipientUserId).HasDatabaseName("IX_surprises_recipient_user_id");
        builder.HasIndex(s => s.ScheduledDate).HasDatabaseName("IX_surprises_scheduled_date");

        builder.HasOne(s => s.Relationship)
            .WithMany()
            .HasForeignKey(s => s.RelationshipId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.CreatedBy)
            .WithMany()
            .HasForeignKey(s => s.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Recipient)
            .WithMany()
            .HasForeignKey(s => s.RecipientUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
