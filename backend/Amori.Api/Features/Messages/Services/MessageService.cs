using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Infrastructure.Relationships;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.Messages.Services;

public sealed class MessageService(
    AmoriDbContext db,
    IRelationshipAccessService relAccess) : IMessageService
{
    private static MessageResponse Map(Message m) => new()
    {
        Id = m.Id, RelationshipId = m.RelationshipId,
        SenderId = m.SenderId, SenderName = m.Sender?.DisplayName ?? string.Empty,
        RecipientId = m.RecipientId, Text = m.Text, ImageKey = m.ImageKey,
        VoiceNoteKey = m.VoiceNoteKey, Category = m.Category,
        IsRead = m.ReadAt.HasValue, ReadAt = m.ReadAt,
        IsFavorite = m.IsFavorite, CreatedAt = m.CreatedAt, UpdatedAt = m.UpdatedAt
    };

    public async Task<IReadOnlyList<MessageResponse>> GetMessagesAsync(Guid userId, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        return await db.Messages.Include(m => m.Sender)
            .Where(m => m.RelationshipId == rel.Id && !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => Map(m)).ToListAsync(ct);
    }

    public async Task<MessageResponse> GetMessageAsync(Guid userId, Guid messageId, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var m = await db.Messages.Include(x => x.Sender)
            .FirstOrDefaultAsync(x => x.Id == messageId && !x.IsDeleted, ct)
            ?? throw new NotFoundException("Message", messageId);
        if (m.RelationshipId != rel.Id) throw new UnauthorizedException();
        return Map(m);
    }

    public async Task<MessageResponse> SendMessageAsync(Guid userId, SendMessageRequest request, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var partner = await relAccess.GetPartnerAsync(userId)
            ?? throw new ConflictException("Your relationship does not have a second partner yet.");

        if (string.IsNullOrWhiteSpace(request.Text) && request.ImageKey == null && request.VoiceNoteKey == null)
            throw new ValidationException("A message must contain text, an image, or a voice note.");

        var msg = new Message
        {
            RelationshipId = rel.Id, SenderId = userId, RecipientId = partner.Id,
            Text = request.Text, ImageKey = request.ImageKey,
            VoiceNoteKey = request.VoiceNoteKey, Category = request.Category
        };
        db.Messages.Add(msg);
        await db.SaveChangesAsync(ct);
        msg.Sender = (await db.Users.FindAsync([userId], ct))!;
        return Map(msg);
    }

    public async Task<MessageResponse> UpdateMessageAsync(Guid userId, Guid messageId, UpdateMessageRequest request, CancellationToken ct = default)
    {
        var m = await db.Messages.Include(x => x.Sender)
            .FirstOrDefaultAsync(x => x.Id == messageId && !x.IsDeleted, ct)
            ?? throw new NotFoundException("Message", messageId);
        if (m.SenderId != userId) throw new UnauthorizedException("Only the sender can update a message.");
        if (request.Text != null) m.Text = request.Text;
        if (request.Category.HasValue) m.Category = request.Category.Value;
        await db.SaveChangesAsync(ct);
        return Map(m);
    }

    public async Task DeleteMessageAsync(Guid userId, Guid messageId, CancellationToken ct = default)
    {
        var m = await db.Messages.FirstOrDefaultAsync(x => x.Id == messageId && !x.IsDeleted, ct)
            ?? throw new NotFoundException("Message", messageId);
        if (m.SenderId != userId) throw new UnauthorizedException("Only the sender can delete a message.");
        m.IsDeleted = true;
        await db.SaveChangesAsync(ct);
    }

    public async Task<MessageResponse> MarkReadAsync(Guid userId, Guid messageId, CancellationToken ct = default)
    {
        var m = await db.Messages.Include(x => x.Sender)
            .FirstOrDefaultAsync(x => x.Id == messageId && !x.IsDeleted, ct)
            ?? throw new NotFoundException("Message", messageId);
        if (m.RecipientId != userId) throw new UnauthorizedException("Only the recipient can mark a message as read.");
        m.ReadAt ??= DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Map(m);
    }

    public async Task<MessageResponse> FavoriteAsync(Guid userId, Guid messageId, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var m = await db.Messages.Include(x => x.Sender)
            .FirstOrDefaultAsync(x => x.Id == messageId && !x.IsDeleted, ct)
            ?? throw new NotFoundException("Message", messageId);
        if (m.RelationshipId != rel.Id) throw new UnauthorizedException();
        m.IsFavorite = true;
        await db.SaveChangesAsync(ct);
        return Map(m);
    }

    public async Task<MessageResponse> UnfavoriteAsync(Guid userId, Guid messageId, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var m = await db.Messages.Include(x => x.Sender)
            .FirstOrDefaultAsync(x => x.Id == messageId && !x.IsDeleted, ct)
            ?? throw new NotFoundException("Message", messageId);
        if (m.RelationshipId != rel.Id) throw new UnauthorizedException();
        m.IsFavorite = false;
        await db.SaveChangesAsync(ct);
        return Map(m);
    }
}
