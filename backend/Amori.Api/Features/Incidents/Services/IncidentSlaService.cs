using Amori.Api.Domain.Enums;

namespace Amori.Api.Features.Incidents.Services;

/// <summary>
/// SLA targets:
///   LOW      = 7 days
///   MEDIUM   = 3 days
///   HIGH     = 24 hours
///   CRITICAL = 4 hours
/// </summary>
public sealed class IncidentSlaService : IIncidentSlaService
{
    public DateTime CalculateDueAt(IncidentPriority priority, DateTime createdAt)
    {
        var offset = priority switch
        {
            IncidentPriority.Critical => TimeSpan.FromHours(4),
            IncidentPriority.High     => TimeSpan.FromHours(24),
            IncidentPriority.Medium   => TimeSpan.FromDays(3),
            IncidentPriority.Low      => TimeSpan.FromDays(7),
            _                         => TimeSpan.FromDays(7)
        };

        return DateTime.SpecifyKind(createdAt.Add(offset), DateTimeKind.Utc);
    }

    public bool IsOverdue(DateTime dueAt, IncidentStatus status)
    {
        // Resolved/closed incidents cannot be overdue
        if (status is IncidentStatus.Resolved or IncidentStatus.Closed)
            return false;

        return DateTime.UtcNow > dueAt;
    }

    public double RemainingSeconds(DateTime dueAt)
    {
        var remaining = dueAt - DateTime.UtcNow;
        return remaining.TotalSeconds > 0 ? remaining.TotalSeconds : 0;
    }
}
