using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Domain.Enums;
using Amori.Api.Infrastructure.Authentication;
using Amori.Api.Infrastructure.Relationships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.Messages;

// ── DTOs ─────────────────────────────────────────────────────────────────────

public sealed class SendMessageRequest
{
    public string? Text { get; set; }
    public string? ImageKey { get; set; }
    public string? VoiceNoteKey { get; set; }
    public MessageCategory Category { get; set; } = MessageCategory.General;
}

public sealed class UpdateMessageRequest
{
    public string? Text { get; set; }
    public MessageCategory? Category { get; set; }
}

public sealed class MessageResponse
{
    public Guid Id { get; init; }
    public Guid RelationshipId { get; init; }
    public Guid SenderId { get; init; }
    public string SenderName { get; init; } = string.Empty;
    public Guid RecipientId { get; init; }
    public string? Text { get; init; }
    public string? ImageKey { get; init; }
    public string? VoiceNoteKey { get; init; }
    public MessageCategory Category { get; init; }
    public bool IsRead { get; init; }
    public DateTime? ReadAt { get; init; }
    public bool IsFavorite { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

// ── Controller ────────────────────────────────────────────────────────────────

/// <summary>Personal messages between partners.</summary>
[ApiController]
[Route("api/messages")]
[Authorize]
public sealed class MessagesController(
    AmoriDbContext db,
    ICurrentUserService currentUser,
    IRelationshipAccessService relAccess) : ControllerBase
{
    private Guid RequireUserId() =>
        currentUser.UserId ?? throw new UnauthorizedException();

    private static MessageResponse Map(Message m) => new()
    {
        Id = m.Id,
        RelationshipId = m.RelationshipId,
        SenderId = m.SenderId,
        SenderName = m.Sender?.DisplayName ?? string.Empty,
        RecipientId = m.RecipientId,
        Text = m.Text,
        ImageKey = m.ImageKey,
        VoiceNoteKey = m.VoiceNoteKey,
        Category = m.Category,
        IsRead = m.ReadAt.HasValue,
        ReadAt = m.ReadAt,
        IsFavorite = m.IsFavorite,
        CreatedAt = m.CreatedAt,
        UpdatedAt = m.UpdatedAt
    };

    private async Task<(Relationship rel, User partner)> RequireRelationshipAsync(Guid userId, CancellationToken ct)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var partner = await relAccess.GetPartnerAsync(userId)
            ?? throw new ConflictException("Your relationship does not have a second partner yet.");
        return (rel, partner);
    }

    /// <summary>List all messages in the authenticated user's relationship.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MessageResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMessages(CancellationToken ct)
    {
        var userId = RequireUserId();
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");

        var msgs = await db.Messages
            .Include(m => m.Sender)
            .Where(m => m.RelationshipId == rel.Id && !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);

        return Ok(msgs.Select(Map).ToList());
    }

    /// <summary>Get a message by ID.</summary>
    [HttpGet("{messageId:guid}")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMessage(Guid messageId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");

        var m = await db.Messages.Include(x => x.Sender)
            .FirstOrDefaultAsync(x => x.Id == messageId && !x.IsDeleted, ct)
            ?? throw new NotFoundException("Message", messageId);

        if (m.RelationshipId != rel.Id) throw new UnauthorizedException();
        return Ok(Map(m));
    }

    /// <summary>Send a message to your partner.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (rel, partner) = await RequireRelationshipAsync(userId, ct);

        if (string.IsNullOrWhiteSpace(req.Text) && req.ImageKey == null && req.VoiceNoteKey == null)
            throw new ValidationException("A message must contain text, an image, or a voice note.");

        var msg = new Message
        {
            RelationshipId = rel.Id,
            SenderId = userId,
            RecipientId = partner.Id,
            Text = req.Text,
            ImageKey = req.ImageKey,
            VoiceNoteKey = req.VoiceNoteKey,
            Category = req.Category
        };

        db.Messages.Add(msg);
        await db.SaveChangesAsync(ct);

        msg.Sender = (await db.Users.FindAsync([userId], ct))!;
        return CreatedAtAction(nameof(GetMessage), new { messageId = msg.Id }, Map(msg));
    }

    /// <summary>Update a message (sender only).</summary>
    [HttpPatch("{messageId:guid}")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateMessage(Guid messageId, [FromBody] UpdateMessageRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var m = await db.Messages.Include(x => x.Sender)
            .FirstOrDefaultAsync(x => x.Id == messageId && !x.IsDeleted, ct)
            ?? throw new NotFoundException("Message", messageId);

        if (m.SenderId != userId) throw new UnauthorizedException("Only the sender can update a message.");
        if (req.Text != null) m.Text = req.Text;
        if (req.Category.HasValue) m.Category = req.Category.Value;

        await db.SaveChangesAsync(ct);
        return Ok(Map(m));
    }

    /// <summary>Delete a message (soft-delete, sender only).</summary>
    [HttpDelete("{messageId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteMessage(Guid messageId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var m = await db.Messages.FirstOrDefaultAsync(x => x.Id == messageId && !x.IsDeleted, ct)
            ?? throw new NotFoundException("Message", messageId);
        if (m.SenderId != userId) throw new UnauthorizedException("Only the sender can delete a message.");
        m.IsDeleted = true;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>Mark a message as read (recipient only).</summary>
    [HttpPost("{messageId:guid}/read")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkRead(Guid messageId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var m = await db.Messages.Include(x => x.Sender)
            .FirstOrDefaultAsync(x => x.Id == messageId && !x.IsDeleted, ct)
            ?? throw new NotFoundException("Message", messageId);
        if (m.RecipientId != userId) throw new UnauthorizedException("Only the recipient can mark a message as read.");
        m.ReadAt ??= DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Ok(Map(m));
    }

    /// <summary>Favorite a message.</summary>
    [HttpPost("{messageId:guid}/favorite")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Favorite(Guid messageId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var m = await db.Messages.Include(x => x.Sender)
            .FirstOrDefaultAsync(x => x.Id == messageId && !x.IsDeleted, ct)
            ?? throw new NotFoundException("Message", messageId);
        if (m.RelationshipId != rel.Id) throw new UnauthorizedException();
        m.IsFavorite = true;
        await db.SaveChangesAsync(ct);
        return Ok(Map(m));
    }

    /// <summary>Unfavorite a message.</summary>
    [HttpDelete("{messageId:guid}/favorite")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Unfavorite(Guid messageId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var m = await db.Messages.Include(x => x.Sender)
            .FirstOrDefaultAsync(x => x.Id == messageId && !x.IsDeleted, ct)
            ?? throw new NotFoundException("Message", messageId);
        if (m.RelationshipId != rel.Id) throw new UnauthorizedException();
        m.IsFavorite = false;
        await db.SaveChangesAsync(ct);
        return Ok(Map(m));
    }
}
