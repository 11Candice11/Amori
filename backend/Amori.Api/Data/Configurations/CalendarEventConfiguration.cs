using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class CalendarEventConfiguration : IEntityTypeConfiguration<CalendarEvent>
{
    public void Configure(EntityTypeBuilder<CalendarEvent> builder)
    {
        builder.ToTable("calendar_events");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.RelationshipId).HasColumnName("relationship_id").IsRequired();
        builder.Property(c => c.CreatedByUserId).HasColumnName("created_by_user_id").IsRequired();
        builder.Property(c => c.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(c => c.Description).HasColumnName("description").HasMaxLength(2000);
        builder.Property(c => c.EventDate).HasColumnName("event_date").IsRequired();
        builder.Property(c => c.StartTime).HasColumnName("start_time");
        builder.Property(c => c.EndTime).HasColumnName("end_time");
        builder.Property(c => c.Location).HasColumnName("location").HasMaxLength(300);
        builder.Property(c => c.ReminderEnabled).HasColumnName("reminder_enabled").IsRequired();
        builder.Property(c => c.ReminderMinutesBefore).HasColumnName("reminder_minutes_before");
        builder.Property(c => c.IsCompleted).HasColumnName("is_completed").IsRequired();
        builder.Property(c => c.CompletedAt).HasColumnName("completed_at");
        builder.Property(c => c.IsShared).HasColumnName("is_shared").IsRequired();
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(c => c.RelationshipId).HasDatabaseName("IX_calendar_events_relationship_id");
        builder.HasIndex(c => c.EventDate).HasDatabaseName("IX_calendar_events_event_date");

        builder.HasOne(c => c.Relationship)
            .WithMany()
            .HasForeignKey(c => c.RelationshipId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.CreatedBy)
            .WithMany()
            .HasForeignKey(c => c.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
