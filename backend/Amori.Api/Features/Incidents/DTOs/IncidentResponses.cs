using Amori.Api.Domain.Enums;

namespace Amori.Api.Features.Incidents.DTOs;

// ── Shared sub-types ──────────────────────────────────────────────────────────

public sealed class IncidentUserRef
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}

// ── Incident summary (list view) ──────────────────────────────────────────────

public sealed class IncidentSummaryResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public IncidentCategory Category { get; init; }
    public string? SubCategory { get; init; }
    public IncidentPriority Priority { get; init; }
    public IncidentImpact Impact { get; init; }
    public IncidentUrgency Urgency { get; init; }
    public IncidentStatus Status { get; init; }
    public IncidentUserRef ReportedBy { get; init; } = null!;
    public IncidentUserRef? AssignedTo { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? DueAt { get; init; }
    public bool IsOverdue { get; init; }
}

// ── Incident detail (full view) ───────────────────────────────────────────────

public sealed class IncidentDetailResponse
{
    public Guid Id { get; init; }
    public Guid RelationshipId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public IncidentCategory Category { get; init; }
    public string? SubCategory { get; init; }
    public IncidentImpact Impact { get; init; }
    public IncidentUrgency Urgency { get; init; }
    public IncidentPriority Priority { get; init; }
    public IncidentStatus Status { get; init; }
    public string? Resolution { get; init; }
    public string? ResolutionNotes { get; init; }
    public IncidentUserRef ReportedBy { get; init; } = null!;
    public IncidentUserRef? AssignedTo { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? AssignedAt { get; init; }
    public DateTime? InvestigatedAt { get; init; }
    public DateTime? ResolvedAt { get; init; }
    public DateTime? ClosedAt { get; init; }
    public DateTime? ReopenedAt { get; init; }
    public DateTime? DueAt { get; init; }
    public bool IsOverdue { get; init; }
}

// ── Note ──────────────────────────────────────────────────────────────────────

public sealed class IncidentNoteResponse
{
    public Guid Id { get; init; }
    public Guid IncidentId { get; init; }
    public IncidentUserRef Author { get; init; } = null!;
    public string Content { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

// ── Response (investigation thread) ──────────────────────────────────────────

public sealed class IncidentResponseDto
{
    public Guid Id { get; init; }
    public Guid IncidentId { get; init; }
    public IncidentUserRef Author { get; init; } = null!;
    public string Message { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

// ── History ───────────────────────────────────────────────────────────────────

public sealed class IncidentHistoryResponse
{
    public Guid Id { get; init; }
    public Guid IncidentId { get; init; }
    public IncidentUserRef Actor { get; init; } = null!;
    public IncidentHistoryAction Action { get; init; }
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
    public DateTime CreatedAt { get; init; }
}

// ── SLA ───────────────────────────────────────────────────────────────────────

public sealed class IncidentSlaResponse
{
    public Guid IncidentId { get; init; }
    public IncidentPriority Priority { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime DueAt { get; init; }
    public bool IsOverdue { get; init; }
    public double RemainingSeconds { get; init; }
}

// ── Review ────────────────────────────────────────────────────────────────────

public sealed class IncidentReviewResponse
{
    public Guid Id { get; init; }
    public Guid IncidentId { get; init; }
    public IncidentUserRef CreatedBy { get; init; } = null!;
    public string? WhatWentWell { get; init; }
    public string? WhatCouldImprove { get; init; }
    public string? FutureAction { get; init; }
    public DateTime CreatedAt { get; init; }
}

// ── Lesson ────────────────────────────────────────────────────────────────────

public sealed class IncidentLessonResponse
{
    public Guid Id { get; init; }
    public Guid IncidentId { get; init; }
    public IncidentUserRef CreatedBy { get; init; } = null!;
    public string Lesson { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

// ── Summary stats ─────────────────────────────────────────────────────────────

public sealed class IncidentSummaryStatsResponse
{
    public Guid RelationshipId { get; init; }
    public int Total { get; init; }
    public int Open { get; init; }
    public int Resolved { get; init; }
    public int Closed { get; init; }
    public int Overdue { get; init; }
    public IDictionary<string, int> ByCategory { get; init; } = new Dictionary<string, int>();
    public IDictionary<string, int> ByPriority { get; init; } = new Dictionary<string, int>();
    public IDictionary<string, int> ByReporter { get; init; } = new Dictionary<string, int>();
    public double? AverageResolutionHours { get; init; }
}
