using Amori.Api.Domain.Enums;

namespace Amori.Api.Features.BucketList.Models;

/// <summary>
/// Internal model for bucket list completion statistics.
/// </summary>
public sealed class BucketListStats
{
    public int Total { get; init; }
    public int Completed { get; init; }
    public int Remaining { get; init; }
    public IDictionary<string, int> ByCategory { get; init; } = new Dictionary<string, int>();
}
