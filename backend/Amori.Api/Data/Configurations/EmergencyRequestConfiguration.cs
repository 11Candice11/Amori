using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class EmergencyRequestConfiguration : IEntityTypeConfiguration<EmergencyRequest>
{
    public void Configure(EntityTypeBuilder<EmergencyRequest> builder)
    {
        builder.ToTable("emergency_requests");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.RelationshipId).HasColumnName("relationship_id").IsRequired();
        builder.Property(e => e.SenderId).HasColumnName("sender_id").IsRequired();
        builder.Property(e => e.RecipientId).HasColumnName("recipient_id").IsRequired();
        builder.Property(e => e.Type).HasColumnName("type").IsRequired();
        builder.Property(e => e.Status).HasColumnName("status").IsRequired();
        builder.Property(e => e.Message).HasColumnName("message").HasMaxLength(2000);
        builder.Property(e => e.AcknowledgedAt).HasColumnName("acknowledged_at");
        builder.Property(e => e.ResolvedAt).HasColumnName("resolved_at");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(e => e.RelationshipId).HasDatabaseName("IX_emergency_requests_relationship_id");
        builder.HasIndex(e => e.SenderId).HasDatabaseName("IX_emergency_requests_sender_id");
        builder.HasIndex(e => e.Status).HasDatabaseName("IX_emergency_requests_status");
        builder.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_emergency_requests_created_at");

        builder.HasOne(e => e.Relationship)
            .WithMany()
            .HasForeignKey(e => e.RelationshipId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Sender)
            .WithMany()
            .HasForeignKey(e => e.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Recipient)
            .WithMany()
            .HasForeignKey(e => e.RecipientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
