using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Features.Users.DTOs;

namespace Amori.Api.Features.Users.Services;

public sealed class UserService(AmoriDbContext db) : IUserService
{
    public async Task<GetCurrentUserResponse> GetCurrentUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await db.Users.FindAsync([userId], ct)
            ?? throw new NotFoundException("User", userId);

        return new GetCurrentUserResponse
        {
            Id = user.Id,
            Name = user.DisplayName,
            Email = user.Email
        };
    }
}
