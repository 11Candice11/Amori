using Amori.Api.Domain.Enums;

namespace Amori.Api.Domain.Entities;

public sealed class RelationshipIncident : BaseEntity
{
    public Guid RelationshipId { get; set; }
    public Guid ReportedByUserId { get; set; }
    public Guid? AssignedToUserId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public IncidentCategory Category { get; set; }
    public string? SubCategory { get; set; }

    public IncidentImpact Impact { get; set; }
    public IncidentUrgency Urgency { get; set; }
    public IncidentPriority Priority { get; set; }

    public IncidentStatus Status { get; set; } = IncidentStatus.Open;

    public string? Resolution { get; set; }
    public string? ResolutionNotes { get; set; }

    public DateTime? AssignedAt { get; set; }
    public DateTime? InvestigatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime? ReopenedAt { get; set; }
    public DateTime? DueAt { get; set; }

    // Navigation
    public Relationship Relationship { get; set; } = null!;
    public User ReportedBy { get; set; } = null!;
    public User? AssignedTo { get; set; }

    public ICollection<RelationshipIncidentNote> Notes { get; set; } = [];
    public ICollection<RelationshipIncidentResponse> Responses { get; set; } = [];
    public ICollection<RelationshipIncidentHistory> History { get; set; } = [];
    public ICollection<RelationshipIncidentReview> Reviews { get; set; } = [];
    public ICollection<RelationshipIncidentLesson> Lessons { get; set; } = [];
}
