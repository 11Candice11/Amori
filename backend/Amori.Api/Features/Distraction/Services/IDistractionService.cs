using Amori.Api.Features.Distraction.Controllers;

namespace Amori.Api.Features.Distraction.Services;

/// <summary>
/// Business logic for the Distraction Centre — games, sessions, and scores.
/// </summary>
public interface IDistractionService
{
    Task<IReadOnlyList<GameResponse>> GetGamesAsync(CancellationToken ct = default);
    Task<GameResponse> GetGameAsync(Guid gameId, CancellationToken ct = default);
    Task<GameSessionResponse> StartSessionAsync(Guid userId, Guid gameId, CancellationToken ct = default);
    Task<GameSessionResponse> CompleteSessionAsync(Guid userId, Guid sessionId, CompleteGameSessionRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<GameSessionResponse>> GetHistoryAsync(Guid userId, CancellationToken ct = default);
}
