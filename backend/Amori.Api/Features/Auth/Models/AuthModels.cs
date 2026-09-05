namespace Amori.Api.Features.Auth.Models;

/// <summary>
/// Internal model used by the auth service when validating credentials.
/// </summary>
public sealed class AuthValidationResult
{
    public bool IsValid { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];

    public static AuthValidationResult Success() => new() { IsValid = true };
    public static AuthValidationResult Failure(IReadOnlyList<string> errors) => new() { IsValid = false, Errors = errors };
}
