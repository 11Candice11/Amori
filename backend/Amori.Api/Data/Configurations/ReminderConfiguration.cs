using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class ReminderConfiguration : IEntityTypeConfiguration<Reminder>
{
    public void Configure(EntityTypeBuilder<Reminder> builder)
    {
        builder.ToTable("reminders");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(r => r.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(r => r.Notes).HasColumnName("notes").HasMaxLength(1000);
        builder.Property(r => r.Type).HasColumnName("type").IsRequired();
        builder.Property(r => r.ReminderTime).HasColumnName("reminder_time").IsRequired();
        builder.Property(r => r.Recurrence).HasColumnName("recurrence").IsRequired();
        builder.Property(r => r.OneTimeDate).HasColumnName("one_time_date");
        builder.Property(r => r.IsEnabled).HasColumnName("is_enabled").IsRequired();
        builder.Property(r => r.LastCompletedAt).HasColumnName("last_completed_at");
        builder.Property(r => r.SnoozeUntil).HasColumnName("snooze_until");
        builder.Property(r => r.NextOccurrenceAt).HasColumnName("next_occurrence_at");
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(r => r.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(r => r.UserId).HasDatabaseName("IX_reminders_user_id");
        builder.HasIndex(r => r.IsEnabled).HasDatabaseName("IX_reminders_is_enabled");
        builder.HasIndex(r => r.NextOccurrenceAt).HasDatabaseName("IX_reminders_next_occurrence_at");

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Completions)
            .WithOne(c => c.Reminder)
            .HasForeignKey(c => c.ReminderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
