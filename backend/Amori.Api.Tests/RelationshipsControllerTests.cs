using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Amori.Api.Features.Auth.DTOs;
using Amori.Api.Features.Relationships.DTOs;
using Xunit;

namespace Amori.Api.Tests;

public sealed class RelationshipsControllerTests(AmoriWebApplicationFactory factory)
    : IClassFixture<AmoriWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public async Task InitializeAsync() => await factory.EnsureDatabaseCreatedAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateRelationship_ShouldReturn201WithRelationshipId()
    {
        var token = await RegisterAndGetTokenAsync($"rel-a-{Guid.NewGuid()}@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsync("/api/relationships",
            Json(new CreateRelationshipRequest { StartDate = new DateOnly(2024, 6, 1) }));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.NotEqual(Guid.Empty, doc.RootElement.GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task GetMyRelationship_WithNoRelationship_ShouldReturn404()
    {
        var token = await RegisterAndGetTokenAsync($"no-rel-{Guid.NewGuid()}@example.com");
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/relationships/me");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task JoinRelationship_ShouldHaveTwoMembers()
    {
        var token1 = await RegisterAndGetTokenAsync($"partner1-{Guid.NewGuid()}@example.com");
        var token2 = await RegisterAndGetTokenAsync($"partner2-{Guid.NewGuid()}@example.com");

        // User 1 creates a relationship
        using var client1 = factory.CreateClient();
        client1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var createResp = await client1.PostAsync("/api/relationships",
            Json(new CreateRelationshipRequest { StartDate = new DateOnly(2023, 1, 1) }));

        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

        var createJson = await createResp.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(createJson);
        var relationshipId = createDoc.RootElement.GetProperty("id").GetGuid();

        // User 2 joins
        using var client2 = factory.CreateClient();
        client2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        var joinResp = await client2.PostAsync($"/api/relationships/{relationshipId}/join", null);

        Assert.Equal(HttpStatusCode.OK, joinResp.StatusCode);

        var joinJson = await joinResp.Content.ReadAsStringAsync();
        using var joinDoc = JsonDocument.Parse(joinJson);
        Assert.Equal(2, joinDoc.RootElement.GetProperty("members").GetArrayLength());
    }

    // ── helpers ──────────────────────────────────────────────────────────────
    private async Task<string> RegisterAndGetTokenAsync(string email)
    {
        using var client = factory.CreateClient();
        var resp = await client.PostAsync("/api/auth/register",
            Json(new RegisterRequest { Name = "Test", Email = email, Password = "TestPass123!" }));

        var body = JsonSerializer.Deserialize<AuthResponse>(
            await resp.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return body!.AccessToken;
    }

    private static StringContent Json<T>(T value) =>
        new(JsonSerializer.Serialize(value), System.Text.Encoding.UTF8, "application/json");
}
