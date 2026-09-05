namespace Amori.Api.Domain.Entities;

public sealed class RelationshipIncidentNote : BaseEntity
{
    public Guid IncidentId { get; set; }
    public Guid AuthorUserId { get; set; }
    public string Content { get; set; } = string.Empty;

    // Navigation
    public RelationshipIncident Incident { get; set; } = null!;
    public User Author { get; set; } = null!;
}
