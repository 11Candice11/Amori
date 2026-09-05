using Amori.Api.Domain.Enums;

namespace Amori.Api.Domain.Entities;

public sealed class Message : BaseEntity
{
    public Guid RelationshipId { get; set; }
    public Guid SenderId { get; set; }
    public Guid RecipientId { get; set; }
    public string? Text { get; set; }
    public string? ImageKey { get; set; }
    public string? VoiceNoteKey { get; set; }
    public MessageCategory Category { get; set; } = MessageCategory.General;
    public DateTime? ReadAt { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation
    public Relationship Relationship { get; set; } = null!;
    public User Sender { get; set; } = null!;
    public User Recipient { get; set; } = null!;
}
