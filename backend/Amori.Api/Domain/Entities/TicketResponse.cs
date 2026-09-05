namespace Amori.Api.Domain.Entities;

public sealed class TicketResponse : BaseEntity
{
    public Guid TicketId { get; set; }
    public Guid RespondedByUserId { get; set; }
    public string Content { get; set; } = string.Empty;

    // Navigation
    public RelationshipTicket Ticket { get; set; } = null!;
    public User RespondedBy { get; set; } = null!;
}
