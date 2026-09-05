using Amori.Api.Domain.Entities;

namespace Amori.Api.Infrastructure.Relationships;

public interface IRelationshipAccessService
{
    /// <summary>
    /// Get the relationship that the user is a member of.
    /// Returns null if the user is not in a relationship.
    /// </summary>
    Task<Relationship?> GetUserRelationshipAsync(Guid userId);

    /// <summary>
    /// Check if a user is a member of a specific relationship.
    /// </summary>
    Task<bool> IsUserMemberAsync(Guid userId, Guid relationshipId);

    /// <summary>
    /// Get the partner user in a relationship (only supports 2-person relationships).
    /// Returns null if user is not in a relationship or has no partner yet.
    /// </summary>
    Task<User?> GetPartnerAsync(Guid userId);

    /// <summary>
    /// Get all members of a relationship.
    /// </summary>
    Task<IReadOnlyList<User>> GetMembersAsync(Guid relationshipId);
}
