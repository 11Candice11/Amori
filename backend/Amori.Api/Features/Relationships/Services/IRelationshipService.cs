using Amori.Api.Features.Relationships.DTOs;

namespace Amori.Api.Features.Relationships.Services;

/// <summary>
/// Business logic for creating and managing couple relationships.
/// </summary>
public interface IRelationshipService
{
    Task<RelationshipResponse> CreateAsync(Guid userId, CreateRelationshipRequest request, CancellationToken ct = default);
    Task<RelationshipResponse?> GetMyRelationshipAsync(Guid userId, CancellationToken ct = default);
    Task<RelationshipResponse> JoinAsync(Guid userId, Guid relationshipId, CancellationToken ct = default);
}
