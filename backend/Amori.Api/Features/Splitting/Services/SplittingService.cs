using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Domain.Enums;
using Amori.Api.Infrastructure.Relationships;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.Splitting.Services;

public sealed class SplittingService(
    AmoriDbContext db,
    IRelationshipAccessService relAccess) : ISplittingService
{
    private static SplittingSessionResponse Map(SplittingSession s) => new()
    {
        Id = s.Id, UserId = s.UserId, RelationshipId = s.RelationshipId,
        FeelingsSelected = s.FeelingsSelected, Trigger = s.Trigger,
        Description = s.Description, WhatINeed = s.WhatINeed,
        RecommendedSupportType = s.RecommendedSupportType, ActionsTaken = s.ActionsTaken,
        InitialMood = s.InitialMood, FinalMood = s.FinalMood,
        Status = s.Status, CreatedAt = s.CreatedAt, CompletedAt = s.CompletedAt
    };

    private static SplittingAction Recommend(List<string> feelings, MoodType? mood) =>
        mood switch
        {
            MoodType.Anxious     => SplittingAction.Grounding,
            MoodType.Angry       => SplittingAction.Space,
            MoodType.Sad         => SplittingAction.VoiceNote,
            MoodType.Overwhelmed => SplittingAction.Cbt,
            MoodType.Numb        => SplittingAction.ContactPartner,
            _                    => SplittingAction.Reassurance
        };

    private async Task<SplittingSession> LoadAsync(Guid userId, Guid sessionId, CancellationToken ct)
    {
        var s = await db.SplittingSessions.FindAsync([sessionId], ct)
            ?? throw new NotFoundException("Session", sessionId);
        if (s.UserId != userId) throw new UnauthorizedException();
        return s;
    }

    public async Task<SplittingSessionResponse> CreateAsync(Guid userId, CreateSplittingSessionRequest request, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");

        var session = new SplittingSession
        {
            UserId = userId, RelationshipId = rel.Id,
            FeelingsSelected = request.FeelingsSelected, Trigger = request.Trigger,
            Description = request.Description, WhatINeed = request.WhatINeed,
            InitialMood = request.InitialMood,
            RecommendedSupportType = Recommend(request.FeelingsSelected, request.InitialMood),
            Status = SplittingSessionStatus.InProgress
        };
        db.SplittingSessions.Add(session);
        await db.SaveChangesAsync(ct);
        return Map(session);
    }

    public async Task<SplittingSessionResponse> GetByIdAsync(Guid userId, Guid sessionId, CancellationToken ct = default) =>
        Map(await LoadAsync(userId, sessionId, ct));

    public async Task<IReadOnlyList<SplittingSessionResponse>> GetAllAsync(Guid userId, CancellationToken ct = default) =>
        await db.SplittingSessions.Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt).Select(s => Map(s)).ToListAsync(ct);

    public async Task<SplittingSessionResponse> UpdateAsync(Guid userId, Guid sessionId, UpdateSplittingSessionRequest request, CancellationToken ct = default)
    {
        var s = await LoadAsync(userId, sessionId, ct);
        if (s.Status != SplittingSessionStatus.InProgress)
            throw new ConflictException("Only in-progress sessions can be updated.");
        if (request.FeelingsSelected != null) s.FeelingsSelected = request.FeelingsSelected;
        if (request.Trigger != null) s.Trigger = request.Trigger;
        if (request.Description != null) s.Description = request.Description;
        if (request.WhatINeed != null) s.WhatINeed = request.WhatINeed;
        if (request.ActionsTaken != null) s.ActionsTaken = request.ActionsTaken;
        if (request.FinalMood.HasValue) s.FinalMood = request.FinalMood;
        await db.SaveChangesAsync(ct);
        return Map(s);
    }

    public async Task<SplittingSessionResponse> CompleteAsync(Guid userId, Guid sessionId, CompleteSplittingSessionRequest request, CancellationToken ct = default)
    {
        var s = await LoadAsync(userId, sessionId, ct);
        if (s.Status != SplittingSessionStatus.InProgress)
            throw new ConflictException("Only in-progress sessions can be completed.");
        if (request.FinalMood.HasValue) s.FinalMood = request.FinalMood;
        if (request.ActionsTaken != null) s.ActionsTaken = request.ActionsTaken;
        s.Status = SplittingSessionStatus.Completed;
        s.CompletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Map(s);
    }

    public async Task<SplittingSessionResponse> CancelAsync(Guid userId, Guid sessionId, CancellationToken ct = default)
    {
        var s = await LoadAsync(userId, sessionId, ct);
        if (s.Status != SplittingSessionStatus.InProgress)
            throw new ConflictException("Only in-progress sessions can be cancelled.");
        s.Status = SplittingSessionStatus.Cancelled;
        await db.SaveChangesAsync(ct);
        return Map(s);
    }
}
