using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Domain.Enums;
using Amori.Api.Features.Relationships.DTOs;
using Amori.Api.Infrastructure.Authentication;
using Amori.Api.Infrastructure.Relationships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.Relationships;

[ApiController]
[Route("api/relationships")]
[Authorize]
public sealed class RelationshipsController(
    AmoriDbContext dbContext,
    ICurrentUserService currentUserService,
    IRelationshipAccessService relationshipAccessService,
    ILogger<RelationshipsController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<RelationshipResponse>> CreateRelationship(CreateRelationshipRequest request)
    {
        var userId = currentUserService.UserId;
        if (userId == null)
            throw new UnauthorizedException("User not authenticated.");

        var user = await dbContext.Users.FindAsync(userId);
        if (user == null)
            throw new NotFoundException("User not found.");

        var existingRelationship = await relationshipAccessService.GetUserRelationshipAsync(userId.Value);
        if (existingRelationship != null)
            throw new ConflictException("User already belongs to a relationship.");

        var relationship = new Relationship
        {
            AnniversaryDate = request.StartDate,
            Status = RelationshipStatus.Active
        };

        var member = new RelationshipMember
        {
            RelationshipId = relationship.Id,
            UserId = userId.Value,
            Role = RelationshipRole.Member,
            InviteStatus = MemberInviteStatus.Accepted
        };

        dbContext.Relationships.Add(relationship);
        dbContext.RelationshipMembers.Add(member);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Relationship created: {RelationshipId} for user {UserId}", relationship.Id, userId);

        return CreatedAtAction(nameof(GetMyRelationship), new { }, MapToResponse(relationship, [user]));
    }

    [HttpGet("me")]
    public async Task<ActionResult<RelationshipResponse>> GetMyRelationship()
    {
        var userId = currentUserService.UserId;
        if (userId == null)
            throw new UnauthorizedException("User not authenticated.");

        var relationship = await relationshipAccessService.GetUserRelationshipAsync(userId.Value);
        if (relationship == null)
            return NotFound(new { message = "User does not belong to a relationship." });

        var members = await relationshipAccessService.GetMembersAsync(relationship.Id);

        return Ok(MapToResponse(relationship, members));
    }

    [HttpPost("{relationshipId:guid}/join")]
    public async Task<ActionResult<RelationshipResponse>> JoinRelationship(Guid relationshipId)
    {
        var userId = currentUserService.UserId;
        if (userId == null)
            throw new UnauthorizedException("User not authenticated.");

        var user = await dbContext.Users.FindAsync(userId);
        if (user == null)
            throw new NotFoundException("User not found.");

        var relationship = await dbContext.Relationships.FindAsync(relationshipId);
        if (relationship == null)
            throw new NotFoundException("Relationship not found.");

        if (relationship.Status != RelationshipStatus.Active)
            throw new ConflictException("Relationship is not active.");

        var alreadyMember = await relationshipAccessService.IsUserMemberAsync(userId.Value, relationshipId);
        if (alreadyMember)
            throw new ConflictException("User is already a member of this relationship.");

        var memberCount = await dbContext.RelationshipMembers
            .CountAsync(rm => rm.RelationshipId == relationshipId);

        if (memberCount >= 2)
            throw new ConflictException("Relationship already has the maximum number of members.");

        var existingRelationship = await relationshipAccessService.GetUserRelationshipAsync(userId.Value);
        if (existingRelationship != null)
            throw new ConflictException("User already belongs to a different relationship.");

        var newMember = new RelationshipMember
        {
            RelationshipId = relationshipId,
            UserId = userId.Value,
            Role = RelationshipRole.Member,
            InviteStatus = MemberInviteStatus.Accepted
        };

        dbContext.RelationshipMembers.Add(newMember);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("User {UserId} joined relationship {RelationshipId}", userId, relationshipId);

        var members = await relationshipAccessService.GetMembersAsync(relationshipId);

        return Ok(MapToResponse(relationship, members));
    }

    private static RelationshipResponse MapToResponse(Relationship relationship, IReadOnlyList<User> members)
    {
        return new RelationshipResponse
        {
            Id = relationship.Id,
            StartDate = relationship.AnniversaryDate,
            IsActive = relationship.Status == RelationshipStatus.Active,
            Members = members
                .Select(m => new RelationshipMemberResponse
                {
                    Id = m.Id,
                    Name = m.DisplayName
                })
                .ToList()
        };
    }
}
