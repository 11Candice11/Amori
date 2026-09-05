namespace Amori.Api.Domain.Entities;

/// <summary>
/// A reusable guided question presented during a splitting session.
/// Questions are global (not relationship-specific) and shared across all sessions.
/// </summary>
public sealed class SplittingQuestion : BaseEntity
{
    public string Question { get; set; } = string.Empty;

    /// <summary>
    /// Free-form type label (e.g. "reflection", "grounding", "needs").
    /// Kept as string for flexibility; not a hard enum.
    /// </summary>
    public string? QuestionType { get; set; }

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<SplittingAnswer> Answers { get; set; } = [];
}
