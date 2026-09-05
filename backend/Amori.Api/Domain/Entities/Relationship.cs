using Amori.Api.Domain.Enums;

namespace Amori.Api.Domain.Entities;

public sealed class Relationship : BaseEntity
{
    public string? NickName { get; set; }
    public RelationshipStatus Status { get; set; } = RelationshipStatus.Active;
    public DateOnly? AnniversaryDate { get; set; }

    // Navigation
    public ICollection<RelationshipMember> Members { get; set; } = [];
}
