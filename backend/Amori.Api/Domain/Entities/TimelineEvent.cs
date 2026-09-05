using Amori.Api.Domain.Enums;

namespace Amori.Api.Domain.Entities;

public sealed class TimelineEvent : BaseEntity
{
    public Guid RelationshipId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly EventDate { get; set; }
    public string? Location { get; set; }
    public TimelineEventType EventType { get; set; } = TimelineEventType.Custom;
    public List<string> PhotoKeys { get; set; } = [];

    // Navigation
    public Relationship Relationship { get; set; } = null!;
    public User CreatedBy { get; set; } = null!;
}
