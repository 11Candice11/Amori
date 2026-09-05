using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Domain.Enums;
using Amori.Api.Infrastructure.Authentication;
using Amori.Api.Infrastructure.Relationships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.BucketList.Controllers;

public sealed class CreateBucketListItemRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public BucketListCategory Category { get; set; }
    public DateOnly? TargetDate { get; set; }
    public string? Notes { get; set; }
}

public sealed class UpdateBucketListItemRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public BucketListCategory? Category { get; set; }
    public DateOnly? TargetDate { get; set; }
    public string? Notes { get; set; }
}

public sealed class BucketListItemResponse
{
    public Guid Id { get; init; }
    public Guid RelationshipId { get; init; }
    public Guid AddedByUserId { get; init; }
    public string AddedByName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Location { get; init; }
    public BucketListCategory Category { get; init; }
    public DateOnly? TargetDate { get; init; }
    public string? Notes { get; init; }
    public bool IsFavorite { get; init; }
    public bool IsCompleted { get; init; }
    public DateTime? CompletedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>Shared relationship bucket list.</summary>
[ApiController]
[Route("api/bucket-list")]
[Authorize]
public sealed class BucketListController(
    AmoriDbContext db,
    ICurrentUserService currentUser,
    IRelationshipAccessService relAccess) : ControllerBase
{
    private Guid RequireUserId() =>
        currentUser.UserId ?? throw new UnauthorizedException();

    private static BucketListItemResponse Map(BucketListItem b) => new()
    {
        Id = b.Id, RelationshipId = b.RelationshipId,
        AddedByUserId = b.AddedByUserId, AddedByName = b.AddedBy?.DisplayName ?? string.Empty,
        Title = b.Title, Description = b.Description, Location = b.Location,
        Category = b.Category, TargetDate = b.TargetDate, Notes = b.Notes,
        IsFavorite = b.IsFavorite, IsCompleted = b.IsCompleted,
        CompletedAt = b.CompletedAt, CreatedAt = b.CreatedAt, UpdatedAt = b.UpdatedAt
    };

    private async Task<(Relationship rel, BucketListItem item)> LoadAsync(Guid itemId, Guid userId, CancellationToken ct)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var item = await db.BucketListItems.Include(b => b.AddedBy)
            .FirstOrDefaultAsync(b => b.Id == itemId, ct)
            ?? throw new NotFoundException("Bucket list item", itemId);
        if (item.RelationshipId != rel.Id) throw new UnauthorizedException();
        return (rel, item);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var userId = RequireUserId();
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var items = await db.BucketListItems.Include(b => b.AddedBy)
            .Where(b => b.RelationshipId == rel.Id)
            .OrderByDescending(b => b.CreatedAt).ToListAsync(ct);
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
    public async Task<IActionResult> Create([FromBody] CreateBucketListItemRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        if (string.IsNullOrWhiteSpace(req.Title)) throw new ValidationException("Title is required.");

        var item = new BucketListItem
        {
            RelationshipId = rel.Id, AddedByUserId = userId,
            Title = req.Title.Trim(), Description = req.Description,
            Location = req.Location, Category = req.Category,
            TargetDate = req.TargetDate, Notes = req.Notes
        };
        db.BucketListItems.Add(item);
        await db.SaveChangesAsync(ct);
        item.AddedBy = (await db.Users.FindAsync([userId], ct))!;
        return CreatedAtAction(nameof(GetById), new { itemId = item.Id }, Map(item));
    }

    [HttpPatch("{itemId:guid}")]
    public async Task<IActionResult> Update(Guid itemId, [FromBody] UpdateBucketListItemRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, item) = await LoadAsync(itemId, userId, ct);
        if (req.Title != null) item.Title = req.Title.Trim();
        if (req.Description != null) item.Description = req.Description;
        if (req.Location != null) item.Location = req.Location;
        if (req.Category.HasValue) item.Category = req.Category.Value;
        if (req.TargetDate.HasValue) item.TargetDate = req.TargetDate;
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
        db.BucketListItems.Remove(item);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("{itemId:guid}/complete")]
    public async Task<IActionResult> Complete(Guid itemId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, item) = await LoadAsync(itemId, userId, ct);
        item.IsCompleted = true;
        item.CompletedAt ??= DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Ok(Map(item));
    }

    [HttpPost("{itemId:guid}/favorite")]
    public async Task<IActionResult> Favorite(Guid itemId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, item) = await LoadAsync(itemId, userId, ct);
        item.IsFavorite = !item.IsFavorite;
        await db.SaveChangesAsync(ct);
        return Ok(Map(item));
    }
}
