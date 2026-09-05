using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class TimelineEventConfiguration : IEntityTypeConfiguration<TimelineEvent>
{
    public void Configure(EntityTypeBuilder<TimelineEvent> builder)
    {
        builder.ToTable("timeline_events");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.RelationshipId).HasColumnName("relationship_id").IsRequired();
        builder.Property(t => t.CreatedByUserId).HasColumnName("created_by_user_id").IsRequired();
        builder.Property(t => t.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(t => t.Description).HasColumnName("description").HasMaxLength(2000);
        builder.Property(t => t.EventDate).HasColumnName("event_date").IsRequired();
        builder.Property(t => t.Location).HasColumnName("location").HasMaxLength(300);
        builder.Property(t => t.EventType).HasColumnName("event_type").IsRequired();
        builder.Property(t => t.PhotoKeys).HasColumnName("photo_keys").HasColumnType("jsonb").IsRequired();
        builder.Property(t => t.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(t => t.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(t => t.RelationshipId).HasDatabaseName("IX_timeline_events_relationship_id");
        builder.HasIndex(t => t.EventDate).HasDatabaseName("IX_timeline_events_event_date");

        builder.HasOne(t => t.Relationship)
            .WithMany()
            .HasForeignKey(t => t.RelationshipId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.CreatedBy)
            .WithMany()
            .HasForeignKey(t => t.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
