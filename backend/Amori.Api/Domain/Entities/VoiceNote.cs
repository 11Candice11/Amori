using Amori.Api.Domain.Enums;

namespace Amori.Api.Domain.Entities;

public sealed class VoiceNote : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid RelationshipId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FileKey { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public VoiceNoteCategory Category { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Relationship Relationship { get; set; } = null!;
}
