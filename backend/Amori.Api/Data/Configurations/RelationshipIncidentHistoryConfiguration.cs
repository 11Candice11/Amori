using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class RelationshipIncidentHistoryConfiguration : IEntityTypeConfiguration<RelationshipIncidentHistory>
{
    public void Configure(EntityTypeBuilder<RelationshipIncidentHistory> builder)
    {
        builder.ToTable("relationship_incident_history");

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).HasColumnName("id");
        builder.Property(h => h.IncidentId).HasColumnName("incident_id").IsRequired();
        builder.Property(h => h.ActorUserId).HasColumnName("actor_user_id").IsRequired();

        builder.Property(h => h.Action)
            .HasColumnName("action")
            .IsRequired();

        builder.Property(h => h.OldValue)
            .HasColumnName("old_value")
            .HasMaxLength(1000);

        builder.Property(h => h.NewValue)
            .HasColumnName("new_value")
            .HasMaxLength(1000);

        builder.Property(h => h.CreatedAt).HasColumnName("created_at").IsRequired();

        // History is append-only — UpdatedAt not meaningful but inherits from BaseEntity
        builder.Property(h => h.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(h => h.IncidentId).HasDatabaseName("IX_incident_history_incident_id");
        builder.HasIndex(h => h.CreatedAt).HasDatabaseName("IX_incident_history_created_at");

        builder.HasOne(h => h.Incident)
            .WithMany(i => i.History)
            .HasForeignKey(h => h.IncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.Actor)
            .WithMany()
            .HasForeignKey(h => h.ActorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
