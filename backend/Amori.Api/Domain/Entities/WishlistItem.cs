using Amori.Api.Domain.Enums;

namespace Amori.Api.Domain.Entities;

public sealed class WishlistItem : BaseEntity
{
    public Guid RelationshipId { get; set; }
    public Guid AddedByUserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageKey { get; set; }
    public decimal? Price { get; set; }
    public string? Url { get; set; }
    public WishlistPriority Priority { get; set; } = WishlistPriority.Medium;
    public string? Notes { get; set; }
    public bool IsPurchased { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime? PurchasedAt { get; set; }

    // Navigation
    public Relationship Relationship { get; set; } = null!;
    public User AddedBy { get; set; } = null!;
}
