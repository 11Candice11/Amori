using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class HugConfiguration : IEntityTypeConfiguration<Hug>
{
    public void Configure(EntityTypeBuilder<Hug> builder)
    {
        builder.ToTable("hugs");

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).HasColumnName("id");
        builder.Property(h => h.RelationshipId).HasColumnName("relationship_id").IsRequired();
        builder.Property(h => h.SenderId).HasColumnName("sender_id").IsRequired();
        builder.Property(h => h.RecipientId).HasColumnName("recipient_id").IsRequired();
        builder.Property(h => h.AcknowledgedAt).HasColumnName("acknowledged_at");
        builder.Property(h => h.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(h => h.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(h => h.RelationshipId).HasDatabaseName("IX_hugs_relationship_id");
        builder.HasIndex(h => h.RecipientId).HasDatabaseName("IX_hugs_recipient_id");
        builder.HasIndex(h => h.CreatedAt).HasDatabaseName("IX_hugs_created_at");

        builder.HasOne(h => h.Relationship)
            .WithMany()
            .HasForeignKey(h => h.RelationshipId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.Sender)
            .WithMany()
            .HasForeignKey(h => h.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.Recipient)
            .WithMany()
            .HasForeignKey(h => h.RecipientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
