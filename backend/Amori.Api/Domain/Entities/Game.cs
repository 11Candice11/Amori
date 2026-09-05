using Amori.Api.Domain.Enums;

namespace Amori.Api.Domain.Entities;

public sealed class Game : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public GameType Type { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<GameSession> Sessions { get; set; } = [];
}
