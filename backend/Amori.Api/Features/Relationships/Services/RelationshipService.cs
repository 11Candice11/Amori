using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Domain.Enums;
using Amori.Api.Features.Relationships.DTOs;
using Amori.Api.Infrastructure.Relationships;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.Relationships.Services;

public sealed class RelationshipService(
    AmoriDbContext db,
    IRelationshipAccessService relAccess,
    ILogger<RelationshipService> logger) : IRelationshipService
{
    public async Task<RelationshipResponse> CreateAsync(Guid userId, CreateRelationshipRequest request, CancellationToken ct = default)
    {
        var user = await db.Users.FindAsync([userId], ct)
            ?? throw new NotFoundException("User", userId);

        if (await relAccess.GetUserRelationshipAsync(userId) != null)
            throw new ConflictException("User already belongs to a relationship.");

        var relationship = new Relationship
        {
            AnniversaryDate = request.StartDate,
            Status = RelationshipStatus.Active
        };

        var member = new RelationshipMember
        {
            RelationshipId = relationship.Id,
            UserId = userId,
            Role = RelationshipRole.Member,
            InviteStatus = MemberInviteStatus.Accepted
        };

        db.Relationships.Add(relationship);
        db.RelationshipMembers.Add(member);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Relationship {RelationshipId} created by user {UserId}", relationship.Id, userId);

        return MapToResponse(relationship, [user]);
    }

    public async Task<RelationshipResponse?> GetMyRelationshipAsync(Guid userId, CancellationToken ct = default)
    {
        var relationship = await relAccess.GetUserRelationshipAsync(userId);
        if (relationship == null) return null;

        var members = await relAccess.GetMembersAsync(relationship.Id);
        return MapToResponse(relationship, members);
    }

    public async Task<RelationshipResponse> JoinAsync(Guid userId, Guid relationshipId, CancellationToken ct = default)
    {
        var user = await db.Users.FindAsync([userId], ct)
            ?? throw new NotFoundException("User", userId);

        var relationship = await db.Relationships.FindAsync([relationshipId], ct)
            ?? throw new NotFoundException("Relationship", relationshipId);

        if (relationship.Status != RelationshipStatus.Active)
            throw new ConflictException("Relationship is not active.");

        if (await relAccess.IsUserMemberAsync(userId, relationshipId))
            throw new ConflictException("User is already a member of this relationship.");

        var memberCount = await db.RelationshipMembers.CountAsync(rm => rm.RelationshipId == relationshipId, ct);
        if (memberCount >= 2)
            throw new ConflictException("Relationship already has the maximum number of members.");

        if (await relAccess.GetUserRelationshipAsync(userId) != null)
            throw new ConflictException("User already belongs to a different relationship.");

        db.RelationshipMembers.Add(new RelationshipMember
        {
            RelationshipId = relationshipId,
            UserId = userId,
            Role = RelationshipRole.Member,
            InviteStatus = MemberInviteStatus.Accepted
        });

        await db.SaveChangesAsync(ct);

        logger.LogInformation("User {UserId} joined relationship {RelationshipId}", userId, relationshipId);

        var members = await relAccess.GetMembersAsync(relationshipId);
        return MapToResponse(relationship, members);
    }

    private static RelationshipResponse MapToResponse(Relationship r, IReadOnlyList<User> members) => new()
    {
        Id = r.Id,
        StartDate = r.AnniversaryDate,
        IsActive = r.Status == RelationshipStatus.Active,
        Members = members.Select(m => new RelationshipMemberResponse { Id = m.Id, Name = m.DisplayName }).ToList()
    };
}
