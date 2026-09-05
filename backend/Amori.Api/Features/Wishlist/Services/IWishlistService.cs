using Amori.Api.Features.Wishlist.Controllers;

namespace Amori.Api.Features.Wishlist.Services;

/// <summary>
/// Business logic for the shared relationship wishlist.
/// </summary>
public interface IWishlistService
{
    Task<IReadOnlyList<WishlistItemResponse>> GetAllAsync(Guid userId, CancellationToken ct = default);
    Task<WishlistItemResponse> GetByIdAsync(Guid userId, Guid itemId, CancellationToken ct = default);
    Task<WishlistItemResponse> CreateAsync(Guid userId, CreateWishlistItemRequest request, CancellationToken ct = default);
    Task<WishlistItemResponse> UpdateAsync(Guid userId, Guid itemId, UpdateWishlistItemRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid userId, Guid itemId, CancellationToken ct = default);
    Task<WishlistItemResponse> CompleteAsync(Guid userId, Guid itemId, CancellationToken ct = default);
    Task<WishlistItemResponse> FavoriteAsync(Guid userId, Guid itemId, CancellationToken ct = default);
}
