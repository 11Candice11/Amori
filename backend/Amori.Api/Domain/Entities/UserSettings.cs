namespace Amori.Api.Domain.Entities;

public sealed class UserSettings : BaseEntity
{
    public Guid UserId { get; set; }
    public bool PushNotificationsEnabled { get; set; } = true;
    public bool MoodRemindersEnabled { get; set; } = true;
    public bool PartnerActivityNotifications { get; set; } = true;
    public bool HugNotifications { get; set; } = true;
    public bool EmergencyNotifications { get; set; } = true;
    public bool MessageNotifications { get; set; } = true;
    public bool ReminderNotifications { get; set; } = true;
    public bool ProfileVisible { get; set; } = true;
    public bool MoodShareDefault { get; set; } = false;

    // Navigation
    public User User { get; set; } = null!;
}
