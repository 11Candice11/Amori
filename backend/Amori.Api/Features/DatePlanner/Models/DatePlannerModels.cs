using Amori.Api.Domain.Enums;

namespace Amori.Api.Features.DatePlanner.Models;

/// <summary>
/// Internal model for date idea recommendation logic.
/// </summary>
public sealed class DateIdeaFilter
{
    public DateCategory? Category { get; init; }
    public bool ExcludeCompleted { get; init; } = true;
    public bool FavoritesOnly { get; init; }
}
