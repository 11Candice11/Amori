using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amori.Api.Data.Configurations;

public sealed class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
{
    public void Configure(EntityTypeBuilder<UserSettings> builder)
    {
        builder.ToTable("user_settings");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(s => s.PushNotificationsEnabled).HasColumnName("push_notifications_enabled").IsRequired();
        builder.Property(s => s.MoodRemindersEnabled).HasColumnName("mood_reminders_enabled").IsRequired();
        builder.Property(s => s.PartnerActivityNotifications).HasColumnName("partner_activity_notifications").IsRequired();
        builder.Property(s => s.HugNotifications).HasColumnName("hug_notifications").IsRequired();
        builder.Property(s => s.EmergencyNotifications).HasColumnName("emergency_notifications").IsRequired();
        builder.Property(s => s.MessageNotifications).HasColumnName("message_notifications").IsRequired();
        builder.Property(s => s.ReminderNotifications).HasColumnName("reminder_notifications").IsRequired();
        builder.Property(s => s.ProfileVisible).HasColumnName("profile_visible").IsRequired();
        builder.Property(s => s.MoodShareDefault).HasColumnName("mood_share_default").IsRequired();
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(s => s.UserId).IsUnique().HasDatabaseName("IX_user_settings_user_id");

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
