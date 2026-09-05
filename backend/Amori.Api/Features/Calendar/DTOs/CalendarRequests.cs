namespace Amori.Api.Features.Calendar.DTOs;

public sealed class CreateCalendarEventRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly EventDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string? Location { get; set; }
    public bool ReminderEnabled { get; set; }
    public int? ReminderMinutesBefore { get; set; }
    public bool IsShared { get; set; } = true;
}

public sealed class UpdateCalendarEventRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateOnly? EventDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string? Location { get; set; }
    public bool? ReminderEnabled { get; set; }
    public int? ReminderMinutesBefore { get; set; }
    public bool? IsShared { get; set; }
}
