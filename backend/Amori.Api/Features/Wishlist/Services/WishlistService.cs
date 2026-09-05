using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Features.Wishlist.Controllers;
using Amori.Api.Infrastructure.Relationships;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.Wishlist.Services;

public sealed class WishlistService(
    AmoriDbContext db,
    IRelationshipAccessService relAccess) : IWishlistService
{
    private static WishlistItemResponse Map(WishlistItem w) => new()
    {
        Id = w.Id, RelationshipId = w.RelationshipId,
        AddedByUserId = w.AddedByUserId, AddedByName = w.AddedBy?.DisplayName ?? string.Empty,
        Name = w.Name, Description = w.Description, ImageKey = w.ImageKey,
        Price = w.Price, Url = w.Url, Priority = w.Priority, Notes = w.Notes,
        IsPurchased = w.IsPurchased, IsFavorite = w.IsFavorite,
        PurchasedAt = w.PurchasedAt, CreatedAt = w.CreatedAt, UpdatedAt = w.UpdatedAt
    };

    private async Task<(Guid relId, WishlistItem item)> LoadAsync(Guid userId, Guid itemId, CancellationToken ct)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var item = await db.WishlistItems.Include(w => w.AddedBy)
            .FirstOrDefaultAsync(w => w.Id == itemId, ct)
            ?? throw new NotFoundException("Wishlist item", itemId);
        if (item.RelationshipId != rel.Id) throw new UnauthorizedException();
        return (rel.Id, item);
    }

    public async Task<IReadOnlyList<WishlistItemResponse>> GetAllAsync(Guid userId, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        return await db.WishlistItems.Include(w => w.AddedBy)
            .Where(w => w.RelationshipId == rel.Id)
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => Map(w)).ToListAsync(ct);
    }

    public async Task<WishlistItemResponse> GetByIdAsync(Guid userId, Guid itemId, CancellationToken ct = default)
    {
        var (_, item) = await LoadAsync(userId, itemId, ct);
        return Map(item);
    }

    public async Task<WishlistItemResponse> CreateAsync(Guid userId, CreateWishlistItemRequest request, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        if (string.IsNullOrWhiteSpace(request.Name)) throw new ValidationException("Name is required.");

        var item = new WishlistItem
        {
            RelationshipId = rel.Id, AddedByUserId = userId,
            Name = request.Name.Trim(), Description = request.Description,
            ImageKey = request.ImageKey, Price = request.Price, Url = request.Url,
            Priority = request.Priority, Notes = request.Notes
        };
        db.WishlistItems.Add(item);
        await db.SaveChangesAsync(ct);
        item.AddedBy = (await db.Users.FindAsync([userId], ct))!;
        return Map(item);
    }

    public async Task<WishlistItemResponse> UpdateAsync(Guid userId, Guid itemId, UpdateWishlistItemRequest request, CancellationToken ct = default)
    {
        var (_, item) = await LoadAsync(userId, itemId, ct);
        if (item.AddedByUserId != userId) throw new UnauthorizedException("Only the owner can update this item.");
        if (request.Name != null) item.Name = request.Name.Trim();
        if (request.Description != null) item.Description = request.Description;
        if (request.ImageKey != null) item.ImageKey = request.ImageKey;
        if (request.Price.HasValue) item.Price = request.Price;
        if (request.Url != null) item.Url = request.Url;
        if (request.Priority.HasValue) item.Priority = request.Priority.Value;
        if (request.Notes != null) item.Notes = request.Notes;
        await db.SaveChangesAsync(ct);
        return Map(item);
    }

    public async Task DeleteAsync(Guid userId, Guid itemId, CancellationToken ct = default)
    {
        var (_, item) = await LoadAsync(userId, itemId, ct);
        if (item.AddedByUserId != userId) throw new UnauthorizedException("Only the owner can delete this item.");
        db.WishlistItems.Remove(item);
        await db.SaveChangesAsync(ct);
    }

    public async Task<WishlistItemResponse> CompleteAsync(Guid userId, Guid itemId, CancellationToken ct = default)
    {
        var (_, item) = await LoadAsync(userId, itemId, ct);
        item.IsPurchased = true;
        item.PurchasedAt ??= DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Map(item);
    }

    public async Task<WishlistItemResponse> FavoriteAsync(Guid userId, Guid itemId, CancellationToken ct = default)
    {
        var (_, item) = await LoadAsync(userId, itemId, ct);
        item.IsFavorite = true;
        await db.SaveChangesAsync(ct);
        return Map(item);
    }
}
