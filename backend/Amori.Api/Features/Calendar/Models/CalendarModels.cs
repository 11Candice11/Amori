namespace Amori.Api.Features.Calendar.Models;

/// <summary>
/// Internal model for a calendar month view used in aggregation.
/// </summary>
public sealed class CalendarMonthFilter
{
    public int Year { get; init; }
    public int Month { get; init; }
    public DateOnly From => new(Year, Month, 1);
    public DateOnly To => From.AddMonths(1);
}
