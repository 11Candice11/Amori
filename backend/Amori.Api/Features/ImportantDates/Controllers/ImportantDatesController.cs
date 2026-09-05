using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Infrastructure.Authentication;
using Amori.Api.Infrastructure.Relationships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.ImportantDates.Controllers;

public sealed class CreateImportantDateRequest
{
    public string Name { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public bool IsRecurring { get; set; }
    public bool ReminderEnabled { get; set; }
    public int? ReminderDaysBefore { get; set; }
    public string? Notes { get; set; }
    public string? ImageKey { get; set; }
}

public sealed class UpdateImportantDateRequest
{
    public string? Name { get; set; }
    public DateOnly? Date { get; set; }
    public bool? IsRecurring { get; set; }
    public bool? ReminderEnabled { get; set; }
    public int? ReminderDaysBefore { get; set; }
    public string? Notes { get; set; }
    public string? ImageKey { get; set; }
}

public sealed class ImportantDateResponse
{
    public Guid Id { get; init; }
    public Guid RelationshipId { get; init; }
    public Guid CreatedByUserId { get; init; }
    public string CreatedByName { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public DateOnly Date { get; init; }
    public bool IsRecurring { get; init; }
    public bool ReminderEnabled { get; init; }
    public int? ReminderDaysBefore { get; init; }
    public string? Notes { get; init; }
    public string? ImageKey { get; init; }
    public int? DaysUntilNext { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>Important dates — birthdays, anniversaries, milestones.</summary>
[ApiController]
[Route("api/important-dates")]
[Authorize]
public sealed class ImportantDatesController(
    AmoriDbContext db,
    ICurrentUserService currentUser,
    IRelationshipAccessService relAccess) : ControllerBase
{
    private Guid RequireUserId() =>
        currentUser.UserId ?? throw new UnauthorizedException();

    private static ImportantDateResponse Map(ImportantDate d)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        int? daysUntil = null;

        if (d.IsRecurring)
        {
            var next = new DateOnly(today.Year, d.Date.Month, d.Date.Day);
            if (next < today) next = next.AddYears(1);
            daysUntil = next.DayNumber - today.DayNumber;
        }
        else if (d.Date >= today)
        {
            daysUntil = d.Date.DayNumber - today.DayNumber;
        }

        return new ImportantDateResponse
        {
            Id = d.Id, RelationshipId = d.RelationshipId,
            CreatedByUserId = d.CreatedByUserId, CreatedByName = d.CreatedBy?.DisplayName ?? string.Empty,
            Name = d.Name, Date = d.Date, IsRecurring = d.IsRecurring,
            ReminderEnabled = d.ReminderEnabled, ReminderDaysBefore = d.ReminderDaysBefore,
            Notes = d.Notes, ImageKey = d.ImageKey, DaysUntilNext = daysUntil,
            CreatedAt = d.CreatedAt, UpdatedAt = d.UpdatedAt
        };
    }

    private async Task<(Relationship rel, ImportantDate date)> LoadAsync(Guid dateId, Guid userId, CancellationToken ct)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var date = await db.ImportantDates.Include(d => d.CreatedBy)
            .FirstOrDefaultAsync(d => d.Id == dateId, ct)
            ?? throw new NotFoundException("Important date", dateId);
        if (date.RelationshipId != rel.Id) throw new UnauthorizedException();
        return (rel, date);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var userId = RequireUserId();
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var dates = await db.ImportantDates.Include(d => d.CreatedBy)
            .Where(d => d.RelationshipId == rel.Id)
            .OrderBy(d => d.Date).ToListAsync(ct);
        return Ok(dates.Select(Map));
    }

    [HttpGet("{dateId:guid}")]
    public async Task<IActionResult> GetById(Guid dateId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, date) = await LoadAsync(dateId, userId, ct);
        return Ok(Map(date));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateImportantDateRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        if (string.IsNullOrWhiteSpace(req.Name)) throw new ValidationException("Name is required.");

        var date = new ImportantDate
        {
            RelationshipId = rel.Id, CreatedByUserId = userId,
            Name = req.Name.Trim(), Date = req.Date, IsRecurring = req.IsRecurring,
            ReminderEnabled = req.ReminderEnabled, ReminderDaysBefore = req.ReminderDaysBefore,
            Notes = req.Notes, ImageKey = req.ImageKey
        };
        db.ImportantDates.Add(date);
        await db.SaveChangesAsync(ct);
        date.CreatedBy = (await db.Users.FindAsync([userId], ct))!;
        return CreatedAtAction(nameof(GetById), new { dateId = date.Id }, Map(date));
    }

    [HttpPatch("{dateId:guid}")]
    public async Task<IActionResult> Update(Guid dateId, [FromBody] UpdateImportantDateRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, date) = await LoadAsync(dateId, userId, ct);
        if (date.CreatedByUserId != userId) throw new UnauthorizedException("Only the creator can update this.");
        if (req.Name != null) date.Name = req.Name.Trim();
        if (req.Date.HasValue) date.Date = req.Date.Value;
        if (req.IsRecurring.HasValue) date.IsRecurring = req.IsRecurring.Value;
        if (req.ReminderEnabled.HasValue) date.ReminderEnabled = req.ReminderEnabled.Value;
        if (req.ReminderDaysBefore.HasValue) date.ReminderDaysBefore = req.ReminderDaysBefore;
        if (req.Notes != null) date.Notes = req.Notes;
        if (req.ImageKey != null) date.ImageKey = req.ImageKey;
        await db.SaveChangesAsync(ct);
        return Ok(Map(date));
    }

    [HttpDelete("{dateId:guid}")]
    public async Task<IActionResult> Delete(Guid dateId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, date) = await LoadAsync(dateId, userId, ct);
        if (date.CreatedByUserId != userId) throw new UnauthorizedException("Only the creator can delete this.");
        db.ImportantDates.Remove(date);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
