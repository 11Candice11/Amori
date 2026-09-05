namespace Amori.Api.Domain.Entities;

public sealed class WouldYouRatherAnswer : BaseEntity
{
    public Guid QuestionId { get; set; }
    public Guid UserId { get; set; }
    public Guid RelationshipId { get; set; }
    public bool ChoseOptionA { get; set; }

    // Navigation
    public WouldYouRatherQuestion Question { get; set; } = null!;
    public User User { get; set; } = null!;
    public Relationship Relationship { get; set; } = null!;
}
