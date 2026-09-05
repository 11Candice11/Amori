using Amori.Api.Domain.Enums;

namespace Amori.Api.Features.DatePlanner.DTOs;

public sealed class CreateDateIdeaRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateCategory Category { get; set; }
    public string? Location { get; set; }
    public decimal? EstimatedCost { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Notes { get; set; }
}

public sealed class UpdateDateIdeaRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateCategory? Category { get; set; }
    public string? Location { get; set; }
    public decimal? EstimatedCost { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Notes { get; set; }
    public bool? IsFavorite { get; set; }
}
