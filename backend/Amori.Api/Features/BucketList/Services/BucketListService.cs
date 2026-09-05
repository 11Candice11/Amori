using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Features.BucketList.Controllers;
using Amori.Api.Infrastructure.Relationships;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.BucketList.Services;

public sealed class BucketListService(
    AmoriDbContext db,
    IRelationshipAccessService relAccess) : IBucketListService
{
    private static BucketListItemResponse Map(BucketListItem b) => new()
    {
        Id = b.Id, RelationshipId = b.RelationshipId,
        AddedByUserId = b.AddedByUserId, AddedByName = b.AddedBy?.DisplayName ?? string.Empty,
        Title = b.Title, Description = b.Description, Location = b.Location,
        Category = b.Category, TargetDate = b.TargetDate, Notes = b.Notes,
        IsFavorite = b.IsFavorite, IsCompleted = b.IsCompleted,
        CompletedAt = b.CompletedAt, CreatedAt = b.CreatedAt, UpdatedAt = b.UpdatedAt
    };

    private async Task<(Guid relId, BucketListItem item)> LoadAsync(Guid userId, Guid itemId, CancellationToken ct)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var item = await db.BucketListItems.Include(b => b.AddedBy)
            .FirstOrDefaultAsync(b => b.Id == itemId, ct)
            ?? throw new NotFoundException("Bucket list item", itemId);
        if (item.RelationshipId != rel.Id) throw new UnauthorizedException();
        return (rel.Id, item);
    }

    public async Task<IReadOnlyList<BucketListItemResponse>> GetAllAsync(Guid userId, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        return await db.BucketListItems.Include(b => b.AddedBy)
            .Where(b => b.RelationshipId == rel.Id)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => Map(b))
            .ToListAsync(ct);
    }

    public async Task<BucketListItemResponse> GetByIdAsync(Guid userId, Guid itemId, CancellationToken ct = default)
    {
        var (_, item) = await LoadAsync(userId, itemId, ct);
        return Map(item);
    }

    public async Task<BucketListItemResponse> CreateAsync(Guid userId, CreateBucketListItemRequest request, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        if (string.IsNullOrWhiteSpace(request.Title)) throw new ValidationException("Title is required.");

        var item = new BucketListItem
        {
            RelationshipId = rel.Id, AddedByUserId = userId,
            Title = request.Title.Trim(), Description = request.Description,
            Location = request.Location, Category = request.Category,
            TargetDate = request.TargetDate, Notes = request.Notes
        };
        db.BucketListItems.Add(item);
        await db.SaveChangesAsync(ct);
        item.AddedBy = (await db.Users.FindAsync([userId], ct))!;
        return Map(item);
    }

    public async Task<BucketListItemResponse> UpdateAsync(Guid userId, Guid itemId, UpdateBucketListItemRequest request, CancellationToken ct = default)
    {
        var (_, item) = await LoadAsync(userId, itemId, ct);
        if (request.Title != null) item.Title = request.Title.Trim();
        if (request.Description != null) item.Description = request.Description;
        if (request.Location != null) item.Location = request.Location;
        if (request.Category.HasValue) item.Category = request.Category.Value;
        if (request.TargetDate.HasValue) item.TargetDate = request.TargetDate;
        if (request.Notes != null) item.Notes = request.Notes;
        await db.SaveChangesAsync(ct);
        return Map(item);
    }

    public async Task DeleteAsync(Guid userId, Guid itemId, CancellationToken ct = default)
    {
        var (_, item) = await LoadAsync(userId, itemId, ct);
        if (item.AddedByUserId != userId) throw new UnauthorizedException("Only the owner can delete this item.");
        db.BucketListItems.Remove(item);
        await db.SaveChangesAsync(ct);
    }

    public async Task<BucketListItemResponse> CompleteAsync(Guid userId, Guid itemId, CancellationToken ct = default)
    {
        var (_, item) = await LoadAsync(userId, itemId, ct);
        item.IsCompleted = true;
        item.CompletedAt ??= DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Map(item);
    }

    public async Task<BucketListItemResponse> ToggleFavoriteAsync(Guid userId, Guid itemId, CancellationToken ct = default)
    {
        var (_, item) = await LoadAsync(userId, itemId, ct);
        item.IsFavorite = !item.IsFavorite;
        await db.SaveChangesAsync(ct);
        return Map(item);
    }
}
