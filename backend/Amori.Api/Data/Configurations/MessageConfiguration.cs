using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("messages");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.RelationshipId).HasColumnName("relationship_id").IsRequired();
        builder.Property(m => m.SenderId).HasColumnName("sender_id").IsRequired();
        builder.Property(m => m.RecipientId).HasColumnName("recipient_id").IsRequired();
        builder.Property(m => m.Text).HasColumnName("text").HasMaxLength(4000);
        builder.Property(m => m.ImageKey).HasColumnName("image_key").HasMaxLength(500);
        builder.Property(m => m.VoiceNoteKey).HasColumnName("voice_note_key").HasMaxLength(500);
        builder.Property(m => m.Category).HasColumnName("category").IsRequired();
        builder.Property(m => m.ReadAt).HasColumnName("read_at");
        builder.Property(m => m.IsFavorite).HasColumnName("is_favorite").IsRequired();
        builder.Property(m => m.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(m => m.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(m => m.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(m => m.RelationshipId).HasDatabaseName("IX_messages_relationship_id");
        builder.HasIndex(m => m.RecipientId).HasDatabaseName("IX_messages_recipient_id");
        builder.HasIndex(m => m.CreatedAt).HasDatabaseName("IX_messages_created_at");
        builder.HasIndex(m => m.IsDeleted).HasDatabaseName("IX_messages_is_deleted");

        builder.HasOne(m => m.Relationship)
            .WithMany()
            .HasForeignKey(m => m.RelationshipId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Recipient)
            .WithMany()
            .HasForeignKey(m => m.RecipientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
