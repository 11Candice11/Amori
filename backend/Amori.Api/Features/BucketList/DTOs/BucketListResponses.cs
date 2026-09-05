using Amori.Api.Domain.Enums;

namespace Amori.Api.Features.BucketList.DTOs;

public sealed class BucketListItemResponse
{
    public Guid Id { get; init; }
    public Guid RelationshipId { get; init; }
    public Guid AddedByUserId { get; init; }
    public string AddedByName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Location { get; init; }
    public BucketListCategory Category { get; init; }
    public DateOnly? TargetDate { get; init; }
    public string? Notes { get; init; }
    public bool IsFavorite { get; init; }
    public bool IsCompleted { get; init; }
    public DateTime? CompletedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
