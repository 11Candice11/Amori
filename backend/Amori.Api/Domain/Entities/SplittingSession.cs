using Amori.Api.Domain.Enums;

namespace Amori.Api.Domain.Entities;

public sealed class SplittingSession : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid RelationshipId { get; set; }
    public List<string> FeelingsSelected { get; set; } = [];
    public string? Trigger { get; set; }
    public string? Description { get; set; }
    public string? WhatINeed { get; set; }
    public SplittingAction? RecommendedSupportType { get; set; }
    public List<SplittingAction> ActionsTaken { get; set; } = [];
    public MoodType? InitialMood { get; set; }
    public MoodType? FinalMood { get; set; }
    public SplittingSessionStatus Status { get; set; } = SplittingSessionStatus.InProgress;
    public DateTime? CompletedAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Relationship Relationship { get; set; } = null!;
}
