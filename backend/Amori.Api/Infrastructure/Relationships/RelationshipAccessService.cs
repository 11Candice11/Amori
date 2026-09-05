using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Infrastructure.Relationships;

public sealed class RelationshipAccessService(AmoriDbContext dbContext) : IRelationshipAccessService
{
    public async Task<Relationship?> GetUserRelationshipAsync(Guid userId)
    {
        return await dbContext.RelationshipMembers
            .Where(rm => rm.UserId == userId)
            .Select(rm => rm.Relationship)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> IsUserMemberAsync(Guid userId, Guid relationshipId)
    {
        return await dbContext.RelationshipMembers
            .AnyAsync(rm => rm.UserId == userId && rm.RelationshipId == relationshipId);
    }

    public async Task<User?> GetPartnerAsync(Guid userId)
    {
        var relationship = await GetUserRelationshipAsync(userId);
        if (relationship == null)
            return null;

        return await dbContext.RelationshipMembers
            .Where(rm => rm.RelationshipId == relationship.Id && rm.UserId != userId)
            .Select(rm => rm.User)
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<User>> GetMembersAsync(Guid relationshipId)
    {
        return await dbContext.RelationshipMembers
            .Where(rm => rm.RelationshipId == relationshipId)
            .Select(rm => rm.User)
            .ToListAsync();
    }
}
