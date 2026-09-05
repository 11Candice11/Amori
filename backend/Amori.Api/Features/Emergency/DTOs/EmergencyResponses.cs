using Amori.Api.Domain.Enums;

namespace Amori.Api.Features.Emergency.DTOs;

public sealed class EmergencyRequestResponse
{
    public Guid Id { get; init; }
    public Guid RelationshipId { get; init; }
    public Guid SenderId { get; init; }
    public string SenderName { get; init; } = string.Empty;
    public Guid RecipientId { get; init; }
    public string RecipientName { get; init; } = string.Empty;
    public EmergencyRequestType Type { get; init; }
    public EmergencyRequestStatus Status { get; init; }
    public string? Message { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? AcknowledgedAt { get; init; }
    public DateTime? ResolvedAt { get; init; }
}
