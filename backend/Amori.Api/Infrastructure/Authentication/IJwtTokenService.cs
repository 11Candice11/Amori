using Amori.Api.Domain.Entities;

namespace Amori.Api.Infrastructure.Authentication;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Guid? GetUserIdFromToken(string token);
}
