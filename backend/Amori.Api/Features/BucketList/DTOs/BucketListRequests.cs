using Amori.Api.Domain.Enums;

namespace Amori.Api.Features.BucketList.DTOs;

public sealed class CreateBucketListItemRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public BucketListCategory Category { get; set; }
    public DateOnly? TargetDate { get; set; }
    public string? Notes { get; set; }
}

public sealed class UpdateBucketListItemRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public BucketListCategory? Category { get; set; }
    public DateOnly? TargetDate { get; set; }
    public string? Notes { get; set; }
}
