using Amori.Api.Domain.Enums;

namespace Amori.Api.Features.Distraction.DTOs;

public sealed class GameResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public GameType Type { get; init; }
    public bool IsActive { get; init; }
}

public sealed class GameSessionResponse
{
    public Guid Id { get; init; }
    public Guid GameId { get; init; }
    public string GameTitle { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public Guid RelationshipId { get; init; }
    public GameSessionStatus Status { get; init; }
    public int? Score { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}
