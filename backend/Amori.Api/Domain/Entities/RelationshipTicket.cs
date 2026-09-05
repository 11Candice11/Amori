using Amori.Api.Domain.Enums;

namespace Amori.Api.Domain.Entities;

public sealed class RelationshipTicket : BaseEntity
{
    public Guid RelationshipId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public TicketCategory Category { get; set; }
    public TicketSeverity Severity { get; set; } = TicketSeverity.Medium;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public string? Description { get; set; }
    public string? Feelings { get; set; }
    public string? WhatHappened { get; set; }
    public string? WhatINeed { get; set; }
    public string? WhatIPreferInFuture { get; set; }
    public string? AdditionalNotes { get; set; }
    public DateTime? ResolvedAt { get; set; }

    // Navigation
    public Relationship Relationship { get; set; } = null!;
    public User CreatedBy { get; set; } = null!;
    public User? AssignedTo { get; set; }
    public ICollection<TicketResponse> Responses { get; set; } = [];
}
