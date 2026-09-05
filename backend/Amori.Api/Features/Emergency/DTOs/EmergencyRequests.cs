using Amori.Api.Domain.Enums;

namespace Amori.Api.Features.Emergency.DTOs;

public sealed class CreateEmergencyRequestDto
{
    public EmergencyRequestType Type { get; set; }
    public string? Message { get; set; }
}

public sealed class UpdateEmergencyRequestDto
{
    public string? Message { get; set; }
}
