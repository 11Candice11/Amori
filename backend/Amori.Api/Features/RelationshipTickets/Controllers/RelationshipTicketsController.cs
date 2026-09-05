using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Domain.Enums;
using Amori.Api.Infrastructure.Authentication;
using Amori.Api.Infrastructure.Relationships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.RelationshipTickets.Controllers;

public sealed class CreateTicketRequest
{
    public string Subject { get; set; } = string.Empty;
    public TicketCategory Category { get; set; }
    public TicketSeverity Severity { get; set; } = TicketSeverity.Medium;
    public string? Description { get; set; }
    public string? Feelings { get; set; }
    public string? WhatHappened { get; set; }
    public string? WhatINeed { get; set; }
    public string? WhatIPreferInFuture { get; set; }
    public string? AdditionalNotes { get; set; }
}

public sealed class UpdateTicketRequest
{
    public string? Subject { get; set; }
    public TicketCategory? Category { get; set; }
    public TicketSeverity? Severity { get; set; }
    public string? Description { get; set; }
    public string? Feelings { get; set; }
    public string? WhatHappened { get; set; }
    public string? WhatINeed { get; set; }
    public string? WhatIPreferInFuture { get; set; }
    public string? AdditionalNotes { get; set; }
}

public sealed class AddTicketResponseRequest
{
    public string Content { get; set; } = string.Empty;
}

public sealed class AssignTicketRequest
{
    public Guid AssignedToUserId { get; set; }
}

public sealed class TicketResponseDto
{
    public Guid Id { get; init; }
    public Guid TicketId { get; init; }
    public Guid RespondedByUserId { get; init; }
    public string RespondedByName { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public sealed class TicketDto
{
    public Guid Id { get; init; }
    public Guid RelationshipId { get; init; }
    public Guid CreatedByUserId { get; init; }
    public string CreatedByName { get; init; } = string.Empty;
    public Guid? AssignedToUserId { get; init; }
    public string? AssignedToName { get; init; }
    public string Subject { get; init; } = string.Empty;
    public TicketCategory Category { get; init; }
    public TicketSeverity Severity { get; init; }
    public TicketStatus Status { get; init; }
    public string? Description { get; init; }
    public string? Feelings { get; init; }
    public string? WhatHappened { get; init; }
    public string? WhatINeed { get; init; }
    public string? WhatIPreferInFuture { get; init; }
    public string? AdditionalNotes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? ResolvedAt { get; init; }
    public IReadOnlyList<TicketResponseDto> Responses { get; init; } = [];
}

/// <summary>Let's Chat — relationship communication ticket system.</summary>
[ApiController]
[Route("api/tickets")]
[Authorize]
public sealed class RelationshipTicketsController(
    AmoriDbContext db,
    ICurrentUserService currentUser,
    IRelationshipAccessService relAccess) : ControllerBase
{
    private Guid RequireUserId() =>
        currentUser.UserId ?? throw new UnauthorizedException();

    private static TicketDto Map(RelationshipTicket t) => new()
    {
        Id = t.Id, RelationshipId = t.RelationshipId,
        CreatedByUserId = t.CreatedByUserId, CreatedByName = t.CreatedBy?.DisplayName ?? string.Empty,
        AssignedToUserId = t.AssignedToUserId, AssignedToName = t.AssignedTo?.DisplayName,
        Subject = t.Subject, Category = t.Category, Severity = t.Severity, Status = t.Status,
        Description = t.Description, Feelings = t.Feelings, WhatHappened = t.WhatHappened,
        WhatINeed = t.WhatINeed, WhatIPreferInFuture = t.WhatIPreferInFuture,
        AdditionalNotes = t.AdditionalNotes,
        CreatedAt = t.CreatedAt, UpdatedAt = t.UpdatedAt, ResolvedAt = t.ResolvedAt,
        Responses = t.Responses.Select(r => new TicketResponseDto
        {
            Id = r.Id, TicketId = r.TicketId,
            RespondedByUserId = r.RespondedByUserId,
            RespondedByName = r.RespondedBy?.DisplayName ?? string.Empty,
            Content = r.Content, CreatedAt = r.CreatedAt, UpdatedAt = r.UpdatedAt
        }).OrderBy(r => r.CreatedAt).ToList()
    };

    private async Task<(Relationship rel, RelationshipTicket ticket)> LoadAsync(Guid ticketId, Guid userId, CancellationToken ct)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var ticket = await db.RelationshipTickets
            .Include(t => t.CreatedBy).Include(t => t.AssignedTo)
            .Include(t => t.Responses).ThenInclude(r => r.RespondedBy)
            .FirstOrDefaultAsync(t => t.Id == ticketId, ct)
            ?? throw new NotFoundException("Ticket", ticketId);
        if (ticket.RelationshipId != rel.Id) throw new UnauthorizedException();
        return (rel, ticket);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] TicketStatus? status, CancellationToken ct)
    {
        var userId = RequireUserId();
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var query = db.RelationshipTickets
            .Include(t => t.CreatedBy).Include(t => t.AssignedTo)
            .Include(t => t.Responses).ThenInclude(r => r.RespondedBy)
            .Where(t => t.RelationshipId == rel.Id);
        if (status.HasValue) query = query.Where(t => t.Status == status.Value);
        var tickets = await query.OrderByDescending(t => t.CreatedAt).ToListAsync(ct);
        return Ok(tickets.Select(Map));
    }

    [HttpGet("{ticketId:guid}")]
    public async Task<IActionResult> GetById(Guid ticketId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, ticket) = await LoadAsync(ticketId, userId, ct);
        return Ok(Map(ticket));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTicketRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        if (string.IsNullOrWhiteSpace(req.Subject)) throw new ValidationException("Subject is required.");

