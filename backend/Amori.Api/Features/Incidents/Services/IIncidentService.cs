using Amori.Api.Features.Incidents.DTOs;

namespace Amori.Api.Features.Incidents.Services;

/// <summary>
/// Core incident business logic.
/// </summary>
public interface IIncidentService
{
    Task<IncidentDetailResponse> CreateAsync(Guid userId, CreateIncidentRequest request, CancellationToken ct = default);

    Task<PagedResult<IncidentSummaryResponse>> ListAsync(Guid userId, IncidentFilterRequest filter, CancellationToken ct = default);

    Task<IncidentDetailResponse> GetAsync(Guid userId, Guid incidentId, CancellationToken ct = default);

    Task<IncidentDetailResponse> UpdateAsync(Guid userId, Guid incidentId, UpdateIncidentRequest request, CancellationToken ct = default);

    Task<IncidentDetailResponse> AssignAsync(Guid userId, Guid incidentId, AssignIncidentRequest request, CancellationToken ct = default);

    Task<IncidentDetailResponse> StartInvestigationAsync(Guid userId, Guid incidentId, CancellationToken ct = default);

    Task<IncidentDetailResponse> SetAwaitingResponseAsync(Guid userId, Guid incidentId, CancellationToken ct = default);

    Task<IncidentDetailResponse> ResolveAsync(Guid userId, Guid incidentId, ResolveIncidentRequest request, CancellationToken ct = default);

    Task<IncidentDetailResponse> CloseAsync(Guid userId, Guid incidentId, CloseIncidentRequest request, CancellationToken ct = default);

    Task<IncidentDetailResponse> ReopenAsync(Guid userId, Guid incidentId, ReopenIncidentRequest request, CancellationToken ct = default);

    // Notes
    Task<IReadOnlyList<IncidentNoteResponse>> GetNotesAsync(Guid userId, Guid incidentId, CancellationToken ct = default);
    Task<IncidentNoteResponse> AddNoteAsync(Guid userId, Guid incidentId, AddIncidentNoteRequest request, CancellationToken ct = default);
    Task<IncidentNoteResponse> UpdateNoteAsync(Guid userId, Guid incidentId, Guid noteId, UpdateIncidentNoteRequest request, CancellationToken ct = default);
    Task DeleteNoteAsync(Guid userId, Guid incidentId, Guid noteId, CancellationToken ct = default);

    // Responses
    Task<IReadOnlyList<IncidentResponseDto>> GetResponsesAsync(Guid userId, Guid incidentId, CancellationToken ct = default);
    Task<IncidentResponseDto> AddResponseAsync(Guid userId, Guid incidentId, AddIncidentResponseRequest request, CancellationToken ct = default);
    Task<IncidentResponseDto> UpdateResponseAsync(Guid userId, Guid incidentId, Guid responseId, UpdateIncidentResponseRequest request, CancellationToken ct = default);
    Task DeleteResponseAsync(Guid userId, Guid incidentId, Guid responseId, CancellationToken ct = default);

    // History
    Task<IReadOnlyList<IncidentHistoryResponse>> GetHistoryAsync(Guid userId, Guid incidentId, CancellationToken ct = default);

    // SLA
    Task<IncidentSlaResponse> GetSlaAsync(Guid userId, Guid incidentId, CancellationToken ct = default);

    // Review
    Task<IncidentReviewResponse> AddReviewAsync(Guid userId, Guid incidentId, AddIncidentReviewRequest request, CancellationToken ct = default);

    // Lessons
    Task<IReadOnlyList<IncidentLessonResponse>> GetLessonsAsync(Guid userId, Guid incidentId, CancellationToken ct = default);
    Task<IncidentLessonResponse> AddLessonAsync(Guid userId, Guid incidentId, AddIncidentLessonRequest request, CancellationToken ct = default);

    // Summary
    Task<IncidentSummaryStatsResponse> GetSummaryAsync(Guid userId, CancellationToken ct = default);
}
