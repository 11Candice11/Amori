using Amori.Api.Data.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace Amori.Api.Tests;

/// <summary>
/// Shared fixture that starts one Postgres container per test class
/// and wires the DbContext to point at it.
/// Implements IAsyncLifetime so xUnit calls InitializeAsync before
/// any test constructor runs.
/// </summary>
public sealed class AmoriWebApplicationFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    // Called by xUnit before any test in the class that uses this fixture.
    public async Task InitializeAsync() => await _db.StartAsync();

    public new async Task DisposeAsync()
    {
        await _db.StopAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real DbContext registration coming from Program.cs.
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AmoriDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Also remove the DbContext itself so EF doesn't complain about
            // a duplicate registration in some SDK versions.
            var ctxDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(AmoriDbContext));
            if (ctxDescriptor != null)
                services.Remove(ctxDescriptor);

            // Register against the test container – _db is already started
            // because InitializeAsync ran before ConfigureWebHost.
            services.AddDbContext<AmoriDbContext>(options =>
                options.UseNpgsql(_db.GetConnectionString()));
        });

        builder.UseEnvironment("Testing");
    }

    /// <summary>
    /// Applies EF migrations (or EnsureCreated) so the schema is ready.
    /// Call this once per fixture before the first request.
    /// </summary>
    public async Task EnsureDatabaseCreatedAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AmoriDbContext>();
        await db.Database.EnsureCreatedAsync();
    }
}
