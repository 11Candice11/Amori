using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Amori.Api.Features.Auth.DTOs;
using Xunit;

namespace Amori.Api.Tests;

public sealed class UsersControllerTests(AmoriWebApplicationFactory factory)
    : IClassFixture<AmoriWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public async Task InitializeAsync() => await factory.EnsureDatabaseCreatedAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetMe_WithAuthentication_ShouldReturnCurrentUser()
    {
        var email = $"me-{Guid.NewGuid()}@example.com";
        var token = await RegisterAndGetTokenAsync(email, "MyPass123!");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.Equal(email, doc.RootElement.GetProperty("email").GetString());
    }

    [Fact]
    public async Task GetMe_WithoutAuthentication_ShouldReturn401()
    {
        // Ensure no auth header on a fresh client from the factory
        using var anonClient = factory.CreateClient();
        var response = await anonClient.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── helpers ──────────────────────────────────────────────────────────────
    private async Task<string> RegisterAndGetTokenAsync(string email, string password)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(new RegisterRequest { Name = "Test", Email = email, Password = password }),
            System.Text.Encoding.UTF8, "application/json");

        var resp = await _client.PostAsync("/api/auth/register", content);
        var body = JsonSerializer.Deserialize<AuthResponse>(
            await resp.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return body!.AccessToken;
    }
}
