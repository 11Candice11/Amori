using Amori.Api.Domain.Enums;

namespace Amori.Api.Features.Incidents.Services;

/// <summary>
/// Calculates SLA due dates and overdue state for incidents.
/// </summary>
public interface IIncidentSlaService
{
    DateTime CalculateDueAt(IncidentPriority priority, DateTime createdAt);
    bool IsOverdue(DateTime dueAt, IncidentStatus status);
    double RemainingSeconds(DateTime dueAt);
}
