namespace Amori.Api.Features.Hugs.DTOs;

public sealed class HugResponse
{
    public Guid Id { get; init; }
    public Guid RelationshipId { get; init; }
    public Guid SenderId { get; init; }
    public string SenderName { get; init; } = string.Empty;
    public Guid RecipientId { get; init; }
    public string RecipientName { get; init; } = string.Empty;
    public bool IsAcknowledged { get; init; }
    public DateTime? AcknowledgedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
