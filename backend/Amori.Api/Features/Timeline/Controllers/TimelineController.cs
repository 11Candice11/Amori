using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Domain.Enums;
using Amori.Api.Infrastructure.Authentication;
using Amori.Api.Infrastructure.Relationships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.Timeline;

// ── DTOs ─────────────────────────────────────────────────────────────────────

public sealed class CreateTimelineEventRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly EventDate { get; set; }
    public string? Location { get; set; }
    public TimelineEventType EventType { get; set; } = TimelineEventType.Custom;
    public List<string> PhotoKeys { get; set; } = [];
}

public sealed class UpdateTimelineEventRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateOnly? EventDate { get; set; }
    public string? Location { get; set; }
    public TimelineEventType? EventType { get; set; }
}

public sealed class AddTimelineMediaRequest
{
    public string PhotoKey { get; set; } = string.Empty;
}

public sealed class TimelineEventResponse
{
    public Guid Id { get; init; }
    public Guid RelationshipId { get; init; }
    public Guid CreatedByUserId { get; init; }
    public string CreatedByName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateOnly EventDate { get; init; }
    public string? Location { get; init; }
    public TimelineEventType EventType { get; init; }
    public List<string> PhotoKeys { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

// ── Controller ────────────────────────────────────────────────────────────────

/// <summary>Relationship timeline events (first date, anniversary, milestones).</summary>
[ApiController]
[Route("api/timeline")]
[Authorize]
public sealed class TimelineController(
    AmoriDbContext db,
    ICurrentUserService currentUser,
    IRelationshipAccessService relAccess) : ControllerBase
{
    private Guid RequireUserId() =>
        currentUser.UserId ?? throw new UnauthorizedException();

    private static TimelineEventResponse Map(TimelineEvent e) => new()
    {
        Id = e.Id,
        RelationshipId = e.RelationshipId,
        CreatedByUserId = e.CreatedByUserId,
        CreatedByName = e.CreatedBy?.DisplayName ?? string.Empty,
        Title = e.Title,
        Description = e.Description,
        EventDate = e.EventDate,
        Location = e.Location,
        EventType = e.EventType,
        PhotoKeys = e.PhotoKeys,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };

    private async Task<TimelineEvent> LoadAndAuthorizeAsync(Guid eventId, Guid userId, CancellationToken ct)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var ev = await db.TimelineEvents
            .Include(e => e.CreatedBy)
            .FirstOrDefaultAsync(e => e.Id == eventId, ct)
            ?? throw new NotFoundException("Timeline event", eventId);
        if (ev.RelationshipId != rel.Id) throw new UnauthorizedException();
        return ev;
    }

    /// <summary>Get all timeline events for the relationship, ordered by event date.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var userId = RequireUserId();
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");

        var events = await db.TimelineEvents
            .Include(e => e.CreatedBy)
            .Where(e => e.RelationshipId == rel.Id)
            .OrderBy(e => e.EventDate)
            .ToListAsync(ct);

        return Ok(events.Select(Map).ToList());
    }

    [HttpGet("{eventId:guid}")]
    public async Task<IActionResult> GetById(Guid eventId, CancellationToken ct)
    {
        var userId = RequireUserId();
        return Ok(Map(await LoadAndAuthorizeAsync(eventId, userId, ct)));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTimelineEventRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");

        if (string.IsNullOrWhiteSpace(req.Title)) throw new ValidationException("Title is required.");

        var ev = new TimelineEvent
        {
            RelationshipId = rel.Id,
            CreatedByUserId = userId,
            Title = req.Title.Trim(),
            Description = req.Description,
            EventDate = req.EventDate,
            Location = req.Location,
            EventType = req.EventType,
            PhotoKeys = req.PhotoKeys
        };

        db.TimelineEvents.Add(ev);
        await db.SaveChangesAsync(ct);
        ev.CreatedBy = (await db.Users.FindAsync([userId], ct))!;
        return CreatedAtAction(nameof(GetById), new { eventId = ev.Id }, Map(ev));
    }

    [HttpPatch("{eventId:guid}")]
    public async Task<IActionResult> Update(Guid eventId, [FromBody] UpdateTimelineEventRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var ev = await LoadAndAuthorizeAsync(eventId, userId, ct);
        if (req.Title != null) ev.Title = req.Title.Trim();
        if (req.Description != null) ev.Description = req.Description;
        if (req.EventDate.HasValue) ev.EventDate = req.EventDate.Value;
        if (req.Location != null) ev.Location = req.Location;
        if (req.EventType.HasValue) ev.EventType = req.EventType.Value;
        await db.SaveChangesAsync(ct);
        return Ok(Map(ev));
    }

    [HttpDelete("{eventId:guid}")]
    public async Task<IActionResult> Delete(Guid eventId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var ev = await LoadAndAuthorizeAsync(eventId, userId, ct);
        if (ev.CreatedByUserId != userId) throw new UnauthorizedException("Only the creator can delete this event.");
        db.TimelineEvents.Remove(ev);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>Add a photo key to an existing timeline event.</summary>
    [HttpPost("{eventId:guid}/media")]
    public async Task<IActionResult> AddMedia(Guid eventId, [FromBody] AddTimelineMediaRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var ev = await LoadAndAuthorizeAsync(eventId, userId, ct);
        if (string.IsNullOrWhiteSpace(req.PhotoKey)) throw new ValidationException("PhotoKey is required.");
        ev.PhotoKeys.Add(req.PhotoKey);
        await db.SaveChangesAsync(ct);
        return Ok(Map(ev));
    }
}
