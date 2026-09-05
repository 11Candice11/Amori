using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Domain.Enums;
using Amori.Api.Infrastructure.Authentication;
using Amori.Api.Infrastructure.Relationships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.VoiceNotes;

// ── DTOs ─────────────────────────────────────────────────────────────────────

public sealed class CreateVoiceNoteRequest
{
    public string Title { get; set; } = string.Empty;
    public string FileKey { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public VoiceNoteCategory Category { get; set; }
}

public sealed class UpdateVoiceNoteRequest
{
    public string? Title { get; set; }
    public VoiceNoteCategory? Category { get; set; }
}

public sealed class VoiceNoteResponse
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid RelationshipId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string FileKey { get; init; } = string.Empty;
    public int DurationSeconds { get; init; }
    public VoiceNoteCategory Category { get; init; }
    public bool IsFavorite { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

// ── Controller ────────────────────────────────────────────────────────────────

/// <summary>
/// Voice note management. Files are stored in S3; only metadata is stored here.
/// </summary>
[ApiController]
[Route("api/voice-notes")]
[Authorize]
public sealed class VoiceNotesController(
    AmoriDbContext db,
    ICurrentUserService currentUser,
    IRelationshipAccessService relAccess) : ControllerBase
{
    private Guid RequireUserId() =>
        currentUser.UserId ?? throw new UnauthorizedException();

    private static VoiceNoteResponse Map(VoiceNote v) => new()
    {
        Id = v.Id,
        UserId = v.UserId,
        RelationshipId = v.RelationshipId,
        Title = v.Title,
        FileKey = v.FileKey,
        DurationSeconds = v.DurationSeconds,
        Category = v.Category,
        IsFavorite = v.IsFavorite,
        CreatedAt = v.CreatedAt,
        UpdatedAt = v.UpdatedAt
    };

    /// <summary>List all voice notes for the authenticated user's relationship.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<VoiceNoteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var userId = RequireUserId();
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");

        var items = await db.VoiceNotes
            .Where(v => v.RelationshipId == rel.Id && !v.IsDeleted)
            .OrderByDescending(v => v.CreatedAt)
            .Select(v => Map(v))
            .ToListAsync(ct);

        return Ok(items);
    }

    /// <summary>Get a voice note by ID.</summary>
    [HttpGet("{voiceNoteId:guid}")]
    [ProducesResponseType(typeof(VoiceNoteResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid voiceNoteId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");

        var v = await db.VoiceNotes.FirstOrDefaultAsync(x => x.Id == voiceNoteId && !x.IsDeleted, ct)
            ?? throw new NotFoundException("Voice note", voiceNoteId);

        if (v.RelationshipId != rel.Id) throw new UnauthorizedException();
        return Ok(Map(v));
    }

    /// <summary>Create a voice note entry (after the file is uploaded to S3).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(VoiceNoteResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateVoiceNoteRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");

        if (string.IsNullOrWhiteSpace(req.Title)) throw new ValidationException("Title is required.");
        if (string.IsNullOrWhiteSpace(req.FileKey)) throw new ValidationException("FileKey is required.");
        if (req.DurationSeconds <= 0) throw new ValidationException("DurationSeconds must be positive.");

        var v = new VoiceNote
        {
            UserId = userId,
            RelationshipId = rel.Id,
            Title = req.Title.Trim(),
            FileKey = req.FileKey,
            DurationSeconds = req.DurationSeconds,
            Category = req.Category
        };

        db.VoiceNotes.Add(v);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { voiceNoteId = v.Id }, Map(v));
    }

    /// <summary>Update title or category of a voice note.</summary>
    [HttpPatch("{voiceNoteId:guid}")]
    [ProducesResponseType(typeof(VoiceNoteResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid voiceNoteId, [FromBody] UpdateVoiceNoteRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var v = await db.VoiceNotes.FirstOrDefaultAsync(x => x.Id == voiceNoteId && !x.IsDeleted, ct)
            ?? throw new NotFoundException("Voice note", voiceNoteId);
        if (v.UserId != userId) throw new UnauthorizedException();
        if (req.Title != null) v.Title = req.Title.Trim();
        if (req.Category.HasValue) v.Category = req.Category.Value;
        await db.SaveChangesAsync(ct);
        return Ok(Map(v));
    }

    /// <summary>Soft-delete a voice note.</summary>
    [HttpDelete("{voiceNoteId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid voiceNoteId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var v = await db.VoiceNotes.FirstOrDefaultAsync(x => x.Id == voiceNoteId && !x.IsDeleted, ct)
            ?? throw new NotFoundException("Voice note", voiceNoteId);
        if (v.UserId != userId) throw new UnauthorizedException();
        v.IsDeleted = true;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>Favorite a voice note.</summary>
    [HttpPost("{voiceNoteId:guid}/favorite")]
    [ProducesResponseType(typeof(VoiceNoteResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Favorite(Guid voiceNoteId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var v = await db.VoiceNotes.FirstOrDefaultAsync(x => x.Id == voiceNoteId && !x.IsDeleted, ct)
            ?? throw new NotFoundException("Voice note", voiceNoteId);
        if (v.RelationshipId != rel.Id) throw new UnauthorizedException();
        v.IsFavorite = true;
        await db.SaveChangesAsync(ct);
        return Ok(Map(v));
    }

    /// <summary>Unfavorite a voice note.</summary>
    [HttpDelete("{voiceNoteId:guid}/favorite")]
    [ProducesResponseType(typeof(VoiceNoteResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Unfavorite(Guid voiceNoteId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var v = await db.VoiceNotes.FirstOrDefaultAsync(x => x.Id == voiceNoteId && !x.IsDeleted, ct)
            ?? throw new NotFoundException("Voice note", voiceNoteId);
        if (v.RelationshipId != rel.Id) throw new UnauthorizedException();
        v.IsFavorite = false;
        await db.SaveChangesAsync(ct);
        return Ok(Map(v));
    }
}
