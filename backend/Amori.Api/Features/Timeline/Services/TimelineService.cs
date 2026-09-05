using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Infrastructure.Relationships;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.Timeline.Services;

public sealed class TimelineService(
    AmoriDbContext db,
    IRelationshipAccessService relAccess) : ITimelineService
{
    private static TimelineEventResponse Map(TimelineEvent e) => new()
    {
        Id = e.Id, RelationshipId = e.RelationshipId,
        CreatedByUserId = e.CreatedByUserId, CreatedByName = e.CreatedBy?.DisplayName ?? string.Empty,
        Title = e.Title, Description = e.Description, EventDate = e.EventDate,
        Location = e.Location, EventType = e.EventType, PhotoKeys = e.PhotoKeys,
        CreatedAt = e.CreatedAt, UpdatedAt = e.UpdatedAt
    };

    private async Task<TimelineEvent> LoadAsync(Guid userId, Guid eventId, CancellationToken ct)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var ev = await db.TimelineEvents.Include(e => e.CreatedBy)
            .FirstOrDefaultAsync(e => e.Id == eventId, ct)
            ?? throw new NotFoundException("Timeline event", eventId);
        if (ev.RelationshipId != rel.Id) throw new UnauthorizedException();
        return ev;
    }

    public async Task<IReadOnlyList<TimelineEventResponse>> GetAllAsync(Guid userId, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        return await db.TimelineEvents.Include(e => e.CreatedBy)
            .Where(e => e.RelationshipId == rel.Id)
            .OrderBy(e => e.EventDate)
            .Select(e => Map(e)).ToListAsync(ct);
    }

    public async Task<TimelineEventResponse> GetByIdAsync(Guid userId, Guid eventId, CancellationToken ct = default) =>
        Map(await LoadAsync(userId, eventId, ct));

    public async Task<TimelineEventResponse> CreateAsync(Guid userId, CreateTimelineEventRequest request, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        if (string.IsNullOrWhiteSpace(request.Title)) throw new ValidationException("Title is required.");

        var ev = new TimelineEvent
        {
            RelationshipId = rel.Id, CreatedByUserId = userId,
            Title = request.Title.Trim(), Description = request.Description,
            EventDate = request.EventDate, Location = request.Location,
            EventType = request.EventType, PhotoKeys = request.PhotoKeys
        };
        db.TimelineEvents.Add(ev);
        await db.SaveChangesAsync(ct);
        ev.CreatedBy = (await db.Users.FindAsync([userId], ct))!;
        return Map(ev);
    }

    public async Task<TimelineEventResponse> UpdateAsync(Guid userId, Guid eventId, UpdateTimelineEventRequest request, CancellationToken ct = default)
    {
        var ev = await LoadAsync(userId, eventId, ct);
        if (request.Title != null) ev.Title = request.Title.Trim();
        if (request.Description != null) ev.Description = request.Description;
        if (request.EventDate.HasValue) ev.EventDate = request.EventDate.Value;
        if (request.Location != null) ev.Location = request.Location;
        if (request.EventType.HasValue) ev.EventType = request.EventType.Value;
        await db.SaveChangesAsync(ct);
        return Map(ev);
    }

    public async Task DeleteAsync(Guid userId, Guid eventId, CancellationToken ct = default)
    {
        var ev = await LoadAsync(userId, eventId, ct);
        if (ev.CreatedByUserId != userId) throw new UnauthorizedException("Only the creator can delete this event.");
        db.TimelineEvents.Remove(ev);
        await db.SaveChangesAsync(ct);
    }

    public async Task<TimelineEventResponse> AddMediaAsync(Guid userId, Guid eventId, AddTimelineMediaRequest request, CancellationToken ct = default)
    {
        var ev = await LoadAsync(userId, eventId, ct);
        if (string.IsNullOrWhiteSpace(request.PhotoKey)) throw new ValidationException("PhotoKey is required.");
        ev.PhotoKeys.Add(request.PhotoKey);
        await db.SaveChangesAsync(ct);
        return Map(ev);
    }
}
