using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Domain.Enums;
using Amori.Api.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.Reminders;

// ── DTOs ─────────────────────────────────────────────────────────────────────

public sealed class CreateReminderRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public ReminderType Type { get; set; }
    public TimeOnly ReminderTime { get; set; }
    public RecurrenceType Recurrence { get; set; } = RecurrenceType.Daily;
    public DateOnly? OneTimeDate { get; set; }
    public bool IsEnabled { get; set; } = true;
}

public sealed class UpdateReminderRequest
{
    public string? Title { get; set; }
    public string? Notes { get; set; }
    public ReminderType? Type { get; set; }
    public TimeOnly? ReminderTime { get; set; }
    public RecurrenceType? Recurrence { get; set; }
    public DateOnly? OneTimeDate { get; set; }
    public bool? IsEnabled { get; set; }
}

public sealed class SnoozeReminderRequest
{
    public int SnoozeMinutes { get; set; } = 15;
}

public sealed class ReminderResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public ReminderType Type { get; init; }
    public TimeOnly ReminderTime { get; init; }
    public RecurrenceType Recurrence { get; init; }
    public DateOnly? OneTimeDate { get; init; }
    public bool IsEnabled { get; init; }
    public DateTime? LastCompletedAt { get; init; }
    public DateTime? SnoozeUntil { get; init; }
    public DateTime? NextOccurrenceAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

// ── Controller ────────────────────────────────────────────────────────────────

