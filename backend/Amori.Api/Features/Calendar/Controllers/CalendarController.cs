using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Infrastructure.Authentication;
using Amori.Api.Infrastructure.Relationships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.Calendar.Controllers;

public sealed class CreateCalendarEventRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly EventDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string? Location { get; set; }
    public bool ReminderEnabled { get; set; }
    public int? ReminderMinutesBefore { get; set; }
    public bool IsShared { get; set; } = true;
}

public sealed class UpdateCalendarEventRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateOnly? EventDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string? Location { get; set; }
    public bool? ReminderEnabled { get; set; }
    public int? ReminderMinutesBefore { get; set; }
    public bool? IsShared { get; set; }
}

public sealed class CalendarEventResponse
{
    public Guid Id { get; init; }
    public Guid RelationshipId { get; init; }
    public Guid CreatedByUserId { get; init; }
    public string CreatedByName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateOnly EventDate { get; init; }
    public TimeOnly? StartTime { get; init; }
    public TimeOnly? EndTime { get; init; }
    public string? Location { get; init; }
    public bool ReminderEnabled { get; init; }
    public int? ReminderMinutesBefore { get; init; }
    public bool IsCompleted { get; init; }
    public DateTime? CompletedAt { get; init; }
    public bool IsShared { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>Shared relationship calendar events.</summary>
[ApiController]
[Route("api/calendar/events")]
[Authorize]
public sealed class CalendarController(
    AmoriDbContext db,
    ICurrentUserService currentUser,
    IRelationshipAccessService relAccess) : ControllerBase
{
    private Guid RequireUserId() =>
        currentUser.UserId ?? throw new UnauthorizedException();

    private static CalendarEventResponse Map(CalendarEvent e) => new()
    {
        Id = e.Id, RelationshipId = e.RelationshipId,
        CreatedByUserId = e.CreatedByUserId, CreatedByName = e.CreatedBy?.DisplayName ?? string.Empty,
        Title = e.Title, Description = e.Description, EventDate = e.EventDate,
        StartTime = e.StartTime, EndTime = e.EndTime, Location = e.Location,
        ReminderEnabled = e.ReminderEnabled, ReminderMinutesBefore = e.ReminderMinutesBefore,
        IsCompleted = e.IsCompleted, CompletedAt = e.CompletedAt,
        IsShared = e.IsShared, CreatedAt = e.CreatedAt, UpdatedAt = e.UpdatedAt
    };

    private async Task<(Relationship rel, CalendarEvent ev)> LoadAsync(Guid eventId, Guid userId, CancellationToken ct)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var ev = await db.CalendarEvents.Include(e => e.CreatedBy)
            .FirstOrDefaultAsync(e => e.Id == eventId, ct)
            ?? throw new NotFoundException("Calendar event", eventId);
        if (ev.RelationshipId != rel.Id) throw new UnauthorizedException();
        return (rel, ev);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? year, [FromQuery] int? month, CancellationToken ct)
    {
        var userId = RequireUserId();
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");

        var query = db.CalendarEvents.Include(e => e.CreatedBy)
            .Where(e => e.RelationshipId == rel.Id);

        if (year.HasValue && month.HasValue)
        {
            var from = new DateOnly(year.Value, month.Value, 1);
            var to = from.AddMonths(1);
            query = query.Where(e => e.EventDate >= from && e.EventDate < to);
        }

        var events = await query.OrderBy(e => e.EventDate).ThenBy(e => e.StartTime).ToListAsync(ct);
        return Ok(events.Select(Map));
    }

    [HttpGet("{eventId:guid}")]
    public async Task<IActionResult> GetById(Guid eventId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, ev) = await LoadAsync(eventId, userId, ct);
        return Ok(Map(ev));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCalendarEventRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        if (string.IsNullOrWhiteSpace(req.Title)) throw new ValidationException("Title is required.");

        var ev = new CalendarEvent
        {
            RelationshipId = rel.Id, CreatedByUserId = userId,
            Title = req.Title.Trim(), Description = req.Description,
            EventDate = req.EventDate, StartTime = req.StartTime, EndTime = req.EndTime,
            Location = req.Location, ReminderEnabled = req.ReminderEnabled,
            ReminderMinutesBefore = req.ReminderMinutesBefore, IsShared = req.IsShared
        };
        db.CalendarEvents.Add(ev);
        await db.SaveChangesAsync(ct);
        ev.CreatedBy = (await db.Users.FindAsync([userId], ct))!;
        return CreatedAtAction(nameof(GetById), new { eventId = ev.Id }, Map(ev));
    }

    [HttpPatch("{eventId:guid}")]
    public async Task<IActionResult> Update(Guid eventId, [FromBody] UpdateCalendarEventRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, ev) = await LoadAsync(eventId, userId, ct);
        if (ev.CreatedByUserId != userId) throw new UnauthorizedException("Only the creator can update this event.");
        if (req.Title != null) ev.Title = req.Title.Trim();
        if (req.Description != null) ev.Description = req.Description;
        if (req.EventDate.HasValue) ev.EventDate = req.EventDate.Value;
        if (req.StartTime.HasValue) ev.StartTime = req.StartTime;
        if (req.EndTime.HasValue) ev.EndTime = req.EndTime;
        if (req.Location != null) ev.Location = req.Location;
        if (req.ReminderEnabled.HasValue) ev.ReminderEnabled = req.ReminderEnabled.Value;
        if (req.ReminderMinutesBefore.HasValue) ev.ReminderMinutesBefore = req.ReminderMinutesBefore;
        if (req.IsShared.HasValue) ev.IsShared = req.IsShared.Value;
        await db.SaveChangesAsync(ct);
        return Ok(Map(ev));
    }

    [HttpDelete("{eventId:guid}")]
    public async Task<IActionResult> Delete(Guid eventId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, ev) = await LoadAsync(eventId, userId, ct);
        if (ev.CreatedByUserId != userId) throw new UnauthorizedException("Only the creator can delete this event.");
        db.CalendarEvents.Remove(ev);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("{eventId:guid}/complete")]
    public async Task<IActionResult> Complete(Guid eventId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, ev) = await LoadAsync(eventId, userId, ct);
        ev.IsCompleted = true;
        ev.CompletedAt ??= DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Ok(Map(ev));
    }
}
