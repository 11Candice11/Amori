using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Domain.Enums;
using Amori.Api.Infrastructure.Authentication;
using Amori.Api.Infrastructure.Relationships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.Splitting;

// ── DTOs ─────────────────────────────────────────────────────────────────────

public sealed class CreateSplittingSessionRequest
{
    public List<string> FeelingsSelected { get; set; } = [];
    public string? Trigger { get; set; }
    public string? Description { get; set; }
    public string? WhatINeed { get; set; }
    public MoodType? InitialMood { get; set; }
}

public sealed class UpdateSplittingSessionRequest
{
    public List<string>? FeelingsSelected { get; set; }
    public string? Trigger { get; set; }
    public string? Description { get; set; }
    public string? WhatINeed { get; set; }
    public List<SplittingAction>? ActionsTaken { get; set; }
    public MoodType? FinalMood { get; set; }
}

public sealed class CompleteSplittingSessionRequest
{
    public MoodType? FinalMood { get; set; }
    public List<SplittingAction>? ActionsTaken { get; set; }
}

public sealed class SplittingSessionResponse
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid RelationshipId { get; init; }
    public List<string> FeelingsSelected { get; init; } = [];
    public string? Trigger { get; init; }
    public string? Description { get; init; }
    public string? WhatINeed { get; init; }
    public SplittingAction? RecommendedSupportType { get; init; }
    public List<SplittingAction> ActionsTaken { get; init; } = [];
    public MoodType? InitialMood { get; init; }
    public MoodType? FinalMood { get; init; }
    public SplittingSessionStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}

// ── Simple recommendation service (kept out of controller) ───────────────────

file static class SplittingSupportRecommender
{
    public static SplittingAction Recommend(List<string> feelings, MoodType? mood) =>
        mood switch
        {
            MoodType.Anxious     => SplittingAction.Grounding,
            MoodType.Angry       => SplittingAction.Space,
            MoodType.Sad         => SplittingAction.VoiceNote,
            MoodType.Overwhelmed => SplittingAction.Cbt,
            MoodType.Numb        => SplittingAction.ContactPartner,
            _                    => SplittingAction.Reassurance
        };
}

// ── Controller ────────────────────────────────────────────────────────────────

/// <summary>"I'm splitting" emotional grounding flow.</summary>
[ApiController]
[Route("api/splitting/sessions")]
[Authorize]
public sealed class SplittingController(
    AmoriDbContext db,
    ICurrentUserService currentUser,
    IRelationshipAccessService relAccess) : ControllerBase
{
    private Guid RequireUserId() =>
        currentUser.UserId ?? throw new UnauthorizedException();

    private static SplittingSessionResponse Map(SplittingSession s) => new()
    {
        Id = s.Id,
        UserId = s.UserId,
        RelationshipId = s.RelationshipId,
        FeelingsSelected = s.FeelingsSelected,
        Trigger = s.Trigger,
        Description = s.Description,
        WhatINeed = s.WhatINeed,
        RecommendedSupportType = s.RecommendedSupportType,
        ActionsTaken = s.ActionsTaken,
        InitialMood = s.InitialMood,
        FinalMood = s.FinalMood,
        Status = s.Status,
        CreatedAt = s.CreatedAt,
        CompletedAt = s.CompletedAt
    };

    private async Task<SplittingSession> LoadAndAuthorizeAsync(Guid sessionId, Guid userId, CancellationToken ct)
    {
        var s = await db.SplittingSessions.FindAsync([sessionId], ct)
            ?? throw new NotFoundException("Session", sessionId);
        if (s.UserId != userId) throw new UnauthorizedException();
        return s;
    }

    /// <summary>Start a new splitting session.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSplittingSessionRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");

        var recommended = SplittingSupportRecommender.Recommend(req.FeelingsSelected, req.InitialMood);

        var session = new SplittingSession
        {
            UserId = userId,
            RelationshipId = rel.Id,
            FeelingsSelected = req.FeelingsSelected,
            Trigger = req.Trigger,
            Description = req.Description,
            WhatINeed = req.WhatINeed,
            InitialMood = req.InitialMood,
            RecommendedSupportType = recommended,
            Status = SplittingSessionStatus.InProgress
        };

        db.SplittingSessions.Add(session);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { sessionId = session.Id }, Map(session));
    }

    [HttpGet("{sessionId:guid}")]
    public async Task<IActionResult> GetById(Guid sessionId, CancellationToken ct)
    {
        var userId = RequireUserId();
        return Ok(Map(await LoadAndAuthorizeAsync(sessionId, userId, ct)));
    }

    /// <summary>List all sessions for the authenticated user.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var userId = RequireUserId();
        var items = await db.SplittingSessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => Map(s))
            .ToListAsync(ct);
        return Ok(items);
    }

    [HttpPatch("{sessionId:guid}")]
    public async Task<IActionResult> Update(Guid sessionId, [FromBody] UpdateSplittingSessionRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var s = await LoadAndAuthorizeAsync(sessionId, userId, ct);
        if (s.Status != SplittingSessionStatus.InProgress)
            throw new ConflictException("Only in-progress sessions can be updated.");

        if (req.FeelingsSelected != null) s.FeelingsSelected = req.FeelingsSelected;
        if (req.Trigger != null) s.Trigger = req.Trigger;
        if (req.Description != null) s.Description = req.Description;
        if (req.WhatINeed != null) s.WhatINeed = req.WhatINeed;
        if (req.ActionsTaken != null) s.ActionsTaken = req.ActionsTaken;
        if (req.FinalMood.HasValue) s.FinalMood = req.FinalMood;

        await db.SaveChangesAsync(ct);
        return Ok(Map(s));
    }

    [HttpPost("{sessionId:guid}/complete")]
    public async Task<IActionResult> Complete(Guid sessionId, [FromBody] CompleteSplittingSessionRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var s = await LoadAndAuthorizeAsync(sessionId, userId, ct);
        if (s.Status != SplittingSessionStatus.InProgress)
            throw new ConflictException("Only in-progress sessions can be completed.");

        if (req.FinalMood.HasValue) s.FinalMood = req.FinalMood;
        if (req.ActionsTaken != null) s.ActionsTaken = req.ActionsTaken;
        s.Status = SplittingSessionStatus.Completed;
        s.CompletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Ok(Map(s));
    }

    [HttpPost("{sessionId:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid sessionId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var s = await LoadAndAuthorizeAsync(sessionId, userId, ct);
        if (s.Status != SplittingSessionStatus.InProgress)
            throw new ConflictException("Only in-progress sessions can be cancelled.");
        s.Status = SplittingSessionStatus.Cancelled;
        await db.SaveChangesAsync(ct);
        return Ok(Map(s));
    }
}
