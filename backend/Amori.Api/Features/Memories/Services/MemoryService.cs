using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Infrastructure.Relationships;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.Memories.Services;

public sealed class MemoryService(
    AmoriDbContext db,
    IRelationshipAccessService relAccess) : IMemoryService
{
    private static MemoryResponse Map(Memory m) => new()
    {
        Id = m.Id, RelationshipId = m.RelationshipId,
        CreatedByUserId = m.CreatedByUserId, CreatedByName = m.CreatedBy?.DisplayName ?? string.Empty,
        Title = m.Title, Description = m.Description, MemoryDate = m.MemoryDate,
        Location = m.Location, Latitude = m.Latitude, Longitude = m.Longitude,
        Tags = m.Tags, IsFavorite = m.IsFavorite,
        Media = m.Media.Select(mm => new MemoryMediaResponse
        {
            Id = mm.Id, FileKey = mm.FileKey, MediaType = mm.MediaType,
            DurationSeconds = mm.DurationSeconds, CreatedAt = mm.CreatedAt
        }).ToList(),
        CreatedAt = m.CreatedAt, UpdatedAt = m.UpdatedAt
    };

    private async Task<(Guid relId, Memory memory)> LoadAsync(Guid userId, Guid memoryId, CancellationToken ct)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var memory = await db.Memories
            .Include(m => m.CreatedBy).Include(m => m.Media)
            .FirstOrDefaultAsync(m => m.Id == memoryId && !m.IsDeleted, ct)
            ?? throw new NotFoundException("Memory", memoryId);
        if (memory.RelationshipId != rel.Id) throw new UnauthorizedException();
        return (rel.Id, memory);
    }

    public async Task<IReadOnlyList<MemoryResponse>> GetAllAsync(Guid userId, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        return await db.Memories
            .Include(m => m.CreatedBy).Include(m => m.Media)
            .Where(m => m.RelationshipId == rel.Id && !m.IsDeleted)
            .OrderByDescending(m => m.MemoryDate ?? DateOnly.FromDateTime(m.CreatedAt))
            .Select(m => Map(m)).ToListAsync(ct);
    }

    public async Task<MemoryResponse> GetByIdAsync(Guid userId, Guid memoryId, CancellationToken ct = default)
    {
        var (_, memory) = await LoadAsync(userId, memoryId, ct);
        return Map(memory);
    }

    public async Task<MemoryResponse> CreateAsync(Guid userId, CreateMemoryRequest request, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        if (string.IsNullOrWhiteSpace(request.Title)) throw new ValidationException("Title is required.");

        var memory = new Memory
        {
            RelationshipId = rel.Id, CreatedByUserId = userId,
            Title = request.Title.Trim(), Description = request.Description,
            MemoryDate = request.MemoryDate, Location = request.Location,
            Latitude = request.Latitude, Longitude = request.Longitude, Tags = request.Tags
        };
        db.Memories.Add(memory);
        await db.SaveChangesAsync(ct);
        memory.CreatedBy = (await db.Users.FindAsync([userId], ct))!;
        return Map(memory);
    }

    public async Task<MemoryResponse> UpdateAsync(Guid userId, Guid memoryId, UpdateMemoryRequest request, CancellationToken ct = default)
    {
        var (_, memory) = await LoadAsync(userId, memoryId, ct);
        if (memory.CreatedByUserId != userId) throw new UnauthorizedException("Only the creator can update this memory.");
        if (request.Title != null) memory.Title = request.Title.Trim();
        if (request.Description != null) memory.Description = request.Description;
        if (request.MemoryDate.HasValue) memory.MemoryDate = request.MemoryDate;
        if (request.Location != null) memory.Location = request.Location;
        if (request.Latitude.HasValue) memory.Latitude = request.Latitude;
        if (request.Longitude.HasValue) memory.Longitude = request.Longitude;
        if (request.Tags != null) memory.Tags = request.Tags;
        await db.SaveChangesAsync(ct);
        return Map(memory);
    }

    public async Task DeleteAsync(Guid userId, Guid memoryId, CancellationToken ct = default)
    {
        var (_, memory) = await LoadAsync(userId, memoryId, ct);
        if (memory.CreatedByUserId != userId) throw new UnauthorizedException("Only the creator can delete this memory.");
        memory.IsDeleted = true;
        await db.SaveChangesAsync(ct);
    }

    public async Task<MemoryResponse> FavoriteAsync(Guid userId, Guid memoryId, CancellationToken ct = default)
    {
        var (_, memory) = await LoadAsync(userId, memoryId, ct);
        memory.IsFavorite = true;
        await db.SaveChangesAsync(ct);
        return Map(memory);
    }

    public async Task<MemoryResponse> UnfavoriteAsync(Guid userId, Guid memoryId, CancellationToken ct = default)
    {
        var (_, memory) = await LoadAsync(userId, memoryId, ct);
        memory.IsFavorite = false;
        await db.SaveChangesAsync(ct);
        return Map(memory);
    }

    public async Task<MemoryResponse> AddMediaAsync(Guid userId, Guid memoryId, AddMemoryMediaRequest request, CancellationToken ct = default)
    {
        var (_, memory) = await LoadAsync(userId, memoryId, ct);
        if (string.IsNullOrWhiteSpace(request.FileKey)) throw new ValidationException("FileKey is required.");

        var media = new MemoryMedia
        {
            MemoryId = memory.Id, FileKey = request.FileKey,
            MediaType = request.MediaType, DurationSeconds = request.DurationSeconds
        };
        db.MemoryMedia.Add(media);
        await db.SaveChangesAsync(ct);
        return Map(memory);
    }

    public async Task DeleteMediaAsync(Guid userId, Guid memoryId, Guid mediaId, CancellationToken ct = default)
    {
        var (_, memory) = await LoadAsync(userId, memoryId, ct);
        if (memory.CreatedByUserId != userId) throw new UnauthorizedException();
        var media = memory.Media.FirstOrDefault(m => m.Id == mediaId)
            ?? throw new NotFoundException("Media", mediaId);
        db.MemoryMedia.Remove(media);
        await db.SaveChangesAsync(ct);
    }
}
