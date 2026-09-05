namespace Amori.Api.Domain.Entities;

public sealed class QuizAnswer : BaseEntity
{
    public Guid QuizSessionId { get; set; }
    public Guid QuestionId { get; set; }
    public string AnswerGiven { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }

    // Navigation
    public QuizSession QuizSession { get; set; } = null!;
    public QuizQuestion Question { get; set; } = null!;
}
