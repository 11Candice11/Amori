namespace Amori.Api.Features.Calendar.DTOs;

public sealed class CalendarEventResponse
{
    public Guid Id { get; init; }
    public Guid RelationshipId { get; init; }
    public Guid CreatedByUserId { get; init; }
    public string CreatedByName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateOnly EventDate { get; init; }
    public TimeOnly? StartTime { get; init; }
    public TimeOnly? EndTime { get; init; }
    public string? Location { get; init; }
    public bool ReminderEnabled { get; init; }
    public int? ReminderMinutesBefore { get; init; }
    public bool IsCompleted { get; init; }
    public DateTime? CompletedAt { get; init; }
    public bool IsShared { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
