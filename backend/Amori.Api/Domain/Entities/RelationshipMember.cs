using Amori.Api.Domain.Enums;

namespace Amori.Api.Domain.Entities;

public sealed class RelationshipMember : BaseEntity
{
    public Guid RelationshipId { get; set; }
    public Guid UserId { get; set; }
    public RelationshipRole Role { get; set; } = RelationshipRole.Member;
    public MemberInviteStatus InviteStatus { get; set; } = MemberInviteStatus.Pending;

    // Navigation
    public Relationship Relationship { get; set; } = null!;
    public User User { get; set; } = null!;
}
