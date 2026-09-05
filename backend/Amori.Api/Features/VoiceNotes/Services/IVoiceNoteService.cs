using Amori.Api.Features.VoiceNotes;

namespace Amori.Api.Features.VoiceNotes.Services;

/// <summary>
/// Business logic for voice note metadata management (files are stored in S3).
/// </summary>
public interface IVoiceNoteService
{
    Task<IReadOnlyList<VoiceNoteResponse>> GetAllAsync(Guid userId, CancellationToken ct = default);
    Task<VoiceNoteResponse> GetByIdAsync(Guid userId, Guid voiceNoteId, CancellationToken ct = default);
    Task<VoiceNoteResponse> CreateAsync(Guid userId, CreateVoiceNoteRequest request, CancellationToken ct = default);
    Task<VoiceNoteResponse> UpdateAsync(Guid userId, Guid voiceNoteId, UpdateVoiceNoteRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid userId, Guid voiceNoteId, CancellationToken ct = default);
    Task<VoiceNoteResponse> FavoriteAsync(Guid userId, Guid voiceNoteId, CancellationToken ct = default);
    Task<VoiceNoteResponse> UnfavoriteAsync(Guid userId, Guid voiceNoteId, CancellationToken ct = default);
}
