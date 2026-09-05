using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Domain.Enums;
using Amori.Api.Infrastructure.Relationships;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.Emergency.Services;

public sealed class EmergencyService(
    AmoriDbContext db,
    IRelationshipAccessService relAccess) : IEmergencyService
{
    private static EmergencyRequestResponse Map(EmergencyRequest r) => new()
    {
        Id = r.Id, RelationshipId = r.RelationshipId,
        SenderId = r.SenderId, SenderName = r.Sender?.DisplayName ?? string.Empty,
        RecipientId = r.RecipientId, RecipientName = r.Recipient?.DisplayName ?? string.Empty,
        Type = r.Type, Status = r.Status, Message = r.Message,
        CreatedAt = r.CreatedAt, AcknowledgedAt = r.AcknowledgedAt, ResolvedAt = r.ResolvedAt
    };

    private async Task<EmergencyRequest> LoadAsync(Guid userId, Guid requestId, CancellationToken ct)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var req = await db.EmergencyRequests
            .Include(r => r.Sender).Include(r => r.Recipient)
            .FirstOrDefaultAsync(r => r.Id == requestId, ct)
            ?? throw new NotFoundException("Emergency request", requestId);
        if (req.RelationshipId != rel.Id) throw new UnauthorizedException();
        return req;
    }

    public async Task<EmergencyRequestResponse> CreateAsync(Guid userId, CreateEmergencyRequestDto request, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var partner = await relAccess.GetPartnerAsync(userId)
            ?? throw new ConflictException("Your relationship does not have a second partner yet.");

        var req = new EmergencyRequest
        {
            RelationshipId = rel.Id, SenderId = userId, RecipientId = partner.Id,
            Type = request.Type, Status = EmergencyRequestStatus.Active, Message = request.Message
        };
        db.EmergencyRequests.Add(req);
        await db.SaveChangesAsync(ct);
        req.Sender = (await db.Users.FindAsync([userId], ct))!;
        req.Recipient = partner;
        return Map(req);
    }

    public async Task<IReadOnlyList<EmergencyRequestResponse>> GetAllAsync(Guid userId, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        return await db.EmergencyRequests
            .Include(r => r.Sender).Include(r => r.Recipient)
            .Where(r => r.RelationshipId == rel.Id)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => Map(r)).ToListAsync(ct);
    }

    public async Task<EmergencyRequestResponse> GetByIdAsync(Guid userId, Guid requestId, CancellationToken ct = default)
    {
        return Map(await LoadAsync(userId, requestId, ct));
    }

    public async Task<EmergencyRequestResponse> UpdateAsync(Guid userId, Guid requestId, UpdateEmergencyRequestDto request, CancellationToken ct = default)
    {
        var req = await LoadAsync(userId, requestId, ct);
        if (req.SenderId != userId) throw new UnauthorizedException("Only the sender can update this request.");
        if (req.Status != EmergencyRequestStatus.Active)
            throw new ConflictException("Only active requests can be updated.");
        req.Message = request.Message;
        await db.SaveChangesAsync(ct);
        return Map(req);
    }

    public async Task<EmergencyRequestResponse> AcknowledgeAsync(Guid userId, Guid requestId, CancellationToken ct = default)
    {
        var req = await LoadAsync(userId, requestId, ct);
        if (req.RecipientId != userId) throw new UnauthorizedException("Only the recipient can acknowledge.");
        if (req.Status != EmergencyRequestStatus.Active)
            throw new ConflictException($"Cannot acknowledge a request in '{req.Status}' status.");
        req.Status = EmergencyRequestStatus.Acknowledged;
        req.AcknowledgedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Map(req);
    }

    public async Task<EmergencyRequestResponse> ResolveAsync(Guid userId, Guid requestId, CancellationToken ct = default)
    {
        var req = await LoadAsync(userId, requestId, ct);
        if (req.Status is EmergencyRequestStatus.Resolved or EmergencyRequestStatus.Cancelled)
            throw new ConflictException($"Request is already '{req.Status}'.");
        req.Status = EmergencyRequestStatus.Resolved;
        req.ResolvedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Map(req);
    }

    public async Task<EmergencyRequestResponse> CancelAsync(Guid userId, Guid requestId, CancellationToken ct = default)
    {
        var req = await LoadAsync(userId, requestId, ct);
        if (req.SenderId != userId) throw new UnauthorizedException("Only the sender can cancel.");
        if (req.Status is EmergencyRequestStatus.Resolved or EmergencyRequestStatus.Cancelled)
            throw new ConflictException($"Request is already '{req.Status}'.");
        req.Status = EmergencyRequestStatus.Cancelled;
        await db.SaveChangesAsync(ct);
        return Map(req);
    }
}
