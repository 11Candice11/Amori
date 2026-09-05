namespace Amori.Api.Infrastructure.Authentication;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    bool IsAuthenticated { get; }
}
