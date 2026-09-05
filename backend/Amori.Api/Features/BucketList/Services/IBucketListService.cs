using Amori.Api.Features.BucketList.Controllers;

namespace Amori.Api.Features.BucketList.Services;

/// <summary>
/// Business logic for the shared relationship bucket list.
/// </summary>
public interface IBucketListService
{
    Task<IReadOnlyList<BucketListItemResponse>> GetAllAsync(Guid userId, CancellationToken ct = default);
    Task<BucketListItemResponse> GetByIdAsync(Guid userId, Guid itemId, CancellationToken ct = default);
    Task<BucketListItemResponse> CreateAsync(Guid userId, CreateBucketListItemRequest request, CancellationToken ct = default);
    Task<BucketListItemResponse> UpdateAsync(Guid userId, Guid itemId, UpdateBucketListItemRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid userId, Guid itemId, CancellationToken ct = default);
    Task<BucketListItemResponse> CompleteAsync(Guid userId, Guid itemId, CancellationToken ct = default);
    Task<BucketListItemResponse> ToggleFavoriteAsync(Guid userId, Guid itemId, CancellationToken ct = default);
}
