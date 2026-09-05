using Amori.Api.Features.Mood;

namespace Amori.Api.Features.Mood.Services;

/// <summary>
/// Business logic for mood check-ins and mood history.
/// </summary>
public interface IMoodService
{
    Task<CheckInResponse> CreateCheckInAsync(Guid userId, CreateCheckInRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<CheckInResponse>> GetCheckInsAsync(Guid userId, CancellationToken ct = default);
    Task<CheckInResponse> GetCheckInAsync(Guid userId, Guid checkInId, CancellationToken ct = default);
    Task<CheckInResponse> UpdateCheckInAsync(Guid userId, Guid checkInId, UpdateCheckInRequest request, CancellationToken ct = default);
    Task DeleteCheckInAsync(Guid userId, Guid checkInId, CancellationToken ct = default);
    Task<CheckInResponse> ShareCheckInAsync(Guid userId, Guid checkInId, CancellationToken ct = default);
    Task<CheckInResponse> UnshareCheckInAsync(Guid userId, Guid checkInId, CancellationToken ct = default);
    Task<CheckInResponse?> GetCurrentAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<CheckInResponse>> GetHistoryAsync(Guid userId, CancellationToken ct = default);
    Task<MoodSummaryResponse> GetSummaryAsync(Guid userId, CancellationToken ct = default);
}