        var ticket = new RelationshipTicket
        {
            RelationshipId = rel.Id, CreatedByUserId = userId,
            Subject = req.Subject.Trim(), Category = req.Category, Severity = req.Severity,
            Status = TicketStatus.Open, Description = req.Description,
            Feelings = req.Feelings, WhatHappened = req.WhatHappened,
            WhatINeed = req.WhatINeed, WhatIPreferInFuture = req.WhatIPreferInFuture,
            AdditionalNotes = req.AdditionalNotes
        };
        db.RelationshipTickets.Add(ticket);
        await db.SaveChangesAsync(ct);
        ticket.CreatedBy = (await db.Users.FindAsync([userId], ct))!;
        return CreatedAtAction(nameof(GetById), new { ticketId = ticket.Id }, Map(ticket));
    }

    [HttpPatch("{ticketId:guid}")]
    public async Task<IActionResult> Update(Guid ticketId, [FromBody] UpdateTicketRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, ticket) = await LoadAsync(ticketId, userId, ct);
        if (ticket.CreatedByUserId != userId) throw new UnauthorizedException("Only the creator can update this ticket.");
        if (req.Subject != null) ticket.Subject = req.Subject.Trim();
        if (req.Category.HasValue) ticket.Category = req.Category.Value;
        if (req.Severity.HasValue) ticket.Severity = req.Severity.Value;
        if (req.Description != null) ticket.Description = req.Description;
        if (req.Feelings != null) ticket.Feelings = req.Feelings;
        if (req.WhatHappened != null) ticket.WhatHappened = req.WhatHappened;
        if (req.WhatINeed != null) ticket.WhatINeed = req.WhatINeed;
        if (req.WhatIPreferInFuture != null) ticket.WhatIPreferInFuture = req.WhatIPreferInFuture;
        if (req.AdditionalNotes != null) ticket.AdditionalNotes = req.AdditionalNotes;
        await db.SaveChangesAsync(ct);
        return Ok(Map(ticket));
    }

    [HttpDelete("{ticketId:guid}")]
    public async Task<IActionResult> Delete(Guid ticketId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, ticket) = await LoadAsync(ticketId, userId, ct);
        if (ticket.CreatedByUserId != userId) throw new UnauthorizedException("Only the creator can delete this ticket.");
        db.RelationshipTickets.Remove(ticket);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("{ticketId:guid}/acknowledge")]
    public async Task<IActionResult> Acknowledge(Guid ticketId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, ticket) = await LoadAsync(ticketId, userId, ct);
        ticket.Status = TicketStatus.Acknowledged;
        await db.SaveChangesAsync(ct);
        return Ok(Map(ticket));
    }

    [HttpPost("{ticketId:guid}/assign")]
    public async Task<IActionResult> Assign(Guid ticketId, [FromBody] AssignTicketRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (rel, ticket) = await LoadAsync(ticketId, userId, ct);
        var isMember = await relAccess.IsUserMemberAsync(req.AssignedToUserId, rel.Id);
        if (!isMember) throw new UnauthorizedException("Can only assign to a member of your relationship.");
        ticket.AssignedToUserId = req.AssignedToUserId;
        ticket.Status = TicketStatus.InProgress;
        ticket.AssignedTo = await db.Users.FindAsync([req.AssignedToUserId], ct);
        await db.SaveChangesAsync(ct);
        return Ok(Map(ticket));
    }

    [HttpPost("{ticketId:guid}/respond")]
    public async Task<IActionResult> Respond(Guid ticketId, [FromBody] AddTicketResponseRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, ticket) = await LoadAsync(ticketId, userId, ct);
        if (string.IsNullOrWhiteSpace(req.Content)) throw new ValidationException("Content is required.");

        var response = new TicketResponse
        {
            TicketId = ticket.Id, RespondedByUserId = userId, Content = req.Content
        };
        db.TicketResponses.Add(response);
        if (ticket.Status == TicketStatus.Open) ticket.Status = TicketStatus.InProgress;
        await db.SaveChangesAsync(ct);
        return Ok(Map(ticket));
    }

    [HttpGet("{ticketId:guid}/responses")]
    public async Task<IActionResult> GetResponses(Guid ticketId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, ticket) = await LoadAsync(ticketId, userId, ct);
        return Ok(ticket.Responses.Select(r => new TicketResponseDto
        {
            Id = r.Id, TicketId = r.TicketId, RespondedByUserId = r.RespondedByUserId,
            RespondedByName = r.RespondedBy?.DisplayName ?? string.Empty,
            Content = r.Content, CreatedAt = r.CreatedAt, UpdatedAt = r.UpdatedAt
        }).OrderBy(r => r.CreatedAt));
    }

    [HttpPost("{ticketId:guid}/status")]
    public async Task<IActionResult> SetStatus(Guid ticketId, [FromBody] TicketStatus status, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, ticket) = await LoadAsync(ticketId, userId, ct);
        ticket.Status = status;
        await db.SaveChangesAsync(ct);
        return Ok(Map(ticket));
    }

    [HttpPost("{ticketId:guid}/resolve")]
    public async Task<IActionResult> Resolve(Guid ticketId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, ticket) = await LoadAsync(ticketId, userId, ct);
        ticket.Status = TicketStatus.Resolved;
        ticket.ResolvedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Ok(Map(ticket));
    }

    [HttpPost("{ticketId:guid}/reopen")]
    public async Task<IActionResult> Reopen(Guid ticketId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, ticket) = await LoadAsync(ticketId, userId, ct);
        ticket.Status = TicketStatus.Open;
        ticket.ResolvedAt = null;
        await db.SaveChangesAsync(ct);
        return Ok(Map(ticket));
    }
}
