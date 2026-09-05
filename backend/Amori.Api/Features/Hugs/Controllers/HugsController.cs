using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Infrastructure.Authentication;
using Amori.Api.Infrastructure.Relationships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.Hugs;

// ── DTOs ─────────────────────────────────────────────────────────────────────

public sealed class HugResponse
{
    public Guid Id { get; init; }
    public Guid RelationshipId { get; init; }
    public Guid SenderId { get; init; }
    public string SenderName { get; init; } = string.Empty;
    public Guid RecipientId { get; init; }
    public string RecipientName { get; init; } = string.Empty;
    public bool IsAcknowledged { get; init; }
    public DateTime? AcknowledgedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

// ── Controller ────────────────────────────────────────────────────────────────

/// <summary>Send and receive virtual hugs between partners.</summary>
[ApiController]
[Route("api/hugs")]
[Authorize]
public sealed class HugsController(
    AmoriDbContext db,
    ICurrentUserService currentUser,
    IRelationshipAccessService relAccess) : ControllerBase
{
    private Guid RequireUserId() =>
        currentUser.UserId ?? throw new UnauthorizedException();

    private static HugResponse Map(Hug h) => new()
    {
        Id = h.Id,
        RelationshipId = h.RelationshipId,
        SenderId = h.SenderId,
        SenderName = h.Sender?.DisplayName ?? string.Empty,
        RecipientId = h.RecipientId,
        RecipientName = h.Recipient?.DisplayName ?? string.Empty,
        IsAcknowledged = h.AcknowledgedAt.HasValue,
        AcknowledgedAt = h.AcknowledgedAt,
        CreatedAt = h.CreatedAt
    };

    /// <summary>Send a hug to your partner.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(HugResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> SendHug(CancellationToken ct)
    {
        var userId = RequireUserId();
        var relationship = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");

        var partner = await relAccess.GetPartnerAsync(userId)
            ?? throw new ConflictException("Your relationship does not have a second partner yet.");

        var hug = new Hug
        {
            RelationshipId = relationship.Id,
            SenderId = userId,
            RecipientId = partner.Id
        };

        db.Hugs.Add(hug);
        await db.SaveChangesAsync(ct);

        // Re-load with navigation properties for mapping
        hug.Sender = (await db.Users.FindAsync([userId], ct))!;
        hug.Recipient = partner;

        return CreatedAtAction(nameof(GetHug), new { hugId = hug.Id }, Map(hug));
    }

    /// <summary>Get all hugs for the authenticated user's relationship.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<HugResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHugs(CancellationToken ct)
    {
        var userId = RequireUserId();
        var relationship = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");

        var hugs = await db.Hugs
            .Include(h => h.Sender)
            .Include(h => h.Recipient)
            .Where(h => h.RelationshipId == relationship.Id)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync(ct);

        return Ok(hugs.Select(Map).ToList());
    }

    /// <summary>Get a hug by ID.</summary>
    [HttpGet("{hugId:guid}")]
    [ProducesResponseType(typeof(HugResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHug(Guid hugId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var relationship = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");

        var hug = await db.Hugs
            .Include(h => h.Sender)
            .Include(h => h.Recipient)
            .FirstOrDefaultAsync(h => h.Id == hugId, ct)
            ?? throw new NotFoundException("Hug", hugId);

        if (hug.RelationshipId != relationship.Id) throw new UnauthorizedException();

        return Ok(Map(hug));
    }

    /// <summary>Acknowledge a received hug.</summary>
    [HttpPost("{hugId:guid}/acknowledge")]
    [ProducesResponseType(typeof(HugResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Acknowledge(Guid hugId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var relationship = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");

        var hug = await db.Hugs
            .Include(h => h.Sender)
            .Include(h => h.Recipient)
            .FirstOrDefaultAsync(h => h.Id == hugId, ct)
            ?? throw new NotFoundException("Hug", hugId);

        if (hug.RelationshipId != relationship.Id) throw new UnauthorizedException();
        if (hug.RecipientId != userId) throw new UnauthorizedException("Only the recipient can acknowledge a hug.");

        hug.AcknowledgedAt ??= DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return Ok(Map(hug));
    }
}
