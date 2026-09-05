using Amori.Api.Features.Incidents.Services;
using Amori.Api.Infrastructure.Relationships;

namespace Amori.Api.Common.Extensions;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Relationship
        services.AddScoped<IRelationshipAccessService, RelationshipAccessService>();

        // Incident Management
        services.AddScoped<IIncidentService, IncidentService>();
        services.AddSingleton<IIncidentPriorityService, IncidentPriorityService>();
        services.AddSingleton<IIncidentStatusService, IncidentStatusService>();
        services.AddSingleton<IIncidentSlaService, IncidentSlaService>();

        return services;
    }
}
