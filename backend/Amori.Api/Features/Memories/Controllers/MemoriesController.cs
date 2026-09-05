using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Domain.Enums;
using Amori.Api.Infrastructure.Authentication;
using Amori.Api.Infrastructure.Relationships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.Memories;

// ── DTOs ─────────────────────────────────────────────────────────────────────

public sealed class CreateMemoryRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly? MemoryDate { get; set; }
    public string? Location { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public List<string> Tags { get; set; } = [];
}

public sealed class UpdateMemoryRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateOnly? MemoryDate { get; set; }
    public string? Location { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public List<string>? Tags { get; set; }
}

public sealed class AddMemoryMediaRequest
{
    public string FileKey { get; set; } = string.Empty;
    public MemoryMediaType MediaType { get; set; }
    public int? DurationSeconds { get; set; }
}

public sealed class MemoryMediaResponse
{
    public Guid Id { get; init; }
    public string FileKey { get; init; } = string.Empty;
    public MemoryMediaType MediaType { get; init; }
    public int? DurationSeconds { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class MemoryResponse
{
    public Guid Id { get; init; }
    public Guid RelationshipId { get; init; }
    public Guid CreatedByUserId { get; init; }
    public string CreatedByName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateOnly? MemoryDate { get; init; }
    public string? Location { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public List<string> Tags { get; init; } = [];
    public bool IsFavorite { get; init; }
    public IReadOnlyList<MemoryMediaResponse> Media { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

// ── Controller ────────────────────────────────────────────────────────────────

/// <summary>Shared relationship memories with photo and voice note support.</summary>
[ApiController]
[Route("api/memories")]
[Authorize]
public sealed class MemoriesController(
    AmoriDbContext db,
    ICurrentUserService currentUser,
    IRelationshipAccessService relAccess) : ControllerBase
{
    private Guid RequireUserId() =>
        currentUser.UserId ?? throw new UnauthorizedException();

    private static MemoryResponse Map(Memory m) => new()
    {
        Id = m.Id,
        RelationshipId = m.RelationshipId,
        CreatedByUserId = m.CreatedByUserId,
        CreatedByName = m.CreatedBy?.DisplayName ?? string.Empty,
        Title = m.Title,
        Description = m.Description,
        MemoryDate = m.MemoryDate,
        Location = m.Location,
        Latitude = m.Latitude,
        Longitude = m.Longitude,
        Tags = m.Tags,
        IsFavorite = m.IsFavorite,
        Media = m.Media.Select(mm => new MemoryMediaResponse
        {
            Id = mm.Id,
            FileKey = mm.FileKey,
            MediaType = mm.MediaType,
            DurationSeconds = mm.DurationSeconds,
            CreatedAt = mm.CreatedAt
        }).ToList(),
        CreatedAt = m.CreatedAt,
        UpdatedAt = m.UpdatedAt
    };

    private async Task<(Relationship rel, Memory memory)> LoadAndAuthorizeAsync(Guid memoryId, Guid userId, CancellationToken ct)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var memory = await db.Memories
            .Include(m => m.CreatedBy)
            .Include(m => m.Media)
            .FirstOrDefaultAsync(m => m.Id == memoryId && !m.IsDeleted, ct)
            ?? throw new NotFoundException("Memory", memoryId);
        if (memory.RelationshipId != rel.Id) throw new UnauthorizedException();
        return (rel, memory);
    }

    /// <summary>List all memories for the authenticated user's relationship.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var userId = RequireUserId();
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");

        var memories = await db.Memories
            .Include(m => m.CreatedBy)
            .Include(m => m.Media)
            .Where(m => m.RelationshipId == rel.Id && !m.IsDeleted)
            .OrderByDescending(m => m.MemoryDate ?? m.CreatedAt.GetDateOnly())
            .ToListAsync(ct);

        return Ok(memories.Select(Map).ToList());
    }

    /// <summary>Get a memory by ID.</summary>
    [HttpGet("{memoryId:guid}")]
    public async Task<IActionResult> GetById(Guid memoryId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, memory) = await LoadAndAuthorizeAsync(memoryId, userId, ct);
        return Ok(Map(memory));
    }

    /// <summary>Create a memory.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMemoryRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");

        if (string.IsNullOrWhiteSpace(req.Title)) throw new ValidationException("Title is required.");

        var memory = new Memory
        {
            RelationshipId = rel.Id,
            CreatedByUserId = userId,
            Title = req.Title.Trim(),
            Description = req.Description,
            MemoryDate = req.MemoryDate,
            Location = req.Location,
            Latitude = req.Latitude,
            Longitude = req.Longitude,
            Tags = req.Tags
        };

        db.Memories.Add(memory);
        await db.SaveChangesAsync(ct);
        memory.CreatedBy = (await db.Users.FindAsync([userId], ct))!;
        return CreatedAtAction(nameof(GetById), new { memoryId = memory.Id }, Map(memory));
    }

    /// <summary>Update a memory.</summary>
    [HttpPatch("{memoryId:guid}")]
    public async Task<IActionResult> Update(Guid memoryId, [FromBody] UpdateMemoryRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, memory) = await LoadAndAuthorizeAsync(memoryId, userId, ct);
        if (memory.CreatedByUserId != userId) throw new UnauthorizedException("Only the creator can update this memory.");
        if (req.Title != null) memory.Title = req.Title.Trim();
        if (req.Description != null) memory.Description = req.Description;
        if (req.MemoryDate.HasValue) memory.MemoryDate = req.MemoryDate;
        if (req.Location != null) memory.Location = req.Location;
        if (req.Latitude.HasValue) memory.Latitude = req.Latitude;
        if (req.Longitude.HasValue) memory.Longitude = req.Longitude;
        if (req.Tags != null) memory.Tags = req.Tags;
        await db.SaveChangesAsync(ct);
        return Ok(Map(memory));
    }