/// <summary>Medication, water, food and custom reminders for the authenticated user.</summary>
[ApiController]
[Route("api/reminders")]
[Authorize]
public sealed class RemindersController(
    AmoriDbContext db,
    ICurrentUserService currentUser) : ControllerBase
{
    private Guid RequireUserId() =>
        currentUser.UserId ?? throw new UnauthorizedException();

    private static ReminderResponse Map(Reminder r) => new()
    {
        Id = r.Id,
        Title = r.Title,
        Notes = r.Notes,
        Type = r.Type,
        ReminderTime = r.ReminderTime,
        Recurrence = r.Recurrence,
        OneTimeDate = r.OneTimeDate,
        IsEnabled = r.IsEnabled,
        LastCompletedAt = r.LastCompletedAt,
        SnoozeUntil = r.SnoozeUntil,
        NextOccurrenceAt = r.NextOccurrenceAt,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt
    };

    /// <summary>List all reminders for the authenticated user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ReminderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReminders(CancellationToken ct)
    {
        var userId = RequireUserId();
        var items = await db.Reminders
            .Where(r => r.UserId == userId)
            .OrderBy(r => r.ReminderTime)
            .Select(r => Map(r))
            .ToListAsync(ct);
        return Ok(items);
    }

    /// <summary>Get a reminder by ID.</summary>
    [HttpGet("{reminderId:guid}")]
    [ProducesResponseType(typeof(ReminderResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReminder(Guid reminderId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var r = await db.Reminders.FindAsync([reminderId], ct)
            ?? throw new NotFoundException("Reminder", reminderId);
        if (r.UserId != userId) throw new UnauthorizedException();
        return Ok(Map(r));
    }

    /// <summary>Create a new reminder.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReminderResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateReminder([FromBody] CreateReminderRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        if (string.IsNullOrWhiteSpace(req.Title))
            throw new ValidationException("Title is required.");

        var reminder = new Reminder
        {
            UserId = userId,
            Title = req.Title.Trim(),
            Notes = req.Notes,
            Type = req.Type,
            ReminderTime = req.ReminderTime,
            Recurrence = req.Recurrence,
            OneTimeDate = req.OneTimeDate,
            IsEnabled = req.IsEnabled
        };

        db.Reminders.Add(reminder);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetReminder), new { reminderId = reminder.Id }, Map(reminder));
    }

    /// <summary>Update a reminder.</summary>
    [HttpPatch("{reminderId:guid}")]
    [ProducesResponseType(typeof(ReminderResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateReminder(Guid reminderId, [FromBody] UpdateReminderRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var r = await db.Reminders.FindAsync([reminderId], ct)
            ?? throw new NotFoundException("Reminder", reminderId);
        if (r.UserId != userId) throw new UnauthorizedException();

        if (req.Title != null) r.Title = req.Title.Trim();
        if (req.Notes != null) r.Notes = req.Notes;
        if (req.Type.HasValue) r.Type = req.Type.Value;
        if (req.ReminderTime.HasValue) r.ReminderTime = req.ReminderTime.Value;
        if (req.Recurrence.HasValue) r.Recurrence = req.Recurrence.Value;
        if (req.OneTimeDate.HasValue) r.OneTimeDate = req.OneTimeDate.Value;
        if (req.IsEnabled.HasValue) r.IsEnabled = req.IsEnabled.Value;

        await db.SaveChangesAsync(ct);
        return Ok(Map(r));
    }

    /// <summary>Delete a reminder.</summary>
    [HttpDelete("{reminderId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteReminder(Guid reminderId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var r = await db.Reminders.FindAsync([reminderId], ct)
            ?? throw new NotFoundException("Reminder", reminderId);
        if (r.UserId != userId) throw new UnauthorizedException();
        db.Reminders.Remove(r);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>Mark a reminder as complete.</summary>
    [HttpPost("{reminderId:guid}/complete")]
    [ProducesResponseType(typeof(ReminderResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Complete(Guid reminderId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var r = await db.Reminders.FindAsync([reminderId], ct)
            ?? throw new NotFoundException("Reminder", reminderId);
        if (r.UserId != userId) throw new UnauthorizedException();
        r.LastCompletedAt = DateTime.UtcNow;
        r.SnoozeUntil = null;
        await db.SaveChangesAsync(ct);
        return Ok(Map(r));
    }

    /// <summary>Skip today's occurrence of a reminder.</summary>
    [HttpPost("{reminderId:guid}/skip")]
    [ProducesResponseType(typeof(ReminderResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Skip(Guid reminderId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var r = await db.Reminders.FindAsync([reminderId], ct)
            ?? throw new NotFoundException("Reminder", reminderId);
        if (r.UserId != userId) throw new UnauthorizedException();
        // Advance NextOccurrenceAt to tomorrow at the same time
        r.NextOccurrenceAt = DateTime.UtcNow.Date.AddDays(1)
            .Add(r.ReminderTime.ToTimeSpan());
        await db.SaveChangesAsync(ct);
        return Ok(Map(r));
    }

    /// <summary>Snooze a reminder.</summary>
    [HttpPost("{reminderId:guid}/snooze")]
    [ProducesResponseType(typeof(ReminderResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Snooze(Guid reminderId, [FromBody] SnoozeReminderRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var r = await db.Reminders.FindAsync([reminderId], ct)
            ?? throw new NotFoundException("Reminder", reminderId);
        if (r.UserId != userId) throw new UnauthorizedException();
        if (req.SnoozeMinutes < 1) throw new ValidationException("Snooze must be at least 1 minute.");
        r.SnoozeUntil = DateTime.UtcNow.AddMinutes(req.SnoozeMinutes);
        await db.SaveChangesAsync(ct);
        return Ok(Map(r));
    }

    /// <summary>Get reminders due today for the authenticated user.</summary>
    [HttpGet("today")]
    [ProducesResponseType(typeof(IReadOnlyList<ReminderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetToday(CancellationToken ct)
    {
        var userId = RequireUserId();
        var todayDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var items = await db.Reminders
            .Where(r => r.UserId == userId && r.IsEnabled &&
                (r.Recurrence != RecurrenceType.None || r.OneTimeDate == todayDate))
            .OrderBy(r => r.ReminderTime)
            .Select(r => Map(r))
            .ToListAsync(ct);
        return Ok(items);
    }
}
