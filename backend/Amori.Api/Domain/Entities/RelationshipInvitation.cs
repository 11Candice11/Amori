using Amori.Api.Domain.Enums;

namespace Amori.Api.Domain.Entities;

public sealed class RelationshipInvitation : BaseEntity
{
    public Guid RelationshipId { get; set; }
    public Guid InvitedByUserId { get; set; }
    public string? InviteeEmail { get; set; }
    public string InviteCode { get; set; } = string.Empty;
    public MemberInviteStatus Status { get; set; } = MemberInviteStatus.Pending;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RespondedAt { get; set; }

    // Navigation
    public Relationship Relationship { get; set; } = null!;
    public User InvitedBy { get; set; } = null!;
}
