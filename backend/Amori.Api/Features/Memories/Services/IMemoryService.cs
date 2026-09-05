using Amori.Api.Features.Memories;

namespace Amori.Api.Features.Memories.Services;

/// <summary>
/// Business logic for shared relationship memories with media support.
/// </summary>
public interface IMemoryService
{
    Task<IReadOnlyList<MemoryResponse>> GetAllAsync(Guid userId, CancellationToken ct = default);
    Task<MemoryResponse> GetByIdAsync(Guid userId, Guid memoryId, CancellationToken ct = default);
    Task<MemoryResponse> CreateAsync(Guid userId, CreateMemoryRequest request, CancellationToken ct = default);
    Task<MemoryResponse> UpdateAsync(Guid userId, Guid memoryId, UpdateMemoryRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid userId, Guid memoryId, CancellationToken ct = default);
    Task<MemoryResponse> FavoriteAsync(Guid userId, Guid memoryId, CancellationToken ct = default);
    Task<MemoryResponse> UnfavoriteAsync(Guid userId, Guid memoryId, CancellationToken ct = default);
    Task<MemoryResponse> AddMediaAsync(Guid userId, Guid memoryId, AddMemoryMediaRequest request, CancellationToken ct = default);
    Task DeleteMediaAsync(Guid userId, Guid memoryId, Guid mediaId, CancellationToken ct = default);
}
