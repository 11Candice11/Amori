using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Features.Mood.DTOs;
using Amori.Api.Infrastructure.Relationships;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.Mood.Services;

public sealed class MoodService(
    AmoriDbContext db,
    IRelationshipAccessService relAccess) : IMoodService
{
    private static CheckInResponse Map(MoodCheckIn c) => new()
    {
        Id = c.Id, UserId = c.UserId, RelationshipId = c.RelationshipId,
        CheckInType = c.CheckInType, Mood = c.Mood, Intensity = c.Intensity,
        WhatHappened = c.WhatHappened, Feelings = c.Feelings,
        PerceivedCause = c.PerceivedCause, WhatINeed = c.WhatINeed,
        IsSharedWithPartner = c.IsSharedWithPartner, CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt
    };

    public async Task<CheckInResponse> CreateCheckInAsync(Guid userId, CreateCheckInRequest req, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");

        if (req.Intensity < 1 || req.Intensity > 10)
            throw new ValidationException("Intensity must be between 1 and 10.");

        var checkIn = new MoodCheckIn
        {
            UserId = userId, RelationshipId = rel.Id, CheckInType = req.CheckInType,
            Mood = req.Mood, Intensity = req.Intensity, WhatHappened = req.WhatHappened,
            Feelings = req.Feelings, PerceivedCause = req.PerceivedCause,
            WhatINeed = req.WhatINeed, IsSharedWithPartner = req.IsSharedWithPartner
        };

        db.MoodCheckIns.Add(checkIn);
        await db.SaveChangesAsync(ct);
        return Map(checkIn);
    }

    public async Task<IReadOnlyList<CheckInResponse>> GetCheckInsAsync(Guid userId, CancellationToken ct = default) =>
        await db.MoodCheckIns.Where(c => c.UserId == userId).OrderByDescending(c => c.CreatedAt)
            .Select(c => Map(c)).ToListAsync(ct);

    public async Task<CheckInResponse> GetCheckInAsync(Guid userId, Guid checkInId, CancellationToken ct = default)
    {
        var c = await db.MoodCheckIns.FindAsync([checkInId], ct)
            ?? throw new NotFoundException("Check-in", checkInId);
        if (c.UserId != userId) throw new UnauthorizedException();
        return Map(c);
    }

    public async Task<CheckInResponse> UpdateCheckInAsync(Guid userId, Guid checkInId, UpdateCheckInRequest req, CancellationToken ct = default)
    {
        var c = await db.MoodCheckIns.FindAsync([checkInId], ct)
            ?? throw new NotFoundException("Check-in", checkInId);
        if (c.UserId != userId) throw new UnauthorizedException();

        if (req.Mood.HasValue) c.Mood = req.Mood.Value;
        if (req.Intensity.HasValue)
        {
            if (req.Intensity < 1 || req.Intensity > 10) throw new ValidationException("Intensity must be between 1 and 10.");
            c.Intensity = req.Intensity.Value;
        }
        if (req.WhatHappened != null) c.WhatHappened = req.WhatHappened;
        if (req.Feelings != null) c.Feelings = req.Feelings;
        if (req.PerceivedCause != null) c.PerceivedCause = req.PerceivedCause;
        if (req.WhatINeed != null) c.WhatINeed = req.WhatINeed;
        if (req.IsSharedWithPartner.HasValue) c.IsSharedWithPartner = req.IsSharedWithPartner.Value;

        await db.SaveChangesAsync(ct);
        return Map(c);
    }

    public async Task DeleteCheckInAsync(Guid userId, Guid checkInId, CancellationToken ct = default)
    {
        var c = await db.MoodCheckIns.FindAsync([checkInId], ct)
            ?? throw new NotFoundException("Check-in", checkInId);
        if (c.UserId != userId) throw new UnauthorizedException();
        db.MoodCheckIns.Remove(c);
        await db.SaveChangesAsync(ct);
    }

    public async Task<CheckInResponse> ShareCheckInAsync(Guid userId, Guid checkInId, CancellationToken ct = default)
    {
        var c = await db.MoodCheckIns.FindAsync([checkInId], ct)
            ?? throw new NotFoundException("Check-in", checkInId);
        if (c.UserId != userId) throw new UnauthorizedException();
        c.IsSharedWithPartner = true;
        await db.SaveChangesAsync(ct);
        return Map(c);
    }

    public async Task<CheckInResponse> UnshareCheckInAsync(Guid userId, Guid checkInId, CancellationToken ct = default)
    {
        var c = await db.MoodCheckIns.FindAsync([checkInId], ct)
            ?? throw new NotFoundException("Check-in", checkInId);
        if (c.UserId != userId) throw new UnauthorizedException();
        c.IsSharedWithPartner = false;
        await db.SaveChangesAsync(ct);
        return Map(c);
    }

    public async Task<CheckInResponse?> GetCurrentAsync(Guid userId, CancellationToken ct = default)
    {
        var c = await db.MoodCheckIns.Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync(ct);
        return c == null ? null : Map(c);
    }

    public async Task<IReadOnlyList<CheckInResponse>> GetHistoryAsync(Guid userId, CancellationToken ct = default)
    {
        var since = DateTime.UtcNow.AddDays(-30);
        return await db.MoodCheckIns
            .Where(c => c.UserId == userId && c.CreatedAt >= since)
            .OrderByDescending(c => c.CreatedAt).Select(c => Map(c)).ToListAsync(ct);
    }

    public async Task<MoodSummaryResponse> GetSummaryAsync(Guid userId, CancellationToken ct = default)
    {
        var since = DateTime.UtcNow.AddDays(-30);
        var checkIns = await db.MoodCheckIns
            .Where(c => c.UserId == userId && c.CreatedAt >= since)
            .OrderByDescending(c => c.CreatedAt).ToListAsync(ct);

        var latest = checkIns.FirstOrDefault();
        return new MoodSummaryResponse
        {
            CurrentMood = latest?.Mood, CurrentIntensity = latest?.Intensity,
            LastCheckInAt = latest?.CreatedAt,
            RecentCheckIns = checkIns.Take(10).Select(Map).ToList(),
            MoodFrequency = checkIns.GroupBy(c => c.Mood.ToString()).ToDictionary(g => g.Key, g => g.Count())
        };
    }
}
