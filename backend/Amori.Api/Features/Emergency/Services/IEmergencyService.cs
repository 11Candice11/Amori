using Amori.Api.Features.Emergency;

namespace Amori.Api.Features.Emergency.Services;

/// <summary>
/// Business logic for support / I'm-not-okay requests between partners.
/// </summary>
public interface IEmergencyService
{
    Task<EmergencyRequestResponse> CreateAsync(Guid userId, CreateEmergencyRequestDto request, CancellationToken ct = default);
    Task<IReadOnlyList<EmergencyRequestResponse>> GetAllAsync(Guid userId, CancellationToken ct = default);
    Task<EmergencyRequestResponse> GetByIdAsync(Guid userId, Guid requestId, CancellationToken ct = default);
    Task<EmergencyRequestResponse> UpdateAsync(Guid userId, Guid requestId, UpdateEmergencyRequestDto request, CancellationToken ct = default);
    Task<EmergencyRequestResponse> AcknowledgeAsync(Guid userId, Guid requestId, CancellationToken ct = default);
    Task<EmergencyRequestResponse> ResolveAsync(Guid userId, Guid requestId, CancellationToken ct = default);
    Task<EmergencyRequestResponse> CancelAsync(Guid userId, Guid requestId, CancellationToken ct = default);
}
