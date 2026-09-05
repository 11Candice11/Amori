namespace Amori.Api.Domain.Entities;

public sealed class QuizQuestion : BaseEntity
{
    public Guid? RelationshipId { get; set; } // null = global question
    public string QuestionText { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public List<string> Options { get; set; } = [];
    public bool IsActive { get; set; } = true;

    // Navigation
    public Relationship? Relationship { get; set; }
}
