using System.ComponentModel.DataAnnotations;
using Amori.Api.Domain.Enums;

namespace Amori.Api.Features.Incidents.DTOs;

// ── Create ────────────────────────────────────────────────────────────────────

public sealed class CreateIncidentRequest
{
    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Description { get; set; }

    [Required]
    public IncidentCategory Category { get; set; }

    [MaxLength(100)]
    public string? SubCategory { get; set; }

    [Required]
    public IncidentImpact Impact { get; set; }

    [Required]
    public IncidentUrgency Urgency { get; set; }
}

// ── Update ────────────────────────────────────────────────────────────────────

public sealed class UpdateIncidentRequest
{
    [MaxLength(300)]
    public string? Title { get; set; }

    [MaxLength(4000)]
    public string? Description { get; set; }

    public IncidentCategory? Category { get; set; }

    [MaxLength(100)]
    public string? SubCategory { get; set; }

    public IncidentImpact? Impact { get; set; }
    public IncidentUrgency? Urgency { get; set; }
}

// ── Assign ────────────────────────────────────────────────────────────────────

public sealed class AssignIncidentRequest
{
    [Required]
    public Guid AssignedToUserId { get; set; }
}

// ── Resolve ───────────────────────────────────────────────────────────────────

public sealed class ResolveIncidentRequest
{
    [Required, MaxLength(4000)]
    public string Resolution { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? ResolutionNotes { get; set; }
}

// ── Close ─────────────────────────────────────────────────────────────────────

public sealed class CloseIncidentRequest
{
    [MaxLength(4000)]
    public string? ClosureNotes { get; set; }
}

// ── Reopen ────────────────────────────────────────────────────────────────────

public sealed class ReopenIncidentRequest
{
    [Required, MaxLength(2000)]
    public string Reason { get; set; } = string.Empty;
}

// ── Note ──────────────────────────────────────────────────────────────────────

public sealed class AddIncidentNoteRequest
{
    [Required, MaxLength(4000)]
    public string Content { get; set; } = string.Empty;
}

public sealed class UpdateIncidentNoteRequest
{
    [Required, MaxLength(4000)]
    public string Content { get; set; } = string.Empty;
}

// ── Response ──────────────────────────────────────────────────────────────────

public sealed class AddIncidentResponseRequest
{
    [Required, MaxLength(4000)]
    public string Message { get; set; } = string.Empty;
}

public sealed class UpdateIncidentResponseRequest
{
    [Required, MaxLength(4000)]
    public string Message { get; set; } = string.Empty;
}

// ── Review ────────────────────────────────────────────────────────────────────

public sealed class AddIncidentReviewRequest
{
    [MaxLength(2000)]
    public string? WhatWentWell { get; set; }

    [MaxLength(2000)]
    public string? WhatCouldImprove { get; set; }

    [MaxLength(2000)]
    public string? FutureAction { get; set; }
}

// ── Lesson ────────────────────────────────────────────────────────────────────

public sealed class AddIncidentLessonRequest
{
    [Required, MaxLength(2000)]
    public string Lesson { get; set; } = string.Empty;
}

// ── Filter ────────────────────────────────────────────────────────────────────

public sealed class IncidentFilterRequest
{
    public IncidentStatus? Status { get; set; }
    public IncidentCategory? Category { get; set; }
    public IncidentPriority? Priority { get; set; }
    public IncidentImpact? Impact { get; set; }
    public IncidentUrgency? Urgency { get; set; }
    public bool? ReportedByMe { get; set; }
    public bool? AssignedToMe { get; set; }
    public bool? OpenOnly { get; set; }
    public bool? Overdue { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
