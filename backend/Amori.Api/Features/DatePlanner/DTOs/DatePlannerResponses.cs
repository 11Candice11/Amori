using Amori.Api.Domain.Enums;

namespace Amori.Api.Features.DatePlanner.DTOs;

public sealed class DateIdeaResponse
{
    public Guid Id { get; init; }
    public Guid RelationshipId { get; init; }
    public Guid CreatedByUserId { get; init; }
    public string CreatedByName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateCategory Category { get; init; }
    public string? Location { get; init; }
    public decimal? EstimatedCost { get; init; }
    public int? DurationMinutes { get; init; }
    public string? Notes { get; init; }
    public bool IsFavorite { get; init; }
    public bool IsCompleted { get; init; }
    public DateTime? CompletedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