    /// <summary>Delete a memory (soft-delete).</summary>
    [HttpDelete("{memoryId:guid}")]
    public async Task<IActionResult> Delete(Guid memoryId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, memory) = await LoadAndAuthorizeAsync(memoryId, userId, ct);
        if (memory.CreatedByUserId != userId) throw new UnauthorizedException("Only the creator can delete this memory.");
        memory.IsDeleted = true;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>Toggle favorite on a memory.</summary>
    [HttpPost("{memoryId:guid}/favorite")]
    public async Task<IActionResult> Favorite(Guid memoryId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, memory) = await LoadAndAuthorizeAsync(memoryId, userId, ct);
        memory.IsFavorite = true;
        await db.SaveChangesAsync(ct);
        return Ok(Map(memory));
    }

    [HttpDelete("{memoryId:guid}/favorite")]
    public async Task<IActionResult> Unfavorite(Guid memoryId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, memory) = await LoadAndAuthorizeAsync(memoryId, userId, ct);
        memory.IsFavorite = false;
        await db.SaveChangesAsync(ct);
        return Ok(Map(memory));
    }

    /// <summary>Add media to a memory (file must already be uploaded to S3).</summary>
    [HttpPost("{memoryId:guid}/media")]
    public async Task<IActionResult> AddMedia(Guid memoryId, [FromBody] AddMemoryMediaRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, memory) = await LoadAndAuthorizeAsync(memoryId, userId, ct);
        if (string.IsNullOrWhiteSpace(req.FileKey)) throw new ValidationException("FileKey is required.");

        var media = new MemoryMedia
        {
            MemoryId = memory.Id,
            FileKey = req.FileKey,
            MediaType = req.MediaType,
            DurationSeconds = req.DurationSeconds
        };

        db.MemoryMedia.Add(media);
        await db.SaveChangesAsync(ct);
        return Ok(Map(memory));
    }

    /// <summary>Remove a media item from a memory.</summary>
    [HttpDelete("{memoryId:guid}/media/{mediaId:guid}")]
    public async Task<IActionResult> DeleteMedia(Guid memoryId, Guid mediaId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, memory) = await LoadAndAuthorizeAsync(memoryId, userId, ct);
        if (memory.CreatedByUserId != userId) throw new UnauthorizedException();

        var media = memory.Media.FirstOrDefault(m => m.Id == mediaId)
            ?? throw new NotFoundException("Media", mediaId);

        db.MemoryMedia.Remove(media);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}

// Extension helper to avoid confusion with DateOnly/DateTime
file static class DateTimeExtensions
{
    public static DateOnly GetDateOnly(this DateTime dt) => DateOnly.FromDateTime(dt);
}
