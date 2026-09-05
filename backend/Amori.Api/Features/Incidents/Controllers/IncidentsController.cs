using Amori.Api.Common.Exceptions;
using Amori.Api.Features.Incidents.DTOs;
using Amori.Api.Features.Incidents.Services;
using Amori.Api.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Amori.Api.Features.Incidents.Controllers;

/// <summary>
/// Relationship Incident Management.
/// Incidents represent problems in the relationship that are logged, investigated and resolved
/// through a structured lifecycle rather than immediate verbal discussion.
/// </summary>
[ApiController]
[Route("api/incidents")]
[Authorize]
public sealed class IncidentsController(
    IIncidentService incidentService,
    ICurrentUserService currentUser) : ControllerBase
{
    private Guid RequireUserId() =>
        currentUser.UserId ?? throw new UnauthorizedException();

    // ── Core CRUD ─────────────────────────────────────────────────────────────

    /// <summary>Create a new incident.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(IncidentDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateIncidentRequest req, CancellationToken ct)
    {
        var result = await incidentService.CreateAsync(RequireUserId(), req, ct);
        return CreatedAtAction(nameof(GetById), new { incidentId = result.Id }, result);
    }

    /// <summary>List incidents for the authenticated user's relationship with optional filters.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<IncidentSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] IncidentFilterRequest filter, CancellationToken ct) =>
        Ok(await incidentService.ListAsync(RequireUserId(), filter, ct));

    /// <summary>Get a single incident with full detail.</summary>
    [HttpGet("{incidentId:guid}")]
    [ProducesResponseType(typeof(IncidentDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid incidentId, CancellationToken ct) =>
        Ok(await incidentService.GetAsync(RequireUserId(), incidentId, ct));

    /// <summary>Update editable fields on an incident (title, description, category, impact, urgency).</summary>
    [HttpPatch("{incidentId:guid}")]
    [ProducesResponseType(typeof(IncidentDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid incidentId, [FromBody] UpdateIncidentRequest req, CancellationToken ct) =>
        Ok(await incidentService.UpdateAsync(RequireUserId(), incidentId, req, ct));

    // ── Lifecycle transitions ─────────────────────────────────────────────────

    /// <summary>Assign the incident to the partner. Sets status to ASSIGNED.</summary>
    [HttpPost("{incidentId:guid}/assign")]
    [ProducesResponseType(typeof(IncidentDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Assign(Guid incidentId, [FromBody] AssignIncidentRequest req, CancellationToken ct) =>
        Ok(await incidentService.AssignAsync(RequireUserId(), incidentId, req, ct));

    /// <summary>Begin investigation. Sets status to INVESTIGATING.</summary>
    [HttpPost("{incidentId:guid}/investigate")]
    [ProducesResponseType(typeof(IncidentDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Investigate(Guid incidentId, CancellationToken ct) =>
        Ok(await incidentService.StartInvestigationAsync(RequireUserId(), incidentId, ct));

    /// <summary>Mark incident as awaiting a response from the partner.</summary>
    [HttpPost("{incidentId:guid}/awaiting-response")]
    [ProducesResponseType(typeof(IncidentDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> AwaitingResponse(Guid incidentId, CancellationToken ct) =>
        Ok(await incidentService.SetAwaitingResponseAsync(RequireUserId(), incidentId, ct));

    /// <summary>Resolve the incident with a resolution summary.</summary>
    [HttpPost("{incidentId:guid}/resolve")]
    [ProducesResponseType(typeof(IncidentDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Resolve(Guid incidentId, [FromBody] ResolveIncidentRequest req, CancellationToken ct) =>
        Ok(await incidentService.ResolveAsync(RequireUserId(), incidentId, req, ct));

    /// <summary>Close the incident. Resolution accepted and documented.</summary>
    [HttpPost("{incidentId:guid}/close")]
    [ProducesResponseType(typeof(IncidentDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Close(Guid incidentId, [FromBody] CloseIncidentRequest req, CancellationToken ct) =>
        Ok(await incidentService.CloseAsync(RequireUserId(), incidentId, req, ct));

    /// <summary>Reopen a closed or resolved incident.</summary>
    [HttpPost("{incidentId:guid}/reopen")]
    [ProducesResponseType(typeof(IncidentDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Reopen(Guid incidentId, [FromBody] ReopenIncidentRequest req, CancellationToken ct) =>
        Ok(await incidentService.ReopenAsync(RequireUserId(), incidentId, req, ct));

    // ── Notes ─────────────────────────────────────────────────────────────────

    /// <summary>Get all investigation notes for an incident.</summary>
    [HttpGet("{incidentId:guid}/notes")]
    [ProducesResponseType(typeof(IReadOnlyList<IncidentNoteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotes(Guid incidentId, CancellationToken ct) =>
        Ok(await incidentService.GetNotesAsync(RequireUserId(), incidentId, ct));

    /// <summary>Add an investigation note.</summary>
    [HttpPost("{incidentId:guid}/notes")]
    [ProducesResponseType(typeof(IncidentNoteResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddNote(Guid incidentId, [FromBody] AddIncidentNoteRequest req, CancellationToken ct)
    {
        var result = await incidentService.AddNoteAsync(RequireUserId(), incidentId, req, ct);
        return CreatedAtAction(nameof(GetNotes), new { incidentId }, result);
    }

    /// <summary>Update an investigation note (author only).</summary>
    [HttpPatch("{incidentId:guid}/notes/{noteId:guid}")]
    [ProducesResponseType(typeof(IncidentNoteResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateNote(Guid incidentId, Guid noteId, [FromBody] UpdateIncidentNoteRequest req, CancellationToken ct) =>
        Ok(await incidentService.UpdateNoteAsync(RequireUserId(), incidentId, noteId, req, ct));

    /// <summary>Delete an investigation note (author only).</summary>
    [HttpDelete("{incidentId:guid}/notes/{noteId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteNote(Guid incidentId, Guid noteId, CancellationToken ct)
    {
        await incidentService.DeleteNoteAsync(RequireUserId(), incidentId, noteId, ct);
        return NoContent();
    }

    // ── Responses ─────────────────────────────────────────────────────────────

    /// <summary>Get all investigation responses for an incident.</summary>
    [HttpGet("{incidentId:guid}/responses")]
    [ProducesResponseType(typeof(IReadOnlyList<IncidentResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResponses(Guid incidentId, CancellationToken ct) =>
        Ok(await incidentService.GetResponsesAsync(RequireUserId(), incidentId, ct));

    /// <summary>Add a response to an incident.</summary>
    [HttpPost("{incidentId:guid}/responses")]
    [ProducesResponseType(typeof(IncidentResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddResponse(Guid incidentId, [FromBody] AddIncidentResponseRequest req, CancellationToken ct)
    {
        var result = await incidentService.AddResponseAsync(RequireUserId(), incidentId, req, ct);
        return CreatedAtAction(nameof(GetResponses), new { incidentId }, result);
    }

    /// <summary>Update a response (author only).</summary>
    [HttpPatch("{incidentId:guid}/responses/{responseId:guid}")]
    [ProducesResponseType(typeof(IncidentResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateResponse(Guid incidentId, Guid responseId, [FromBody] UpdateIncidentResponseRequest req, CancellationToken ct) =>
        Ok(await incidentService.UpdateResponseAsync(RequireUserId(), incidentId, responseId, req, ct));

    /// <summary>Delete a response (author only).</summary>
    [HttpDelete("{incidentId:guid}/responses/{responseId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteResponse(Guid incidentId, Guid responseId, CancellationToken ct)
    {
        await incidentService.DeleteResponseAsync(RequireUserId(), incidentId, responseId, ct);
        return NoContent();
    }

    // ── History ───────────────────────────────────────────────────────────────

    /// <summary>Get the append-only audit history for an incident.</summary>
    [HttpGet("{incidentId:guid}/history")]
    [ProducesResponseType(typeof(IReadOnlyList<IncidentHistoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(Guid incidentId, CancellationToken ct) =>
        Ok(await incidentService.GetHistoryAsync(RequireUserId(), incidentId, ct));

    // ── SLA ───────────────────────────────────────────────────────────────────

    /// <summary>Get SLA countdown information for an incident.</summary>
    [HttpGet("{incidentId:guid}/sla")]
    [ProducesResponseType(typeof(IncidentSlaResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSla(Guid incidentId, CancellationToken ct) =>
        Ok(await incidentService.GetSlaAsync(RequireUserId(), incidentId, ct));

    // ── Post-Implementation Review ────────────────────────────────────────────

    /// <summary>Add a post-implementation review (only after resolved/closed).</summary>
    [HttpPost("{incidentId:guid}/review")]
    [ProducesResponseType(typeof(IncidentReviewResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddReview(Guid incidentId, [FromBody] AddIncidentReviewRequest req, CancellationToken ct)
    {
        var result = await incidentService.AddReviewAsync(RequireUserId(), incidentId, req, ct);
        return CreatedAtAction(nameof(GetById), new { incidentId }, result);
    }

    // ── Lessons Learned ───────────────────────────────────────────────────────

    /// <summary>Get all lessons learned for an incident.</summary>
    [HttpGet("{incidentId:guid}/lessons")]
    [ProducesResponseType(typeof(IReadOnlyList<IncidentLessonResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLessons(Guid incidentId, CancellationToken ct) =>
        Ok(await incidentService.GetLessonsAsync(RequireUserId(), incidentId, ct));

    /// <summary>Record a lesson learned.</summary>
    [HttpPost("{incidentId:guid}/lessons")]
    [ProducesResponseType(typeof(IncidentLessonResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddLesson(Guid incidentId, [FromBody] AddIncidentLessonRequest req, CancellationToken ct)
    {
        var result = await incidentService.AddLessonAsync(RequireUserId(), incidentId, req, ct);
        return CreatedAtAction(nameof(GetLessons), new { incidentId }, result);
    }

    // ── Summary ───────────────────────────────────────────────────────────────

    /// <summary>Get relationship-level incident statistics.</summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(IncidentSummaryStatsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken ct) =>
        Ok(await incidentService.GetSummaryAsync(RequireUserId(), ct));
}
