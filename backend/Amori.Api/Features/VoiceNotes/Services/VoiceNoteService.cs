using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Infrastructure.Relationships;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.VoiceNotes.Services;

public sealed class VoiceNoteService(
    AmoriDbContext db,
    IRelationshipAccessService relAccess) : IVoiceNoteService
{
    private static VoiceNoteResponse Map(VoiceNote v) => new()
    {
        Id = v.Id, UserId = v.UserId, RelationshipId = v.RelationshipId,
        Title = v.Title, FileKey = v.FileKey, DurationSeconds = v.DurationSeconds,
        Category = v.Category, IsFavorite = v.IsFavorite,
        CreatedAt = v.CreatedAt, UpdatedAt = v.UpdatedAt
    };

    public async Task<IReadOnlyList<VoiceNoteResponse>> GetAllAsync(Guid userId, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        return await db.VoiceNotes
            .Where(v => v.RelationshipId == rel.Id && !v.IsDeleted)
            .OrderByDescending(v => v.CreatedAt)
            .Select(v => Map(v)).ToListAsync(ct);
    }

    public async Task<VoiceNoteResponse> GetByIdAsync(Guid userId, Guid voiceNoteId, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var v = await db.VoiceNotes.FirstOrDefaultAsync(x => x.Id == voiceNoteId && !x.IsDeleted, ct)
            ?? throw new NotFoundException("Voice note", voiceNoteId);
        if (v.RelationshipId != rel.Id) throw new UnauthorizedException();
        return Map(v);
    }

    public async Task<VoiceNoteResponse> CreateAsync(Guid userId, CreateVoiceNoteRequest request, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        if (string.IsNullOrWhiteSpace(request.Title)) throw new ValidationException("Title is required.");
        if (string.IsNullOrWhiteSpace(request.FileKey)) throw new ValidationException("FileKey is required.");
        if (request.DurationSeconds <= 0) throw new ValidationException("DurationSeconds must be positive.");

        var v = new VoiceNote
        {
            UserId = userId, RelationshipId = rel.Id, Title = request.Title.Trim(),
            FileKey = request.FileKey, DurationSeconds = request.DurationSeconds, Category = request.Category
        };
        db.VoiceNotes.Add(v);
        await db.SaveChangesAsync(ct);
        return Map(v);
    }

    public async Task<VoiceNoteResponse> UpdateAsync(Guid userId, Guid voiceNoteId, UpdateVoiceNoteRequest request, CancellationToken ct = default)
    {
        var v = await db.VoiceNotes.FirstOrDefaultAsync(x => x.Id == voiceNoteId && !x.IsDeleted, ct)
            ?? throw new NotFoundException("Voice note", voiceNoteId);
        if (v.UserId != userId) throw new UnauthorizedException();
        if (request.Title != null) v.Title = request.Title.Trim();
        if (request.Category.HasValue) v.Category = request.Category.Value;
        await db.SaveChangesAsync(ct);
        return Map(v);
    }

    public async Task DeleteAsync(Guid userId, Guid voiceNoteId, CancellationToken ct = default)
    {
        var v = await db.VoiceNotes.FirstOrDefaultAsync(x => x.Id == voiceNoteId && !x.IsDeleted, ct)
            ?? throw new NotFoundException("Voice note", voiceNoteId);
        if (v.UserId != userId) throw new UnauthorizedException();
        v.IsDeleted = true;
        await db.SaveChangesAsync(ct);
    }

    public async Task<VoiceNoteResponse> FavoriteAsync(Guid userId, Guid voiceNoteId, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var v = await db.VoiceNotes.FirstOrDefaultAsync(x => x.Id == voiceNoteId && !x.IsDeleted, ct)
            ?? throw new NotFoundException("Voice note", voiceNoteId);
        if (v.RelationshipId != rel.Id) throw new UnauthorizedException();
        v.IsFavorite = true;
        await db.SaveChangesAsync(ct);
        return Map(v);
    }

    public async Task<VoiceNoteResponse> UnfavoriteAsync(Guid userId, Guid voiceNoteId, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var v = await db.VoiceNotes.FirstOrDefaultAsync(x => x.Id == voiceNoteId && !x.IsDeleted, ct)
            ?? throw new NotFoundException("Voice note", voiceNoteId);
        if (v.RelationshipId != rel.Id) throw new UnauthorizedException();
        v.IsFavorite = false;
        await db.SaveChangesAsync(ct);
        return Map(v);
    }
}
