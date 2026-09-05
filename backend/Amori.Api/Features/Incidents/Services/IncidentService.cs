using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Domain.Enums;
using Amori.Api.Features.Incidents.DTOs;
using Amori.Api.Infrastructure.Relationships;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.Incidents.Services;

public sealed class IncidentService(
    AmoriDbContext db,
    IRelationshipAccessService relAccess,
    IIncidentPriorityService priorityService,
    IIncidentStatusService statusService,
    IIncidentSlaService slaService) : IIncidentService
{
    // ── Mapping ───────────────────────────────────────────────────────────────

    private IncidentDetailResponse MapDetail(RelationshipIncident i) => new()
    {
        Id = i.Id, RelationshipId = i.RelationshipId,
        Title = i.Title, Description = i.Description,
        Category = i.Category, SubCategory = i.SubCategory,
        Impact = i.Impact, Urgency = i.Urgency, Priority = i.Priority, Status = i.Status,
        Resolution = i.Resolution, ResolutionNotes = i.ResolutionNotes,
        ReportedBy = new IncidentUserRef { Id = i.ReportedBy.Id, Name = i.ReportedBy.DisplayName },
        AssignedTo = i.AssignedTo == null ? null : new IncidentUserRef { Id = i.AssignedTo.Id, Name = i.AssignedTo.DisplayName },
        CreatedAt = i.CreatedAt, UpdatedAt = i.UpdatedAt,
        AssignedAt = i.AssignedAt, InvestigatedAt = i.InvestigatedAt,
        ResolvedAt = i.ResolvedAt, ClosedAt = i.ClosedAt, ReopenedAt = i.ReopenedAt,
        DueAt = i.DueAt,
        IsOverdue = i.DueAt.HasValue && slaService.IsOverdue(i.DueAt.Value, i.Status)
    };

    private IncidentSummaryResponse MapSummary(RelationshipIncident i) => new()
    {
        Id = i.Id, Title = i.Title, Category = i.Category, SubCategory = i.SubCategory,
        Priority = i.Priority, Impact = i.Impact, Urgency = i.Urgency, Status = i.Status,
        ReportedBy = new IncidentUserRef { Id = i.ReportedBy.Id, Name = i.ReportedBy.DisplayName },
        AssignedTo = i.AssignedTo == null ? null : new IncidentUserRef { Id = i.AssignedTo.Id, Name = i.AssignedTo.DisplayName },
        CreatedAt = i.CreatedAt,
        DueAt = i.DueAt,
        IsOverdue = i.DueAt.HasValue && slaService.IsOverdue(i.DueAt.Value, i.Status)
    };

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<(Guid relId, RelationshipIncident incident)> LoadAndAuthorizeAsync(
        Guid userId, Guid incidentId, CancellationToken ct)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var incident = await db.RelationshipIncidents
            .Include(i => i.ReportedBy)
            .Include(i => i.AssignedTo)
            .FirstOrDefaultAsync(i => i.Id == incidentId, ct)
            ?? throw new NotFoundException("Incident", incidentId);
        if (incident.RelationshipId != rel.Id) throw new UnauthorizedException();
        return (rel.Id, incident);
    }

    private async Task AddHistoryAsync(Guid incidentId, Guid actorId,
        IncidentHistoryAction action, string? oldValue = null, string? newValue = null)
    {
        db.RelationshipIncidentHistory.Add(new RelationshipIncidentHistory
        {
            IncidentId = incidentId, ActorUserId = actorId,
            Action = action, OldValue = oldValue, NewValue = newValue
        });
        await Task.CompletedTask; // will be saved with the parent SaveChangesAsync
    }

    // ── CRUD ──────────────────────────────────────────────────────────────────

    public async Task<IncidentDetailResponse> CreateAsync(Guid userId, CreateIncidentRequest req, CancellationToken ct)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");

        if (string.IsNullOrWhiteSpace(req.Title)) throw new ValidationException("Title is required.");

        var priority = priorityService.Calculate(req.Impact, req.Urgency);
        var now = DateTime.UtcNow;
        var dueAt = slaService.CalculateDueAt(priority, now);

        var incident = new RelationshipIncident
        {
            RelationshipId = rel.Id,
            ReportedByUserId = userId,
            Title = req.Title.Trim(),
            Description = req.Description,
            Category = req.Category,
            SubCategory = req.SubCategory,
            Impact = req.Impact,
            Urgency = req.Urgency,
            Priority = priority,
            Status = IncidentStatus.Open,
            DueAt = dueAt
        };

        db.RelationshipIncidents.Add(incident);
        await AddHistoryAsync(incident.Id, userId, IncidentHistoryAction.Created,
            newValue: $"Priority={priority}, DueAt={dueAt:u}");
        await db.SaveChangesAsync(ct);

        // Reload for navigation
        incident.ReportedBy = (await db.Users.FindAsync([userId], ct))!;
        return MapDetail(incident);
    }

    public async Task<PagedResult<IncidentSummaryResponse>> ListAsync(
        Guid userId, IncidentFilterRequest filter, CancellationToken ct)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");

        var query = db.RelationshipIncidents
            .Include(i => i.ReportedBy).Include(i => i.AssignedTo)
            .Where(i => i.RelationshipId == rel.Id);

        if (filter.Status.HasValue) query = query.Where(i => i.Status == filter.Status.Value);
        if (filter.Category.HasValue) query = query.Where(i => i.Category == filter.Category.Value);
        if (filter.Priority.HasValue) query = query.Where(i => i.Priority == filter.Priority.Value);
        if (filter.Impact.HasValue) query = query.Where(i => i.Impact == filter.Impact.Value);
        if (filter.Urgency.HasValue) query = query.Where(i => i.Urgency == filter.Urgency.Value);
        if (filter.ReportedByMe == true) query = query.Where(i => i.ReportedByUserId == userId);
        if (filter.AssignedToMe == true) query = query.Where(i => i.AssignedToUserId == userId);
        if (filter.OpenOnly == true) query = query.Where(i =>
            i.Status != IncidentStatus.Resolved && i.Status != IncidentStatus.Closed);
        if (filter.Overdue == true) query = query.Where(i =>
            i.DueAt.HasValue && i.DueAt.Value < DateTime.UtcNow &&
            i.Status != IncidentStatus.Resolved && i.Status != IncidentStatus.Closed);

        var total = await query.CountAsync(ct);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var page = Math.Max(1, filter.Page);

        var items = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<IncidentSummaryResponse>
        {
            Items = items.Select(MapSummary).ToList(),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<IncidentDetailResponse> GetAsync(Guid userId, Guid incidentId, CancellationToken ct)
    {
        var (_, incident) = await LoadAndAuthorizeAsync(userId, incidentId, ct);
        return MapDetail(incident);
    }

    public async Task<IncidentDetailResponse> UpdateAsync(
        Guid userId, Guid incidentId, UpdateIncidentRequest req, CancellationToken ct)
    {
        var (_, incident) = await LoadAndAuthorizeAsync(userId, incidentId, ct);

        var recalculate = false;
        if (req.Title != null) incident.Title = req.Title.Trim();
        if (req.Description != null) incident.Description = req.Description;
        if (req.SubCategory != null) incident.SubCategory = req.SubCategory;

        if (req.Category.HasValue && req.Category.Value != incident.Category)
        {
            await AddHistoryAsync(incidentId, userId, IncidentHistoryAction.CategoryChanged,
                incident.Category.ToString(), req.Category.Value.ToString());
            incident.Category = req.Category.Value;
        }
        if (req.Impact.HasValue && req.Impact.Value != incident.Impact)
        {
            incident.Impact = req.Impact.Value; recalculate = true;
        }
        if (req.Urgency.HasValue && req.Urgency.Value != incident.Urgency)
        {
            incident.Urgency = req.Urgency.Value; recalculate = true;
        }

        if (recalculate)
        {
            var oldPriority = incident.Priority;
            incident.Priority = priorityService.Calculate(incident.Impact, incident.Urgency);
            if (incident.Priority != oldPriority)
            {
                await AddHistoryAsync(incidentId, userId, IncidentHistoryAction.PriorityChanged,
                    oldPriority.ToString(), incident.Priority.ToString());
                incident.DueAt = slaService.CalculateDueAt(incident.Priority, incident.CreatedAt);
            }
        }

        await AddHistoryAsync(incidentId, userId, IncidentHistoryAction.Updated);
        await db.SaveChangesAsync(ct);
        return MapDetail(incident);
    }

    public async Task<IncidentDetailResponse> AssignAsync(
        Guid userId, Guid incidentId, AssignIncidentRequest req, CancellationToken ct)
    {
        var (relId, incident) = await LoadAndAuthorizeAsync(userId, incidentId, ct);
        var isMember = await relAccess.IsUserMemberAsync(req.AssignedToUserId, relId);
        if (!isMember) throw new UnauthorizedException("Can only assign to a member of your relationship.");

        incident.AssignedToUserId = req.AssignedToUserId;
        incident.AssignedAt = DateTime.UtcNow;

        if (incident.Status == IncidentStatus.Open)
        {
            statusService.EnsureValidTransition(incident.Status, IncidentStatus.Assigned);
            incident.Status = IncidentStatus.Assigned;
            await AddHistoryAsync(incidentId, userId, IncidentHistoryAction.StatusChanged,
                IncidentStatus.Open.ToString(), IncidentStatus.Assigned.ToString());
        }

        incident.AssignedTo = await db.Users.FindAsync([req.AssignedToUserId], ct);
        await AddHistoryAsync(incidentId, userId, IncidentHistoryAction.Assigned,
            newValue: req.AssignedToUserId.ToString());
        await db.SaveChangesAsync(ct);
        return MapDetail(incident);
    }

    public async Task<IncidentDetailResponse> StartInvestigationAsync(
        Guid userId, Guid incidentId, CancellationToken ct)
    {
        var (_, incident) = await LoadAndAuthorizeAsync(userId, incidentId, ct);
        statusService.EnsureValidTransition(incident.Status, IncidentStatus.Investigating);
        var oldStatus = incident.Status;
        incident.Status = IncidentStatus.Investigating;
        incident.InvestigatedAt ??= DateTime.UtcNow;
        await AddHistoryAsync(incidentId, userId, IncidentHistoryAction.InvestigationStarted,
            oldStatus.ToString(), IncidentStatus.Investigating.ToString());
        await db.SaveChangesAsync(ct);
        return MapDetail(incident);
    }

    public async Task<IncidentDetailResponse> SetAwaitingResponseAsync(
        Guid userId, Guid incidentId, CancellationToken ct)
    {
        var (_, incident) = await LoadAndAuthorizeAsync(userId, incidentId, ct);
        statusService.EnsureValidTransition(incident.Status, IncidentStatus.AwaitingResponse);
        var oldStatus = incident.Status;
        incident.Status = IncidentStatus.AwaitingResponse;
        await AddHistoryAsync(incidentId, userId, IncidentHistoryAction.ResponseRequested,
            oldStatus.ToString(), IncidentStatus.AwaitingResponse.ToString());
        await db.SaveChangesAsync(ct);
        return MapDetail(incident);
    }

    public async Task<IncidentDetailResponse> ResolveAsync(
        Guid userId, Guid incidentId, ResolveIncidentRequest req, CancellationToken ct)
    {
        var (_, incident) = await LoadAndAuthorizeAsync(userId, incidentId, ct);
        statusService.EnsureValidTransition(incident.Status, IncidentStatus.Resolved);
        var oldStatus = incident.Status;
        incident.Status = IncidentStatus.Resolved;
        incident.Resolution = req.Resolution;
        incident.ResolutionNotes = req.ResolutionNotes;
        incident.ResolvedAt = DateTime.UtcNow;
        await AddHistoryAsync(incidentId, userId, IncidentHistoryAction.Resolved,
            oldStatus.ToString(), IncidentStatus.Resolved.ToString());
        await db.SaveChangesAsync(ct);
        return MapDetail(incident);
    }

    public async Task<IncidentDetailResponse> CloseAsync(
        Guid userId, Guid incidentId, CloseIncidentRequest req, CancellationToken ct)
    {
        var (_, incident) = await LoadAndAuthorizeAsync(userId, incidentId, ct);
        statusService.EnsureValidTransition(incident.Status, IncidentStatus.Closed);
        var oldStatus = incident.Status;
        incident.Status = IncidentStatus.Closed;
        incident.ClosedAt = DateTime.UtcNow;
        if (req.ClosureNotes != null) incident.ResolutionNotes = req.ClosureNotes;
        await AddHistoryAsync(incidentId, userId, IncidentHistoryAction.Closed,
            oldStatus.ToString(), IncidentStatus.Closed.ToString());
        await db.SaveChangesAsync(ct);
        return MapDetail(incident);
    }

    public async Task<IncidentDetailResponse> ReopenAsync(
        Guid userId, Guid incidentId, ReopenIncidentRequest req, CancellationToken ct)
    {
        var (_, incident) = await LoadAndAuthorizeAsync(userId, incidentId, ct);
        statusService.EnsureValidTransition(incident.Status, IncidentStatus.Reopened);
        var oldStatus = incident.Status;
        incident.Status = IncidentStatus.Reopened;
        incident.ReopenedAt = DateTime.UtcNow;
        await AddHistoryAsync(incidentId, userId, IncidentHistoryAction.Reopened,
            oldStatus.ToString(), $"{IncidentStatus.Reopened}: {req.Reason}");
        await db.SaveChangesAsync(ct);
        return MapDetail(incident);
    }

    // ── Notes ─────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<IncidentNoteResponse>> GetNotesAsync(
        Guid userId, Guid incidentId, CancellationToken ct)
    {
        await LoadAndAuthorizeAsync(userId, incidentId, ct);
        return await db.RelationshipIncidentNotes
            .Include(n => n.Author)
            .Where(n => n.IncidentId == incidentId)
            .OrderBy(n => n.CreatedAt)
            .Select(n => new IncidentNoteResponse
            {
                Id = n.Id, IncidentId = n.IncidentId,
                Author = new IncidentUserRef { Id = n.Author.Id, Name = n.Author.DisplayName },
                Content = n.Content, CreatedAt = n.CreatedAt, UpdatedAt = n.UpdatedAt
            }).ToListAsync(ct);
    }

    public async Task<IncidentNoteResponse> AddNoteAsync(
        Guid userId, Guid incidentId, AddIncidentNoteRequest req, CancellationToken ct)
    {
        await LoadAndAuthorizeAsync(userId, incidentId, ct);
        var note = new RelationshipIncidentNote
        {
            IncidentId = incidentId, AuthorUserId = userId, Content = req.Content
        };
        db.RelationshipIncidentNotes.Add(note);
        await AddHistoryAsync(incidentId, userId, IncidentHistoryAction.NoteAdded);
        await db.SaveChangesAsync(ct);
        note.Author = (await db.Users.FindAsync([userId], ct))!;
        return new IncidentNoteResponse
        {
            Id = note.Id, IncidentId = note.IncidentId,
            Author = new IncidentUserRef { Id = note.Author.Id, Name = note.Author.DisplayName },
            Content = note.Content, CreatedAt = note.CreatedAt
        };
    }

    public async Task<IncidentNoteResponse> UpdateNoteAsync(
        Guid userId, Guid incidentId, Guid noteId, UpdateIncidentNoteRequest req, CancellationToken ct)
    {
        await LoadAndAuthorizeAsync(userId, incidentId, ct);
        var note = await db.RelationshipIncidentNotes.Include(n => n.Author)
            .FirstOrDefaultAsync(n => n.Id == noteId && n.IncidentId == incidentId, ct)
            ?? throw new NotFoundException("Note", noteId);
        if (note.AuthorUserId != userId) throw new UnauthorizedException("Only the author can edit this note.");
        note.Content = req.Content;
        await db.SaveChangesAsync(ct);
        return new IncidentNoteResponse
        {
            Id = note.Id, IncidentId = note.IncidentId,
            Author = new IncidentUserRef { Id = note.Author.Id, Name = note.Author.DisplayName },
            Content = note.Content, CreatedAt = note.CreatedAt, UpdatedAt = note.UpdatedAt
        };
    }

    public async Task DeleteNoteAsync(Guid userId, Guid incidentId, Guid noteId, CancellationToken ct)
    {
        await LoadAndAuthorizeAsync(userId, incidentId, ct);
        var note = await db.RelationshipIncidentNotes
            .FirstOrDefaultAsync(n => n.Id == noteId && n.IncidentId == incidentId, ct)
            ?? throw new NotFoundException("Note", noteId);
        if (note.AuthorUserId != userId) throw new UnauthorizedException("Only the author can delete this note.");
        db.RelationshipIncidentNotes.Remove(note);
        await db.SaveChangesAsync(ct);
    }

    // ── Responses ─────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<IncidentResponseDto>> GetResponsesAsync(
        Guid userId, Guid incidentId, CancellationToken ct)
    {
        await LoadAndAuthorizeAsync(userId, incidentId, ct);
        return await db.RelationshipIncidentResponses
            .Include(r => r.Author)
            .Where(r => r.IncidentId == incidentId)
            .OrderBy(r => r.CreatedAt)
            .Select(r => new IncidentResponseDto
            {
                Id = r.Id, IncidentId = r.IncidentId,
                Author = new IncidentUserRef { Id = r.Author.Id, Name = r.Author.DisplayName },
                Message = r.Message, CreatedAt = r.CreatedAt, UpdatedAt = r.UpdatedAt
            }).ToListAsync(ct);
    }

    public async Task<IncidentResponseDto> AddResponseAsync(
        Guid userId, Guid incidentId, AddIncidentResponseRequest req, CancellationToken ct)
    {
        await LoadAndAuthorizeAsync(userId, incidentId, ct);
        var response = new RelationshipIncidentResponse
        {
            IncidentId = incidentId, AuthorUserId = userId, Message = req.Message
        };
        db.RelationshipIncidentResponses.Add(response);
        await AddHistoryAsync(incidentId, userId, IncidentHistoryAction.ResponseAdded);
        await db.SaveChangesAsync(ct);
        response.Author = (await db.Users.FindAsync([userId], ct))!;
        return new IncidentResponseDto
        {
            Id = response.Id, IncidentId = response.IncidentId,
            Author = new IncidentUserRef { Id = response.Author.Id, Name = response.Author.DisplayName },
            Message = response.Message, CreatedAt = response.CreatedAt
        };
    }

    public async Task<IncidentResponseDto> UpdateResponseAsync(
        Guid userId, Guid incidentId, Guid responseId, UpdateIncidentResponseRequest req, CancellationToken ct)
    {
        await LoadAndAuthorizeAsync(userId, incidentId, ct);
        var response = await db.RelationshipIncidentResponses.Include(r => r.Author)
            .FirstOrDefaultAsync(r => r.Id == responseId && r.IncidentId == incidentId, ct)
            ?? throw new NotFoundException("Response", responseId);
        if (response.AuthorUserId != userId) throw new UnauthorizedException("Only the author can edit this response.");
        response.Message = req.Message;
        await db.SaveChangesAsync(ct);
        return new IncidentResponseDto
        {
            Id = response.Id, IncidentId = response.IncidentId,
            Author = new IncidentUserRef { Id = response.Author.Id, Name = response.Author.DisplayName },
            Message = response.Message, CreatedAt = response.CreatedAt, UpdatedAt = response.UpdatedAt
        };
    }

    public async Task DeleteResponseAsync(Guid userId, Guid incidentId, Guid responseId, CancellationToken ct)
    {
        await LoadAndAuthorizeAsync(userId, incidentId, ct);
        var response = await db.RelationshipIncidentResponses
            .FirstOrDefaultAsync(r => r.Id == responseId && r.IncidentId == incidentId, ct)
            ?? throw new NotFoundException("Response", responseId);
        if (response.AuthorUserId != userId) throw new UnauthorizedException("Only the author can delete this response.");
        db.RelationshipIncidentResponses.Remove(response);
        await db.SaveChangesAsync(ct);
    }

    // ── History ───────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<IncidentHistoryResponse>> GetHistoryAsync(
        Guid userId, Guid incidentId, CancellationToken ct)
    {
        await LoadAndAuthorizeAsync(userId, incidentId, ct);
        return await db.RelationshipIncidentHistory
            .Include(h => h.Actor)
            .Where(h => h.IncidentId == incidentId)
            .OrderBy(h => h.CreatedAt)
            .Select(h => new IncidentHistoryResponse
            {
                Id = h.Id, IncidentId = h.IncidentId,
                Actor = new IncidentUserRef { Id = h.Actor.Id, Name = h.Actor.DisplayName },
                Action = h.Action, OldValue = h.OldValue, NewValue = h.NewValue,
                CreatedAt = h.CreatedAt
            }).ToListAsync(ct);
    }

    // ── SLA ───────────────────────────────────────────────────────────────────

    public async Task<IncidentSlaResponse> GetSlaAsync(Guid userId, Guid incidentId, CancellationToken ct)
    {
        var (_, incident) = await LoadAndAuthorizeAsync(userId, incidentId, ct);
        if (!incident.DueAt.HasValue)
            throw new NotFoundException("SLA information is not available for this incident.");

        return new IncidentSlaResponse
        {
            IncidentId = incident.Id,
            Priority = incident.Priority,
            CreatedAt = incident.CreatedAt,
            DueAt = incident.DueAt.Value,
            IsOverdue = slaService.IsOverdue(incident.DueAt.Value, incident.Status),
            RemainingSeconds = slaService.RemainingSeconds(incident.DueAt.Value)
        };
    }

    // ── Review ────────────────────────────────────────────────────────────────

    public async Task<IncidentReviewResponse> AddReviewAsync(
        Guid userId, Guid incidentId, AddIncidentReviewRequest req, CancellationToken ct)
    {
        var (_, incident) = await LoadAndAuthorizeAsync(userId, incidentId, ct);
        if (incident.Status is not (IncidentStatus.Resolved or IncidentStatus.Closed))
            throw new ConflictException("Reviews can only be added after an incident is resolved or closed.");

        var review = new RelationshipIncidentReview
        {
            IncidentId = incidentId, CreatedByUserId = userId,
            WhatWentWell = req.WhatWentWell,
            WhatCouldImprove = req.WhatCouldImprove,
            FutureAction = req.FutureAction
        };
        db.RelationshipIncidentReviews.Add(review);
        await AddHistoryAsync(incidentId, userId, IncidentHistoryAction.ReviewAdded);
        await db.SaveChangesAsync(ct);
        review.CreatedBy = (await db.Users.FindAsync([userId], ct))!;
        return new IncidentReviewResponse
        {
            Id = review.Id, IncidentId = review.IncidentId,
            CreatedBy = new IncidentUserRef { Id = review.CreatedBy.Id, Name = review.CreatedBy.DisplayName },
            WhatWentWell = review.WhatWentWell, WhatCouldImprove = review.WhatCouldImprove,
            FutureAction = review.FutureAction, CreatedAt = review.CreatedAt
        };
    }

    // ── Lessons ───────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<IncidentLessonResponse>> GetLessonsAsync(
        Guid userId, Guid incidentId, CancellationToken ct)
    {
        await LoadAndAuthorizeAsync(userId, incidentId, ct);
        return await db.RelationshipIncidentLessons
            .Include(l => l.CreatedBy)
            .Where(l => l.IncidentId == incidentId)
            .OrderBy(l => l.CreatedAt)
            .Select(l => new IncidentLessonResponse
            {
                Id = l.Id, IncidentId = l.IncidentId,
                CreatedBy = new IncidentUserRef { Id = l.CreatedBy.Id, Name = l.CreatedBy.DisplayName },
                Lesson = l.Lesson, CreatedAt = l.CreatedAt
            }).ToListAsync(ct);
    }

    public async Task<IncidentLessonResponse> AddLessonAsync(
        Guid userId, Guid incidentId, AddIncidentLessonRequest req, CancellationToken ct)
    {
        await LoadAndAuthorizeAsync(userId, incidentId, ct);
        var lesson = new RelationshipIncidentLesson
        {
            IncidentId = incidentId, CreatedByUserId = userId, Lesson = req.Lesson
        };
        db.RelationshipIncidentLessons.Add(lesson);
        await AddHistoryAsync(incidentId, userId, IncidentHistoryAction.LessonAdded);
        await db.SaveChangesAsync(ct);
        lesson.CreatedBy = (await db.Users.FindAsync([userId], ct))!;
        return new IncidentLessonResponse
        {
            Id = lesson.Id, IncidentId = lesson.IncidentId,
            CreatedBy = new IncidentUserRef { Id = lesson.CreatedBy.Id, Name = lesson.CreatedBy.DisplayName },
            Lesson = lesson.Lesson, CreatedAt = lesson.CreatedAt
        };
    }

    // ── Summary ───────────────────────────────────────────────────────────────

    public async Task<IncidentSummaryStatsResponse> GetSummaryAsync(Guid userId, CancellationToken ct)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");

        var incidents = await db.RelationshipIncidents
            .Include(i => i.ReportedBy)
            .Where(i => i.RelationshipId == rel.Id)
            .ToListAsync(ct);

        var now = DateTime.UtcNow;
        var resolved = incidents.Where(i =>
            i.ResolvedAt.HasValue && i.Status == IncidentStatus.Resolved).ToList();

        double? avgHours = resolved.Count > 0
            ? resolved.Average(i => (i.ResolvedAt!.Value - i.CreatedAt).TotalHours)
            : null;

        return new IncidentSummaryStatsResponse
        {
            RelationshipId = rel.Id,
            Total = incidents.Count,
            Open = incidents.Count(i => i.Status is IncidentStatus.Open or IncidentStatus.Assigned
                or IncidentStatus.Investigating or IncidentStatus.AwaitingResponse or IncidentStatus.Reopened),
            Resolved = incidents.Count(i => i.Status == IncidentStatus.Resolved),
            Closed = incidents.Count(i => i.Status == IncidentStatus.Closed),
            Overdue = incidents.Count(i => i.DueAt.HasValue && slaService.IsOverdue(i.DueAt.Value, i.Status)),
            ByCategory = incidents.GroupBy(i => i.Category.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            ByPriority = incidents.GroupBy(i => i.Priority.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            ByReporter = incidents.GroupBy(i => i.ReportedBy.DisplayName)
                .ToDictionary(g => g.Key, g => g.Count()),
            AverageResolutionHours = avgHours
        };
    }
}
