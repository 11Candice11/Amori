using System.Net;
using System.Text.Json;
using Amori.Api.Features.Auth.DTOs;
using Xunit;

namespace Amori.Api.Tests;

public sealed class AuthControllerTests(AmoriWebApplicationFactory factory)
    : IClassFixture<AmoriWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public async Task InitializeAsync() => await factory.EnsureDatabaseCreatedAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Register_WithValidData_ShouldSucceed()
    {
        var request = new RegisterRequest
        {
            Name = "Alice Smith",
            Email = $"alice-{Guid.NewGuid()}@example.com",
            Password = "SecurePass123!"
        };

        var response = await _client.PostAsync("/api/auth/register", Json(request));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = Deserialize<AuthResponse>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.AccessToken));
        Assert.NotNull(body.User);
        Assert.Equal(request.Email, body.User.Email);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnConflict()
    {
        var email = $"duplicate-{Guid.NewGuid()}@example.com";
        var request = new RegisterRequest { Name = "Bob", Email = email, Password = "Pass123!" };

        await _client.PostAsync("/api/auth/register", Json(request));
        var second = await _client.PostAsync("/api/auth/register", Json(request));

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldSucceed()
    {
        var email = $"carol-{Guid.NewGuid()}@example.com";
        const string password = "TestPass123!";

        await _client.PostAsync("/api/auth/register",
            Json(new RegisterRequest { Name = "Carol", Email = email, Password = password }));

        var loginResp = await _client.PostAsync("/api/auth/login",
            Json(new LoginRequest { Email = email, Password = password }));

        Assert.Equal(HttpStatusCode.OK, loginResp.StatusCode);

        var body = Deserialize<AuthResponse>(await loginResp.Content.ReadAsStringAsync());
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.AccessToken));
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldReturnForbidden()
    {
        var email = $"dave-{Guid.NewGuid()}@example.com";

        await _client.PostAsync("/api/auth/register",
            Json(new RegisterRequest { Name = "Dave", Email = email, Password = "Correct123!" }));

        var loginResp = await _client.PostAsync("/api/auth/login",
            Json(new LoginRequest { Email = email, Password = "WrongPassword!" }));

        Assert.Equal(HttpStatusCode.Forbidden, loginResp.StatusCode);
    }

    // ── helpers ──────────────────────────────────────────────────────────────
    private static StringContent Json<T>(T value) =>
        new(JsonSerializer.Serialize(value), System.Text.Encoding.UTF8, "application/json");

    private static T? Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
}
