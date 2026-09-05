using Amori.Api.Common.Exceptions;
using Amori.Api.Domain.Enums;

namespace Amori.Api.Features.Incidents.Services;

/// <summary>
/// Enforces the incident lifecycle state machine.
///
/// Valid transitions:
///   OPEN              → ASSIGNED, INVESTIGATING
///   ASSIGNED          → INVESTIGATING, AWAITING_RESPONSE
///   INVESTIGATING     → AWAITING_RESPONSE, RESOLVED
///   AWAITING_RESPONSE → INVESTIGATING, RESOLVED
///   RESOLVED          → CLOSED, REOPENED
///   CLOSED            → REOPENED
///   REOPENED          → INVESTIGATING
/// </summary>
public sealed class IncidentStatusService : IIncidentStatusService
{
    private static readonly Dictionary<IncidentStatus, HashSet<IncidentStatus>> ValidTransitions = new()
    {
        [IncidentStatus.Open]             = [IncidentStatus.Assigned, IncidentStatus.Investigating],
        [IncidentStatus.Assigned]         = [IncidentStatus.Investigating, IncidentStatus.AwaitingResponse],
        [IncidentStatus.Investigating]    = [IncidentStatus.AwaitingResponse, IncidentStatus.Resolved],
        [IncidentStatus.AwaitingResponse] = [IncidentStatus.Investigating, IncidentStatus.Resolved],
        [IncidentStatus.Resolved]         = [IncidentStatus.Closed, IncidentStatus.Reopened],
        [IncidentStatus.Closed]           = [IncidentStatus.Reopened],
        [IncidentStatus.Reopened]         = [IncidentStatus.Investigating],
    };

    public bool IsValidTransition(IncidentStatus current, IncidentStatus next)
    {
        return ValidTransitions.TryGetValue(current, out var allowed) && allowed.Contains(next);
    }

    public void EnsureValidTransition(IncidentStatus current, IncidentStatus next)
    {
        if (!IsValidTransition(current, next))
        {
            throw new ValidationException(
                $"Cannot transition from '{current}' to '{next}'. " +
                $"Allowed transitions from '{current}': {string.Join(", ", ValidTransitions.GetValueOrDefault(current, []))}.");
        }
    }
}
