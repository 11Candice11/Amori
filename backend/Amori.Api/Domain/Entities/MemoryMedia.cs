using Amori.Api.Domain.Enums;

namespace Amori.Api.Domain.Entities;

public sealed class MemoryMedia : BaseEntity
{
    public Guid MemoryId { get; set; }
    public string FileKey { get; set; } = string.Empty;
    public MemoryMediaType MediaType { get; set; }
    public int? DurationSeconds { get; set; }

    // Navigation
    public Memory Memory { get; set; } = null!;
}
