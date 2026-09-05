using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Infrastructure.Relationships;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.Hugs.Services;

public sealed class HugService(
    AmoriDbContext db,
    IRelationshipAccessService relAccess) : IHugService
{
    private static HugResponse Map(Hug h) => new()
    {
        Id = h.Id, RelationshipId = h.RelationshipId,
        SenderId = h.SenderId, SenderName = h.Sender?.DisplayName ?? string.Empty,
        RecipientId = h.RecipientId, RecipientName = h.Recipient?.DisplayName ?? string.Empty,
        IsAcknowledged = h.AcknowledgedAt.HasValue,
        AcknowledgedAt = h.AcknowledgedAt, CreatedAt = h.CreatedAt
    };

    public async Task<HugResponse> SendHugAsync(Guid userId, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var partner = await relAccess.GetPartnerAsync(userId)
            ?? throw new ConflictException("Your relationship does not have a second partner yet.");

        var hug = new Hug { RelationshipId = rel.Id, SenderId = userId, RecipientId = partner.Id };
        db.Hugs.Add(hug);
        await db.SaveChangesAsync(ct);
        hug.Sender = (await db.Users.FindAsync([userId], ct))!;
        hug.Recipient = partner;
        return Map(hug);
    }

    public async Task<IReadOnlyList<HugResponse>> GetHugsAsync(Guid userId, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        return await db.Hugs
            .Include(h => h.Sender).Include(h => h.Recipient)
            .Where(h => h.RelationshipId == rel.Id)
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => Map(h)).ToListAsync(ct);
    }

    public async Task<HugResponse> GetHugAsync(Guid userId, Guid hugId, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var hug = await db.Hugs
            .Include(h => h.Sender).Include(h => h.Recipient)
            .FirstOrDefaultAsync(h => h.Id == hugId, ct)
            ?? throw new NotFoundException("Hug", hugId);
        if (hug.RelationshipId != rel.Id) throw new UnauthorizedException();
        return Map(hug);
    }

    public async Task<HugResponse> AcknowledgeAsync(Guid userId, Guid hugId, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var hug = await db.Hugs
            .Include(h => h.Sender).Include(h => h.Recipient)
            .FirstOrDefaultAsync(h => h.Id == hugId, ct)
            ?? throw new NotFoundException("Hug", hugId);
        if (hug.RelationshipId != rel.Id) throw new UnauthorizedException();
        if (hug.RecipientId != userId) throw new UnauthorizedException("Only the recipient can acknowledge a hug.");
        hug.AcknowledgedAt ??= DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Map(hug);
    }
}
