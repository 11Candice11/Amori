using Amori.Api.Domain.Enums;

namespace Amori.Api.Domain.Entities;

public sealed class RelationshipIncidentHistory : BaseEntity
{
    public Guid IncidentId { get; set; }
    public Guid ActorUserId { get; set; }
    public IncidentHistoryAction Action { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    // Navigation
    public RelationshipIncident Incident { get; set; } = null!;
    public User Actor { get; set; } = null!;
}
