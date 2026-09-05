using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class RelationshipIncidentConfiguration : IEntityTypeConfiguration<RelationshipIncident>
{
    public void Configure(EntityTypeBuilder<RelationshipIncident> builder)
    {
        builder.ToTable("relationship_incidents");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");

        builder.Property(i => i.RelationshipId).HasColumnName("relationship_id").IsRequired();
        builder.Property(i => i.ReportedByUserId).HasColumnName("reported_by_user_id").IsRequired();
        builder.Property(i => i.AssignedToUserId).HasColumnName("assigned_to_user_id");

        builder.Property(i => i.Title)
            .HasColumnName("title")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(i => i.Description)
            .HasColumnName("description")
            .HasMaxLength(4000);

        builder.Property(i => i.Category)
            .HasColumnName("category")
            .IsRequired();

        builder.Property(i => i.SubCategory)
            .HasColumnName("sub_category")
            .HasMaxLength(100);

        builder.Property(i => i.Impact)
            .HasColumnName("impact")
            .IsRequired();

        builder.Property(i => i.Urgency)
            .HasColumnName("urgency")
            .IsRequired();

        builder.Property(i => i.Priority)
            .HasColumnName("priority")
            .IsRequired();

        builder.Property(i => i.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(i => i.Resolution)
            .HasColumnName("resolution")
            .HasMaxLength(4000);

        builder.Property(i => i.ResolutionNotes)
            .HasColumnName("resolution_notes")
            .HasMaxLength(4000);

        builder.Property(i => i.AssignedAt).HasColumnName("assigned_at");
        builder.Property(i => i.InvestigatedAt).HasColumnName("investigated_at");
        builder.Property(i => i.ResolvedAt).HasColumnName("resolved_at");
        builder.Property(i => i.ClosedAt).HasColumnName("closed_at");
        builder.Property(i => i.ReopenedAt).HasColumnName("reopened_at");
        builder.Property(i => i.DueAt).HasColumnName("due_at");
        builder.Property(i => i.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(i => i.UpdatedAt).HasColumnName("updated_at");

        // Indexes
        builder.HasIndex(i => i.RelationshipId).HasDatabaseName("IX_relationship_incidents_relationship_id");
        builder.HasIndex(i => i.Status).HasDatabaseName("IX_relationship_incidents_status");
        builder.HasIndex(i => i.Priority).HasDatabaseName("IX_relationship_incidents_priority");
        builder.HasIndex(i => i.Category).HasDatabaseName("IX_relationship_incidents_category");
        builder.HasIndex(i => i.ReportedByUserId).HasDatabaseName("IX_relationship_incidents_reported_by");
        builder.HasIndex(i => i.AssignedToUserId).HasDatabaseName("IX_relationship_incidents_assigned_to");
        builder.HasIndex(i => i.CreatedAt).HasDatabaseName("IX_relationship_incidents_created_at");
        builder.HasIndex(i => i.DueAt).HasDatabaseName("IX_relationship_incidents_due_at");

        // FK: Relationship (restrict delete — don't cascade-delete incidents when relationship is changed)
        builder.HasOne(i => i.Relationship)
            .WithMany()
            .HasForeignKey(i => i.RelationshipId)
            .OnDelete(DeleteBehavior.Restrict);

        // FK: ReportedBy user
        builder.HasOne(i => i.ReportedBy)
            .WithMany()
            .HasForeignKey(i => i.ReportedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // FK: AssignedTo user (nullable)
        builder.HasOne(i => i.AssignedTo)
            .WithMany()
            .HasForeignKey(i => i.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
