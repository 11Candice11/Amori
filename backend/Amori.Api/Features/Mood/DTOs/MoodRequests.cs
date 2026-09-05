using Amori.Api.Domain.Enums;

namespace Amori.Api.Features.Mood.DTOs;

public sealed class CreateCheckInRequest
{
    public CheckInType CheckInType { get; set; } = CheckInType.AdHoc;
    public MoodType Mood { get; set; }
    public int Intensity { get; set; } = 5;
    public string? WhatHappened { get; set; }
    public string? Feelings { get; set; }
    public string? PerceivedCause { get; set; }
    public string? WhatINeed { get; set; }
    public bool IsSharedWithPartner { get; set; }
}

public sealed class UpdateCheckInRequest
{
    public MoodType? Mood { get; set; }
    public int? Intensity { get; set; }
    public string? WhatHappened { get; set; }
    public string? Feelings { get; set; }
    public string? PerceivedCause { get; set; }
    public string? WhatINeed { get; set; }
    public bool? IsSharedWithPartner { get; set; }
}
