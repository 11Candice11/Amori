using Amori.Api.Domain.Enums;

namespace Amori.Api.Features.Incidents.Services;

/// <summary>
/// Calculates incident priority from impact and urgency using a priority matrix.
/// </summary>
public interface IIncidentPriorityService
{
    IncidentPriority Calculate(IncidentImpact impact, IncidentUrgency urgency);
}
