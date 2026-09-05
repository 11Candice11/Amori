using Amori.Api.Features.Auth.DTOs;

namespace Amori.Api.Features.Auth.Services;

/// <summary>
/// Core authentication business logic — register and login.
/// </summary>
public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
