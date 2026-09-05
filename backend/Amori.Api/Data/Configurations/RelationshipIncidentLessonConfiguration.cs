using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class RelationshipIncidentLessonConfiguration : IEntityTypeConfiguration<RelationshipIncidentLesson>
{
    public void Configure(EntityTypeBuilder<RelationshipIncidentLesson> builder)
    {
        builder.ToTable("relationship_incident_lessons");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasColumnName("id");
        builder.Property(l => l.IncidentId).HasColumnName("incident_id").IsRequired();
        builder.Property(l => l.CreatedByUserId).HasColumnName("created_by_user_id").IsRequired();

        builder.Property(l => l.Lesson)
            .HasColumnName("lesson")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(l => l.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(l => l.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(l => l.IncidentId).HasDatabaseName("IX_incident_lessons_incident_id");

        builder.HasOne(l => l.Incident)
            .WithMany(i => i.Lessons)
            .HasForeignKey(l => l.IncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.CreatedBy)
            .WithMany()
            .HasForeignKey(l => l.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
