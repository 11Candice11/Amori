namespace Amori.Api.Features.Users.Models;

/// <summary>
/// Internal model representing a resolved user identity used within service logic.
/// </summary>
public sealed class UserIdentity
{
    public Guid Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
