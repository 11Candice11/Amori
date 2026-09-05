using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Domain.Enums;
using Amori.Api.Features.Auth.DTOs;
using Amori.Api.Infrastructure.Authentication;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.Auth.Services;

public sealed class AuthService(
    AmoriDbContext db,
    IJwtTokenService jwtTokenService,
    IPasswordHasher passwordHasher,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        ValidateRegister(request);

        var email = request.Email.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(u => u.Email == email, ct))
            throw new ConflictException($"User with email '{request.Email}' already exists.");

        var user = new User
        {
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password),
            DisplayName = request.Name.Trim(),
            Status = UserStatus.Active
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("User registered: {UserId}", user.Id);

        return BuildResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        ValidateLogin(request);

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

        if (user == null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        if (user.Status != UserStatus.Active)
            throw new UnauthorizedException("User account is not active.");

        logger.LogInformation("User logged in: {UserId}", user.Id);

        return BuildResponse(user);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private AuthResponse BuildResponse(User user) => new()
    {
        AccessToken = jwtTokenService.GenerateAccessToken(user),
        User = new UserResponse { Id = user.Id, Name = user.DisplayName, Email = user.Email }
    };

    private static void ValidateRegister(RegisterRequest req)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(req.Name)) errors.Add("Name is required.");
        if (string.IsNullOrWhiteSpace(req.Email)) errors.Add("Email is required.");
        else if (!IsValidEmail(req.Email)) errors.Add("Email format is invalid.");
        if (string.IsNullOrWhiteSpace(req.Password)) errors.Add("Password is required.");
        else if (req.Password.Length < 8) errors.Add("Password must be at least 8 characters.");
        if (errors.Count > 0) throw new Common.Exceptions.ValidationException(errors);
    }

    private static void ValidateLogin(LoginRequest req)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(req.Email)) errors.Add("Email is required.");
        if (string.IsNullOrWhiteSpace(req.Password)) errors.Add("Password is required.");
        if (errors.Count > 0) throw new Common.Exceptions.ValidationException(errors);
    }

    private static bool IsValidEmail(string email)
    {
        try { var a = new System.Net.Mail.MailAddress(email); return a.Address == email; }
        catch { return false; }
    }
}
