namespace Amori.Api.Features.ImportantDates.Models;

/// <summary>
/// Internal model for upcoming date calculation result.
/// </summary>
public sealed class UpcomingDateInfo
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateOnly Date { get; init; }
    public int DaysUntil { get; init; }
    public bool IsRecurring { get; init; }
}
