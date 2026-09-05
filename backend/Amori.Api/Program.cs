using Amori.Api.Common.Extensions;
using Amori.Api.Common.Middleware;
using Amori.Api.Configuration;
using Amori.Api.Data.Context;
using Amori.Api.Data.Seed;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Configuration ─────────────────────────────────────────────────────────────
builder.Services.Configure<AwsSettings>(
    builder.Configuration.GetSection(AwsSettings.SectionName));
builder.Services.Configure<NotificationSettings>(
    builder.Configuration.GetSection(NotificationSettings.SectionName));

// ── Core services ─────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDatabase(builder.Configuration);

// ── Authentication / JWT ──────────────────────────────────────────────────────
builder.Services.AddJwtAuthentication(builder.Configuration);

// ── Application services ──────────────────────────────────────────────────────
builder.Services.AddApplicationServices();

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddAmoriCors(builder.Configuration);

// ── Swagger ───────────────────────────────────────────────────────────────────
builder.Services.AddSwagger();

// ── Health Checks ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks();

// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithUi();
}

app.UseHttpsRedirection();
app.UseCors(Amori.Api.Configuration.CorsSettings.PolicyName);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/api/healthz");

// ── Auto-migrate and seed ─────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AmoriDbContext>();
    await db.Database.MigrateAsync();
    await AmoriDbSeeder.SeedAsync(db);
}

app.Run();

// Needed for integration test WebApplicationFactory
public partial class Program { }
