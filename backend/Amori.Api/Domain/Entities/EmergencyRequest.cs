using Amori.Api.Domain.Enums;

namespace Amori.Api.Domain.Entities;

public sealed class EmergencyRequest : BaseEntity
{
    public Guid RelationshipId { get; set; }
    public Guid SenderId { get; set; }
    public Guid RecipientId { get; set; }
    public EmergencyRequestType Type { get; set; }
    public EmergencyRequestStatus Status { get; set; } = EmergencyRequestStatus.Active;
    public string? Message { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    // Navigation
    public Relationship Relationship { get; set; } = null!;
    public User Sender { get; set; } = null!;
    public User Recipient { get; set; } = null!;
}
