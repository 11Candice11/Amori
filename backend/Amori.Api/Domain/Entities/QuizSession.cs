namespace Amori.Api.Domain.Entities;

public sealed class QuizSession : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid RelationshipId { get; set; }
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Relationship Relationship { get; set; } = null!;
    public ICollection<QuizAnswer> Answers { get; set; } = [];
}
