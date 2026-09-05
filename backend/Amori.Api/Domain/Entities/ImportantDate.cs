namespace Amori.Api.Domain.Entities;

public sealed class ImportantDate : BaseEntity
{
    public Guid RelationshipId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public bool IsRecurring { get; set; }
    public bool ReminderEnabled { get; set; }
    public int? ReminderDaysBefore { get; set; }
    public string? Notes { get; set; }
    public string? ImageKey { get; set; }

    // Navigation
    public Relationship Relationship { get; set; } = null!;
    public User CreatedBy { get; set; } = null!;
}
