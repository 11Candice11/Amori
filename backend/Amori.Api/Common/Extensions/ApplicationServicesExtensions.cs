using Amori.Api.Infrastructure.Relationships;

namespace Amori.Api.Common.Extensions;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IRelationshipAccessService, RelationshipAccessService>();

        return services;
    }
}
