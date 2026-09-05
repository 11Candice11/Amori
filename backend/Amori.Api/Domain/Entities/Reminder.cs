using Amori.Api.Domain.Enums;

namespace Amori.Api.Domain.Entities;

public sealed class Reminder : BaseEntity
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public ReminderType Type { get; set; }
    public TimeOnly ReminderTime { get; set; }
    public RecurrenceType Recurrence { get; set; } = RecurrenceType.Daily;
    public DateOnly? OneTimeDate { get; set; }
    public bool IsEnabled { get; set; } = true;
    public DateTime? LastCompletedAt { get; set; }
    public DateTime? SnoozeUntil { get; set; }
    public DateTime? NextOccurrenceAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}
