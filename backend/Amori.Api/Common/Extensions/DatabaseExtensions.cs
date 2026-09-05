using Amori.Api.Configuration;
using Amori.Api.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Common.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<DatabaseSettings>(
            configuration.GetSection(DatabaseSettings.SectionName));

        var connectionString = configuration
            .GetSection(DatabaseSettings.SectionName)
            .GetValue<string>(nameof(DatabaseSettings.ConnectionString));

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Database connection string is not configured. " +
                "Set 'Database:ConnectionString' in appsettings or environment variables.");
        }

        services.AddDbContext<AmoriDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(AmoriDbContext).Assembly.FullName);
                npgsql.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            });
        });

        return services;
    }
}
