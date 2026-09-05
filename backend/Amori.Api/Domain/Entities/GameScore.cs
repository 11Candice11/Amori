namespace Amori.Api.Domain.Entities;

/// <summary>
/// Leaderboard / high-score record for a game, scoped to a relationship.
/// </summary>
public sealed class GameScore : BaseEntity
{
    public Guid GameId { get; set; }
    public Guid RelationshipId { get; set; }
    public Guid UserId { get; set; }
    public int Score { get; set; }
    public DateTime PlayedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Game Game { get; set; } = null!;
    public Relationship Relationship { get; set; } = null!;
    public User User { get; set; } = null!;
}
