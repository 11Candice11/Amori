using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Features.Users.DTOs;
using Amori.Api.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Amori.Api.Features.Users;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController(
    AmoriDbContext dbContext,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<GetCurrentUserResponse>> GetCurrentUser()
    {
        var userId = currentUserService.UserId;
        if (userId == null)
            throw new UnauthorizedException("User not authenticated.");

        var user = await dbContext.Users.FindAsync(userId);
        if (user == null)
            throw new NotFoundException("User not found.");

        return Ok(new GetCurrentUserResponse
        {
            Id = user.Id,
            Name = user.DisplayName,
            Email = user.Email
        });
    }
}
