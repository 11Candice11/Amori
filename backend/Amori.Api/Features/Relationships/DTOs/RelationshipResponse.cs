namespace Amori.Api.Features.Relationships.DTOs;

public sealed class RelationshipResponse
{
    public Guid Id { get; set; }
    public DateOnly? StartDate { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyList<RelationshipMemberResponse> Members { get; set; } = [];
}

public sealed class RelationshipMemberResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
