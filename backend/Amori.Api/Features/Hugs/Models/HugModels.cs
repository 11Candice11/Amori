namespace Amori.Api.Features.Hugs.Models;

/// <summary>
/// Internal model for hug statistics within a relationship.
/// </summary>
public sealed class HugStats
{
    public int TotalSent { get; init; }
    public int TotalReceived { get; init; }
    public int Unacknowledged { get; init; }
    public DateTime? LastHugAt { get; init; }
}
