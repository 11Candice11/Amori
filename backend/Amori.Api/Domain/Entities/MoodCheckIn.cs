using Amori.Api.Domain.Enums;

namespace Amori.Api.Domain.Entities;

public sealed class MoodCheckIn : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid RelationshipId { get; set; }
    public CheckInType CheckInType { get; set; } = CheckInType.AdHoc;
    public MoodType Mood { get; set; }
    public int Intensity { get; set; } // 1-10
    public string? WhatHappened { get; set; }
    public string? Feelings { get; set; }
    public string? PerceivedCause { get; set; }
    public string? WhatINeed { get; set; }
    public bool IsSharedWithPartner { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Relationship Relationship { get; set; } = null!;
}
