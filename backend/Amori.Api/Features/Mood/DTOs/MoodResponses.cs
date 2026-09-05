using Amori.Api.Domain.Enums;

namespace Amori.Api.Features.Mood.DTOs;

public sealed class CheckInResponse
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid RelationshipId { get; init; }
    public CheckInType CheckInType { get; init; }
    public MoodType Mood { get; init; }
    public int Intensity { get; init; }
    public string? WhatHappened { get; init; }
    public string? Feelings { get; init; }
    public string? PerceivedCause { get; init; }
    public string? WhatINeed { get; init; }
    public bool IsSharedWithPartner { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public sealed class MoodSummaryResponse
{
    public MoodType? CurrentMood { get; init; }
    public int? CurrentIntensity { get; init; }
    public DateTime? LastCheckInAt { get; init; }
    public IReadOnlyList<CheckInResponse> RecentCheckIns { get; init; } = [];
    public IDictionary<string, int> MoodFrequency { get; init; } = new Dictionary<string, int>();
}
