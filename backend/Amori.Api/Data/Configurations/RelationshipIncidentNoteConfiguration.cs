using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class RelationshipIncidentNoteConfiguration : IEntityTypeConfiguration<RelationshipIncidentNote>
{
    public void Configure(EntityTypeBuilder<RelationshipIncidentNote> builder)
    {
        builder.ToTable("relationship_incident_notes");

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).HasColumnName("id");
        builder.Property(n => n.IncidentId).HasColumnName("incident_id").IsRequired();
        builder.Property(n => n.AuthorUserId).HasColumnName("author_user_id").IsRequired();

        builder.Property(n => n.Content)
            .HasColumnName("content")
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(n => n.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(n => n.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(n => n.IncidentId).HasDatabaseName("IX_incident_notes_incident_id");

        builder.HasOne(n => n.Incident)
            .WithMany(i => i.Notes)
            .HasForeignKey(n => n.IncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.Author)
            .WithMany()
            .HasForeignKey(n => n.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
