namespace Amori.Api.Features.Distraction.Models;

/// <summary>
/// Internal model for aggregated game session statistics.
/// </summary>
public sealed class GameSessionStats
{
    public Guid GameId { get; init; }
    public string GameTitle { get; init; } = string.Empty;
    public int SessionsPlayed { get; init; }
    public int? HighScore { get; init; }
    public DateTime? LastPlayedAt { get; init; }
}
