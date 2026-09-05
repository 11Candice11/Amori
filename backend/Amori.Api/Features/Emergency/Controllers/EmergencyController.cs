using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Domain.Enums;
using Amori.Api.Infrastructure.Authentication;
using Amori.Api.Infrastructure.Relationships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.Emergency;

// ── DTOs ─────────────────────────────────────────────────────────────────────

public sealed class CreateEmergencyRequestDto
{
    public EmergencyRequestType Type { get; set; }
    public string? Message { get; set; }
}

public sealed class UpdateEmergencyRequestDto
{
    public string? Message { get; set; }
}

public sealed class EmergencyRequestResponse
{
    public Guid Id { get; init; }
    public Guid RelationshipId { get; init; }
    public Guid SenderId { get; init; }
    public string SenderName { get; init; } = string.Empty;
    public Guid RecipientId { get; init; }
    public string RecipientName { get; init; } = string.Empty;
    public EmergencyRequestType Type { get; init; }
    public EmergencyRequestStatus Status { get; init; }
    public string? Message { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? AcknowledgedAt { get; init; }
    public DateTime? ResolvedAt { get; init; }
}

// ── Controller ────────────────────────────────────────────────────────────────

/// <summary>
/// I'm-not-okay / support request flow.
/// NOT a medical emergency service — this is a communication and support tool.
/// </summary>
[ApiController]
[Route("api/emergency/requests")]
[Authorize]
public sealed class EmergencyController(
    AmoriDbContext db,
    ICurrentUserService currentUser,
    IRelationshipAccessService relAccess) : ControllerBase
{
    private Guid RequireUserId() =>
        currentUser.UserId ?? throw new UnauthorizedException();

    private static EmergencyRequestResponse Map(EmergencyRequest r) => new()
    {
        Id = r.Id,
        RelationshipId = r.RelationshipId,
        SenderId = r.SenderId,
        SenderName = r.Sender?.DisplayName ?? string.Empty,
        RecipientId = r.RecipientId,
        RecipientName = r.Recipient?.DisplayName ?? string.Empty,
        Type = r.Type,
        Status = r.Status,
        Message = r.Message,
        CreatedAt = r.CreatedAt,
        AcknowledgedAt = r.AcknowledgedAt,
        ResolvedAt = r.ResolvedAt
    };

    private async Task<EmergencyRequest> LoadAndAuthorizeAsync(Guid requestId, Guid userId, CancellationToken ct)
    {
        var relationship = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var req = await db.EmergencyRequests
            .Include(r => r.Sender)
            .Include(r => r.Recipient)
            .FirstOrDefaultAsync(r => r.Id == requestId, ct)
            ?? throw new NotFoundException("Emergency request", requestId);
        if (req.RelationshipId != relationship.Id) throw new UnauthorizedException();
        return req;
    }

    /// <summary>Create a support / I'm-not-okay request.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(EmergencyRequestResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateEmergencyRequestDto dto, CancellationToken ct)
    {
        var userId = RequireUserId();
        var relationship = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var partner = await relAccess.GetPartnerAsync(userId)
            ?? throw new ConflictException("Your relationship does not have a second partner yet.");

        var req = new EmergencyRequest
        {
            RelationshipId = relationship.Id,
            SenderId = userId,
            RecipientId = partner.Id,
            Type = dto.Type,
            Status = EmergencyRequestStatus.Active,
            Message = dto.Message
        };

        db.EmergencyRequests.Add(req);
        await db.SaveChangesAsync(ct);

        var sender = await db.Users.FindAsync([userId], ct);
        req.Sender = sender!;
        req.Recipient = partner;

        return CreatedAtAction(nameof(GetById), new { requestId = req.Id }, Map(req));
    }

    /// <summary>List all support requests in the authenticated user's relationship.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EmergencyRequestResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var userId = RequireUserId();
        var relationship = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");

        var list = await db.EmergencyRequests
            .Include(r => r.Sender)
            .Include(r => r.Recipient)
            .Where(r => r.RelationshipId == relationship.Id)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        return Ok(list.Select(Map).ToList());
    }

    /// <summary>Get a single support request by ID.</summary>
    [HttpGet("{requestId:guid}")]
    [ProducesResponseType(typeof(EmergencyRequestResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid requestId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var req = await LoadAndAuthorizeAsync(requestId, userId, ct);
        return Ok(Map(req));
    }

    /// <summary>Update the message on an active support request.</summary>
    [HttpPatch("{requestId:guid}")]
    [ProducesResponseType(typeof(EmergencyRequestResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid requestId, [FromBody] UpdateEmergencyRequestDto dto, CancellationToken ct)
    {
        var userId = RequireUserId();
        var req = await LoadAndAuthorizeAsync(requestId, userId, ct);
        if (req.SenderId != userId) throw new UnauthorizedException("Only the sender can update this request.");
        if (req.Status != EmergencyRequestStatus.Active)
            throw new ConflictException("Only active requests can be updated.");
        req.Message = dto.Message;
        await db.SaveChangesAsync(ct);
        return Ok(Map(req));
    }

    /// <summary>Acknowledge a support request (recipient only).</summary>
    [HttpPost("{requestId:guid}/acknowledge")]
    [ProducesResponseType(typeof(EmergencyRequestResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Acknowledge(Guid requestId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var req = await LoadAndAuthorizeAsync(requestId, userId, ct);
        if (req.RecipientId != userId) throw new UnauthorizedException("Only the recipient can acknowledge.");
        if (req.Status != EmergencyRequestStatus.Active)
            throw new ConflictException($"Cannot acknowledge a request in '{req.Status}' status.");
        req.Status = EmergencyRequestStatus.Acknowledged;
        req.AcknowledgedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Ok(Map(req));
    }

    /// <summary>Resolve a support request.</summary>
    [HttpPost("{requestId:guid}/resolve")]
    [ProducesResponseType(typeof(EmergencyRequestResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Resolve(Guid requestId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var req = await LoadAndAuthorizeAsync(requestId, userId, ct);
        if (req.Status is EmergencyRequestStatus.Resolved or EmergencyRequestStatus.Cancelled)
            throw new ConflictException($"Request is already '{req.Status}'.");
        req.Status = EmergencyRequestStatus.Resolved;
        req.ResolvedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Ok(Map(req));
    }

    /// <summary>Cancel a support request (sender only).</summary>
    [HttpPost("{requestId:guid}/cancel")]
    [ProducesResponseType(typeof(EmergencyRequestResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Cancel(Guid requestId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var req = await LoadAndAuthorizeAsync(requestId, userId, ct);
        if (req.SenderId != userId) throw new UnauthorizedException("Only the sender can cancel.");
        if (req.Status is EmergencyRequestStatus.Resolved or EmergencyRequestStatus.Cancelled)
            throw new ConflictException($"Request is already '{req.Status}'.");
        req.Status = EmergencyRequestStatus.Cancelled;
        await db.SaveChangesAsync(ct);
        return Ok(Map(req));
    }
}
