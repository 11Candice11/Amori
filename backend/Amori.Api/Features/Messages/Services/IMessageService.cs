using Amori.Api.Features.Messages;

namespace Amori.Api.Features.Messages.Services;

/// <summary>
/// Business logic for partner messaging.
/// </summary>
public interface IMessageService
{
    Task<IReadOnlyList<MessageResponse>> GetMessagesAsync(Guid userId, CancellationToken ct = default);
    Task<MessageResponse> GetMessageAsync(Guid userId, Guid messageId, CancellationToken ct = default);
    Task<MessageResponse> SendMessageAsync(Guid userId, SendMessageRequest request, CancellationToken ct = default);
    Task<MessageResponse> UpdateMessageAsync(Guid userId, Guid messageId, UpdateMessageRequest request, CancellationToken ct = default);
    Task DeleteMessageAsync(Guid userId, Guid messageId, CancellationToken ct = default);
    Task<MessageResponse> MarkReadAsync(Guid userId, Guid messageId, CancellationToken ct = default);
    Task<MessageResponse> FavoriteAsync(Guid userId, Guid messageId, CancellationToken ct = default);
    Task<MessageResponse> UnfavoriteAsync(Guid userId, Guid messageId, CancellationToken ct = default);
}
