using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Domain.Enums;
using Amori.Api.Infrastructure.Authentication;
using Amori.Api.Infrastructure.Relationships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.Wishlist.Controllers;

public sealed class CreateWishlistItemRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageKey { get; set; }
    public decimal? Price { get; set; }
    public string? Url { get; set; }
    public WishlistPriority Priority { get; set; } = WishlistPriority.Medium;
    public string? Notes { get; set; }
}

public sealed class UpdateWishlistItemRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ImageKey { get; set; }
    public decimal? Price { get; set; }
    public string? Url { get; set; }
    public WishlistPriority? Priority { get; set; }
    public string? Notes { get; set; }
}

public sealed class WishlistItemResponse
{
    public Guid Id { get; init; }
    public Guid RelationshipId { get; init; }
    public Guid AddedByUserId { get; init; }
    public string AddedByName { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? ImageKey { get; init; }
    public decimal? Price { get; init; }
    public string? Url { get; init; }
    public WishlistPriority Priority { get; init; }
    public string? Notes { get; init; }
    public bool IsPurchased { get; init; }
    public bool IsFavorite { get; init; }
    public DateTime? PurchasedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>Shared relationship wishlist.</summary>
[ApiController]
[Route("api/wishlist")]
[Authorize]
public sealed class WishlistController(
    AmoriDbContext db,
    ICurrentUserService currentUser,
    IRelationshipAccessService relAccess) : ControllerBase
{
    private Guid RequireUserId() =>
        currentUser.UserId ?? throw new UnauthorizedException();

    private static WishlistItemResponse Map(WishlistItem w) => new()
    {
        Id = w.Id, RelationshipId = w.RelationshipId,
        AddedByUserId = w.AddedByUserId, AddedByName = w.AddedBy?.DisplayName ?? string.Empty,
        Name = w.Name, Description = w.Description, ImageKey = w.ImageKey,
        Price = w.Price, Url = w.Url, Priority = w.Priority, Notes = w.Notes,
        IsPurchased = w.IsPurchased, IsFavorite = w.IsFavorite,
        PurchasedAt = w.PurchasedAt, CreatedAt = w.CreatedAt, UpdatedAt = w.UpdatedAt
    };

    private async Task<(Relationship rel, WishlistItem item)> LoadAsync(Guid itemId, Guid userId, CancellationToken ct)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var item = await db.WishlistItems.Include(w => w.AddedBy)
            .FirstOrDefaultAsync(w => w.Id == itemId, ct)
            ?? throw new NotFoundException("Wishlist item", itemId);
        if (item.RelationshipId != rel.Id) throw new UnauthorizedException();
        return (rel, item);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var userId = RequireUserId();
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var items = await db.WishlistItems.Include(w => w.AddedBy)
            .Where(w => w.RelationshipId == rel.Id)
            .OrderByDescending(w => w.CreatedAt).ToListAsync(ct);
        return Ok(items.Select(Map));
    }

    [HttpGet("{itemId:guid}")]
    public async Task<IActionResult> GetById(Guid itemId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, item) = await LoadAsync(itemId, userId, ct);
        return Ok(Map(item));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWishlistItemRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        if (string.IsNullOrWhiteSpace(req.Name)) throw new ValidationException("Name is required.");

        var item = new WishlistItem
        {
            RelationshipId = rel.Id, AddedByUserId = userId,
            Name = req.Name.Trim(), Description = req.Description,
            ImageKey = req.ImageKey, Price = req.Price, Url = req.Url,
            Priority = req.Priority, Notes = req.Notes
        };
        db.WishlistItems.Add(item);
        await db.SaveChangesAsync(ct);
        item.AddedBy = (await db.Users.FindAsync([userId], ct))!;
        return CreatedAtAction(nameof(GetById), new { itemId = item.Id }, Map(item));
    }

    [HttpPatch("{itemId:guid}")]
    public async Task<IActionResult> Update(Guid itemId, [FromBody] UpdateWishlistItemRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, item) = await LoadAsync(itemId, userId, ct);
        if (item.AddedByUserId != userId) throw new UnauthorizedException("Only the owner can update this item.");
        if (req.Name != null) item.Name = req.Name.Trim();
        if (req.Description != null) item.Description = req.Description;
        if (req.ImageKey != null) item.ImageKey = req.ImageKey;
        if (req.Price.HasValue) item.Price = req.Price;
        if (req.Url != null) item.Url = req.Url;
        if (req.Priority.HasValue) item.Priority = req.Priority.Value;
        if (req.Notes != null) item.Notes = req.Notes;
        await db.SaveChangesAsync(ct);
        return Ok(Map(item));
    }

    [HttpDelete("{itemId:guid}")]
    public async Task<IActionResult> Delete(Guid itemId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, item) = await LoadAsync(itemId, userId, ct);
        if (item.AddedByUserId != userId) throw new UnauthorizedException("Only the owner can delete this item.");
        db.WishlistItems.Remove(item);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("{itemId:guid}/complete")]
    public async Task<IActionResult> Complete(Guid itemId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, item) = await LoadAsync(itemId, userId, ct);
        item.IsPurchased = true;
        item.PurchasedAt ??= DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Ok(Map(item));
    }

    [HttpPost("{itemId:guid}/favorite")]
    public async Task<IActionResult> Favorite(Guid itemId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, item) = await LoadAsync(itemId, userId, ct);
        item.IsFavorite = true;
        await db.SaveChangesAsync(ct);
        return Ok(Map(item));
    }
}
