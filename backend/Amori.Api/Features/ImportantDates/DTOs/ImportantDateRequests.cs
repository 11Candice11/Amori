namespace Amori.Api.Features.ImportantDates.DTOs;

public sealed class CreateImportantDateRequest
{
    public string Name { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public bool IsRecurring { get; set; }
    public bool ReminderEnabled { get; set; }
    public int? ReminderDaysBefore { get; set; }
    public string? Notes { get; set; }
    public string? ImageKey { get; set; }
}

public sealed class UpdateImportantDateRequest
{
    public string? Name { get; set; }
    public DateOnly? Date { get; set; }
    public bool? IsRecurring { get; set; }
    public bool? ReminderEnabled { get; set; }
    public int? ReminderDaysBefore { get; set; }
    public string? Notes { get; set; }
    public string? ImageKey { get; set; }
}
