using Amori.Api.Domain.Enums;

namespace Amori.Api.Domain.Entities;

public sealed class BucketListItem : BaseEntity
{
    public Guid RelationshipId { get; set; }
    public Guid AddedByUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public BucketListCategory Category { get; set; }
    public DateOnly? TargetDate { get; set; }
    public string? Notes { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation
    public Relationship Relationship { get; set; } = null!;
    public User AddedBy { get; set; } = null!;
}
