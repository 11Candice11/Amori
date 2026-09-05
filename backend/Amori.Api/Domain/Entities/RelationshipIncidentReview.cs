namespace Amori.Api.Domain.Entities;

public sealed class RelationshipIncidentReview : BaseEntity
{
    public Guid IncidentId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string? WhatWentWell { get; set; }
    public string? WhatCouldImprove { get; set; }
    public string? FutureAction { get; set; }

    // Navigation
    public RelationshipIncident Incident { get; set; } = null!;
    public User CreatedBy { get; set; } = null!;
}
