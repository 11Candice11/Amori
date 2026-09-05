using Amori.Api.Domain.Enums;

namespace Amori.Api.Features.Mood.Models;

/// <summary>
/// Internal model for aggregated mood statistics used by the summary calculation.
/// </summary>
public sealed class MoodAggregation
{
    public MoodType? LatestMood { get; init; }
    public int? LatestIntensity { get; init; }
    public DateTime? LatestCheckInAt { get; init; }
    public IDictionary<string, int> FrequencyByMood { get; init; } = new Dictionary<string, int>();
}
