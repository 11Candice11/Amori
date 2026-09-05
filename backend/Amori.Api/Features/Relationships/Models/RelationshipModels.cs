namespace Amori.Api.Features.Relationships.Models;

/// <summary>
/// Internal model for relationship membership validation.
/// </summary>
public sealed class RelationshipMembershipInfo
{
    public Guid RelationshipId { get; init; }
    public Guid UserId { get; init; }
    public bool IsActive { get; init; }
    public int MemberCount { get; init; }
}
