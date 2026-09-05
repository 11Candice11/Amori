using Amori.Api.Features.Users.DTOs;

namespace Amori.Api.Features.Users.Services;

/// <summary>
/// Business logic for user profile operations.
/// </summary>
public interface IUserService
{
    Task<GetCurrentUserResponse> GetCurrentUserAsync(Guid userId, CancellationToken ct = default);
}
