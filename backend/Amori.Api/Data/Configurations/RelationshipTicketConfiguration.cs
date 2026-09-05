using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class RelationshipTicketConfiguration : IEntityTypeConfiguration<RelationshipTicket>
{
    public void Configure(EntityTypeBuilder<RelationshipTicket> builder)
    {
        builder.ToTable("relationship_tickets");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.RelationshipId).HasColumnName("relationship_id").IsRequired();
        builder.Property(t => t.CreatedByUserId).HasColumnName("created_by_user_id").IsRequired();
        builder.Property(t => t.AssignedToUserId).HasColumnName("assigned_to_user_id");
        builder.Property(t => t.Subject).HasColumnName("subject").HasMaxLength(300).IsRequired();
        builder.Property(t => t.Category).HasColumnName("category").IsRequired();
        builder.Property(t => t.Severity).HasColumnName("severity").IsRequired();
        builder.Property(t => t.Status).HasColumnName("status").IsRequired();
        builder.Property(t => t.Description).HasColumnName("description").HasMaxLength(4000);
        builder.Property(t => t.Feelings).HasColumnName("feelings").HasMaxLength(2000);
        builder.Property(t => t.WhatHappened).HasColumnName("what_happened").HasMaxLength(2000);
        builder.Property(t => t.WhatINeed).HasColumnName("what_i_need").HasMaxLength(2000);
        builder.Property(t => t.WhatIPreferInFuture).HasColumnName("what_i_prefer_in_future").HasMaxLength(2000);
        builder.Property(t => t.AdditionalNotes).HasColumnName("additional_notes").HasMaxLength(2000);
        builder.Property(t => t.ResolvedAt).HasColumnName("resolved_at");
        builder.Property(t => t.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(t => t.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(t => t.RelationshipId).HasDatabaseName("IX_relationship_tickets_relationship_id");
        builder.HasIndex(t => t.Status).HasDatabaseName("IX_relationship_tickets_status");
        builder.HasIndex(t => t.CreatedAt).HasDatabaseName("IX_relationship_tickets_created_at");

        builder.HasOne(t => t.Relationship)
            .WithMany()
            .HasForeignKey(t => t.RelationshipId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.CreatedBy)
            .WithMany()
            .HasForeignKey(t => t.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.AssignedTo)
            .WithMany()
            .HasForeignKey(t => t.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasMany(t => t.Responses)
            .WithOne(r => r.Ticket)
            .HasForeignKey(r => r.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
