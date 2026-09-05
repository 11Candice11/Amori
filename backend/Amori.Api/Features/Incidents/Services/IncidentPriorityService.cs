using Amori.Api.Domain.Enums;

namespace Amori.Api.Features.Incidents.Services;

/// <summary>
/// Calculates incident priority from impact + urgency using a 4×4 matrix.
///
/// Matrix (Impact × Urgency → Priority):
///
///              Urgency
///              Low     Med     High    Critical
/// Impact Low   LOW     LOW     MED     HIGH
///        Med   LOW     MED     HIGH    CRIT
///        High  MED     HIGH    HIGH    CRIT
///        Crit  HIGH    HIGH    CRIT    CRIT
/// </summary>
public sealed class IncidentPriorityService : IIncidentPriorityService
{
    private static readonly IncidentPriority[,] Matrix =
    {
        // Urgency:   Low                   Med                   High                  Critical
        /* Low    */ { IncidentPriority.Low, IncidentPriority.Low,    IncidentPriority.Medium,   IncidentPriority.High     },
        /* Med    */ { IncidentPriority.Low, IncidentPriority.Medium, IncidentPriority.High,     IncidentPriority.Critical },
        /* High   */ { IncidentPriority.Medium, IncidentPriority.High, IncidentPriority.High,   IncidentPriority.Critical },
        /* Crit   */ { IncidentPriority.High, IncidentPriority.High, IncidentPriority.Critical, IncidentPriority.Critical },
    };

    public IncidentPriority Calculate(IncidentImpact impact, IncidentUrgency urgency)
    {
        var impactIndex = (int)impact - 1; // enum is 1-based
        var urgencyIndex = (int)urgency - 1;
        return Matrix[impactIndex, urgencyIndex];
    }
}
