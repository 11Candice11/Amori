using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class ReminderCompletionConfiguration : IEntityTypeConfiguration<ReminderCompletion>
{
    public void Configure(EntityTypeBuilder<ReminderCompletion> builder)
    {
        builder.ToTable("reminder_completions");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.ReminderId).HasColumnName("reminder_id").IsRequired();
        builder.Property(c => c.CompletedByUserId).HasColumnName("completed_by_user_id").IsRequired();
        builder.Property(c => c.CompletedAt).HasColumnName("completed_at").IsRequired();
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(c => c.ReminderId).HasDatabaseName("IX_reminder_completions_reminder_id");
        builder.HasIndex(c => c.CompletedAt).HasDatabaseName("IX_reminder_completions_completed_at");

        builder.HasOne(c => c.Reminder)
            .WithMany(r => r.Completions)
            .HasForeignKey(c => c.ReminderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.CompletedBy)
            .WithMany()
            .HasForeignKey(c => c.CompletedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
