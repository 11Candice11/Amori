using Amori.Api.Domain.Enums;

namespace Amori.Api.Features.Emergency.Models;

/// <summary>
/// Internal model for tracking active emergency request state.
/// </summary>
public sealed class EmergencyRequestContext
{
    public Guid RelationshipId { get; init; }
    public Guid SenderId { get; init; }
    public Guid RecipientId { get; init; }
    public EmergencyRequestStatus Status { get; init; }
}
