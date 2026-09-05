namespace Amori.Api.Domain.Entities;

/// <summary>
/// An answer given by the user to a guided splitting question during a session.
/// </summary>
public sealed class SplittingAnswer : BaseEntity
{
    public Guid SessionId { get; set; }
    public Guid QuestionId { get; set; }
    public string Answer { get; set; } = string.Empty;

    // Navigation
    public SplittingSession Session { get; set; } = null!;
    public SplittingQuestion Question { get; set; } = null!;
}
