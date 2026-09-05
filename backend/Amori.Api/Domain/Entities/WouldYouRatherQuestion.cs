namespace Amori.Api.Domain.Entities;

public sealed class WouldYouRatherQuestion : BaseEntity
{
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<WouldYouRatherAnswer> Answers { get; set; } = [];
}
