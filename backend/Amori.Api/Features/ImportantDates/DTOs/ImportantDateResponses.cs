namespace Amori.Api.Features.ImportantDates.DTOs;

public sealed class ImportantDateResponse
{
    public Guid Id { get; init; }
    public Guid RelationshipId { get; init; }
    public Guid CreatedByUserId { get; init; }
    public string CreatedByName { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public DateOnly Date { get; init; }
    public bool IsRecurring { get; init; }
    public bool ReminderEnabled { get; init; }
    public int? ReminderDaysBefore { get; init; }
    public string? Notes { get; init; }
    public string? ImageKey { get; init; }
    public int? DaysUntilNext { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
