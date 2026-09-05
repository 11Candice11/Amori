using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Amori.Api.Infrastructure.Authentication;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid? UserId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;

            // JwtSecurityTokenHandler maps "sub" → ClaimTypes.NameIdentifier by default.
            // Fall back to the short-form "sub" in case MapInboundClaims is disabled.
            var value = user?.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? user?.FindFirstValue(JwtRegisteredClaimNames.Sub);

            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;
}
