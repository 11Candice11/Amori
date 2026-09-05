using Amori.Api.Domain.Enums;

namespace Amori.Api.Domain.Entities;

public sealed class DateIdea : BaseEntity
{
    public Guid RelationshipId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateCategory Category { get; set; }
    public string? Location { get; set; }
    public decimal? EstimatedCost { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Notes { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation
    public Relationship Relationship { get; set; } = null!;
    public User CreatedBy { get; set; } = null!;
}
