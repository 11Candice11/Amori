using Amori.Api.Domain.Enums;

namespace Amori.Api.Domain.Entities;

public sealed class GameSession : BaseEntity
{
    public Guid GameId { get; set; }
    public Guid UserId { get; set; }
    public Guid RelationshipId { get; set; }
    public GameSessionStatus Status { get; set; } = GameSessionStatus.InProgress;
    public int? Score { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation
    public Game Game { get; set; } = null!;
    public User User { get; set; } = null!;
    public Relationship Relationship { get; set; } = null!;
}
