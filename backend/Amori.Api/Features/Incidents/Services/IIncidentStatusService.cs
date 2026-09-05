using Amori.Api.Domain.Enums;

namespace Amori.Api.Features.Incidents.Services;

/// <summary>
/// Validates and enforces incident status transitions.
/// </summary>
public interface IIncidentStatusService
{
    /// <summary>
    /// Returns true if transitioning from <paramref name="current"/> to
    /// <paramref name="next"/> is a valid lifecycle step.
    /// </summary>
    bool IsValidTransition(IncidentStatus current, IncidentStatus next);

    /// <summary>
    /// Throws a ValidationException if the transition is invalid.
    /// </summary>
    void EnsureValidTransition(IncidentStatus current, IncidentStatus next);
}
