namespace Amori.Api.Domain.Entities;

public sealed class Surprise : BaseEntity
{
    public Guid RelationshipId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public Guid RecipientUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? MessageText { get; set; }
    public string? ImageKey { get; set; }
    public string? VoiceNoteKey { get; set; }
    public DateOnly? ScheduledDate { get; set; }
    public DateTime? OpenedAt { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation
    public Relationship Relationship { get; set; } = null!;
    public User CreatedBy { get; set; } = null!;
    public User Recipient { get; set; } = null!;
}
