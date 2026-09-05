namespace Amori.Api.Domain.Entities;

public sealed class RelationshipIncidentLesson : BaseEntity
{
    public Guid IncidentId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string Lesson { get; set; } = string.Empty;

    // Navigation
    public RelationshipIncident Incident { get; set; } = null!;
    public User CreatedBy { get; set; } = null!;
}
