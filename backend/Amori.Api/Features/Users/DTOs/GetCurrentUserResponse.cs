namespace Amori.Api.Features.Users.DTOs;

public sealed class GetCurrentUserResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
