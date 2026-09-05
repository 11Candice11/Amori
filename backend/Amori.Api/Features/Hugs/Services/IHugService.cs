using Amori.Api.Features.Hugs;

namespace Amori.Api.Features.Hugs.Services;

/// <summary>
/// Business logic for virtual hugs between partners.
/// </summary>
public interface IHugService
{
    Task<HugResponse> SendHugAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<HugResponse>> GetHugsAsync(Guid userId, CancellationToken ct = default);
    Task<HugResponse> GetHugAsync(Guid userId, Guid hugId, CancellationToken ct = default);
    Task<HugResponse> AcknowledgeAsync(Guid userId, Guid hugId, CancellationToken ct = default);
}
