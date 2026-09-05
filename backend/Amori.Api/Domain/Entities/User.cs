using Amori.Api.Domain.Enums;

namespace Amori.Api.Domain.Entities;

public sealed class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public DateTime? LastActiveAt { get; set; }

    // Navigation
    public ICollection<RelationshipMember> RelationshipMemberships { get; set; } = [];
}
