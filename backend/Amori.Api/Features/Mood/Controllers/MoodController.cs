using Amori.Api.Common.Exceptions;
using Amori.Api.Common.Responses;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Domain.Enums;
using Amori.Api.Infrastructure.Authentication;
using Amori.Api.Infrastructure.Relationships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.Mood;

// ── DTOs ─────────────────────────────────────────────────────────────────────

public sealed class CreateCheckInRequest
{
    public CheckInType CheckInType { get; set; } = CheckInType.AdHoc;
    public MoodType Mood { get; set; }
    public int Intensity { get; set; } = 5;
    public string? WhatHappened { get; set; }
    public string? Feelings { get; set; }
    public string? PerceivedCause { get; set; }
    public string? WhatINeed { get; set; }
    public bool IsSharedWithPartner { get; set; }
}

public sealed class UpdateCheckInRequest
{
    public MoodType? Mood { get; set; }
    public int? Intensity { get; set; }
    public string? WhatHappened { get; set; }
    public string? Feelings { get; set; }
    public string? PerceivedCause { get; set; }
    public string? WhatINeed { get; set; }
    public bool? IsSharedWithPartner { get; set; }
}

public sealed class CheckInResponse
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid RelationshipId { get; init; }
    public CheckInType CheckInType { get; init; }
    public MoodType Mood { get; init; }
    public int Intensity { get; init; }
    public string? WhatHappened { get; init; }
    public string? Feelings { get; init; }
    public string? PerceivedCause { get; init; }
    public string? WhatINeed { get; init; }
    public bool IsSharedWithPartner { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public sealed class MoodSummaryResponse
{
    public MoodType? CurrentMood { get; init; }
    public int? CurrentIntensity { get; init; }
    public DateTime? LastCheckInAt { get; init; }
    public IReadOnlyList<CheckInResponse> RecentCheckIns { get; init; } = [];
    public IDictionary<string, int> MoodFrequency { get; init; } = new Dictionary<string, int>();
}

// ── Controller ────────────────────────────────────────────────────────────────

/// <summary>Mood check-in endpoints for tracking emotional state.</summary>
[ApiController]
[Route("api/moods")]
[Authorize]
public sealed class MoodController(
    AmoriDbContext db,
    ICurrentUserService currentUser,
    IRelationshipAccessService relAccess) : ControllerBase
{
    private Guid RequireUserId() =>
        currentUser.UserId ?? throw new UnauthorizedException();

    private static CheckInResponse Map(MoodCheckIn c) => new()
    {
        Id = c.Id,
        UserId = c.UserId,
        RelationshipId = c.RelationshipId,
        CheckInType = c.CheckInType,
        Mood = c.Mood,
        Intensity = c.Intensity,
        WhatHappened = c.WhatHappened,
        Feelings = c.Feelings,
        PerceivedCause = c.PerceivedCause,
        WhatINeed = c.WhatINeed,
        IsSharedWithPartner = c.IsSharedWithPartner,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt
    };

    /// <summary>Create a mood check-in.</summary>
    [HttpPost("check-ins")]
    [ProducesResponseType(typeof(CheckInResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateCheckIn([FromBody] CreateCheckInRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var relationship = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");

        if (req.Intensity < 1 || req.Intensity > 10)
            throw new ValidationException("Intensity must be between 1 and 10.");

        var checkIn = new MoodCheckIn
        {
            UserId = userId,
            RelationshipId = relationship.Id,
            CheckInType = req.CheckInType,
            Mood = req.Mood,
            Intensity = req.Intensity,
            WhatHappened = req.WhatHappened,
            Feelings = req.Feelings,
            PerceivedCause = req.PerceivedCause,
            WhatINeed = req.WhatINeed,
            IsSharedWithPartner = req.IsSharedWithPartner
        };

        db.MoodCheckIns.Add(checkIn);
        await db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetCheckIn), new { checkInId = checkIn.Id }, Map(checkIn));
    }

    /// <summary>List all check-ins for the authenticated user.</summary>
    [HttpGet("check-ins")]
    [ProducesResponseType(typeof(IReadOnlyList<CheckInResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCheckIns(CancellationToken ct)
    {
        var userId = RequireUserId();
        var items = await db.MoodCheckIns
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => Map(c))
            .ToListAsync(ct);
        return Ok(items);
    }

    /// <summary>Get a single check-in by ID.</summary>
    [HttpGet("check-ins/{checkInId:guid}")]
    [ProducesResponseType(typeof(CheckInResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCheckIn(Guid checkInId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var c = await db.MoodCheckIns.FindAsync([checkInId], ct)
            ?? throw new NotFoundException("Check-in", checkInId);
        if (c.UserId != userId) throw new UnauthorizedException();
        return Ok(Map(c));
    }

    /// <summary>Update a check-in.</summary>
    [HttpPatch("check-ins/{checkInId:guid}")]
    [ProducesResponseType(typeof(CheckInResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateCheckIn(Guid checkInId, [FromBody] UpdateCheckInRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var c = await db.MoodCheckIns.FindAsync([checkInId], ct)
            ?? throw new NotFoundException("Check-in", checkInId);
        if (c.UserId != userId) throw new UnauthorizedException();

        if (req.Mood.HasValue) c.Mood = req.Mood.Value;
        if (req.Intensity.HasValue)
        {
            if (req.Intensity < 1 || req.Intensity > 10)
                throw new ValidationException("Intensity must be between 1 and 10.");
            c.Intensity = req.Intensity.Value;
        }
        if (req.WhatHappened != null) c.WhatHappened = req.WhatHappened;
        if (req.Feelings != null) c.Feelings = req.Feelings;
        if (req.PerceivedCause != null) c.PerceivedCause = req.PerceivedCause;
        if (req.WhatINeed != null) c.WhatINeed = req.WhatINeed;
        if (req.IsSharedWithPartner.HasValue) c.IsSharedWithPartner = req.IsSharedWithPartner.Value;

        await db.SaveChangesAsync(ct);
        return Ok(Map(c));
    }

    /// <summary>Delete a check-in.</summary>
    [HttpDelete("check-ins/{checkInId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteCheckIn(Guid checkInId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var c = await db.MoodCheckIns.FindAsync([checkInId], ct)
            ?? throw new NotFoundException("Check-in", checkInId);
        if (c.UserId != userId) throw new UnauthorizedException();
        db.MoodCheckIns.Remove(c);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>Share a check-in with partner.</summary>
    [HttpPost("check-ins/{checkInId:guid}/share")]
    [ProducesResponseType(typeof(CheckInResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Share(Guid checkInId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var c = await db.MoodCheckIns.FindAsync([checkInId], ct)
            ?? throw new NotFoundException("Check-in", checkInId);
        if (c.UserId != userId) throw new UnauthorizedException();
        c.IsSharedWithPartner = true;
        await db.SaveChangesAsync(ct);
        return Ok(Map(c));
    }

    /// <summary>Unshare a check-in from partner.</summary>
    [HttpPost("check-ins/{checkInId:guid}/unshare")]
    [ProducesResponseType(typeof(CheckInResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Unshare(Guid checkInId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var c = await db.MoodCheckIns.FindAsync([checkInId], ct)
            ?? throw new NotFoundException("Check-in", checkInId);
        if (c.UserId != userId) throw new UnauthorizedException();
        c.IsSharedWithPartner = false;
        await db.SaveChangesAsync(ct);
        return Ok(Map(c));
    }

    /// <summary>Get the user's most recent check-in (current mood).</summary>
    [HttpGet("current")]
    [ProducesResponseType(typeof(CheckInResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrent(CancellationToken ct)
    {
        var userId = RequireUserId();
        var c = await db.MoodCheckIns
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);
        if (c == null) return NoContent();
        return Ok(Map(c));
    }

    /// <summary>Get the user's mood history (last 30 days).</summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(IReadOnlyList<CheckInResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(CancellationToken ct)
    {
        var userId = RequireUserId();
        var since = DateTime.UtcNow.AddDays(-30);
        var items = await db.MoodCheckIns
            .Where(c => c.UserId == userId && c.CreatedAt >= since)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => Map(c))
            .ToListAsync(ct);
        return Ok(items);
    }

    /// <summary>Get mood summary statistics.</summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(MoodSummaryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
    {
        var userId = RequireUserId();
        var since = DateTime.UtcNow.AddDays(-30);

        var checkIns = await db.MoodCheckIns
            .Where(c => c.UserId == userId && c.CreatedAt >= since)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);

        var latest = checkIns.FirstOrDefault();
        var freq = checkIns.GroupBy(c => c.Mood.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var summary = new MoodSummaryResponse
        {
            CurrentMood = latest?.Mood,
            CurrentIntensity = latest?.Intensity,
            LastCheckInAt = latest?.CreatedAt,
            RecentCheckIns = checkIns.Take(10).Select(Map).ToList(),
            MoodFrequency = freq
        };
        return Ok(summary);
    }
}
