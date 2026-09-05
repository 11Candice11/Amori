namespace Amori.Api.Domain.Entities;

/// <summary>
/// Records each time a reminder was completed.
/// Allows recurring reminders to maintain a full completion history.
/// </summary>
public sealed class ReminderCompletion : BaseEntity
{
    public Guid ReminderId { get; set; }
    public Guid CompletedByUserId { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Reminder Reminder { get; set; } = null!;
    public User CompletedBy { get; set; } = null!;
}
