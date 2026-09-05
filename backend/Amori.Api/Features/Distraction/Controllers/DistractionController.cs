using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Domain.Enums;
using Amori.Api.Infrastructure.Authentication;
using Amori.Api.Infrastructure.Relationships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.Distraction.Controllers;

public sealed class GameResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public GameType Type { get; init; }
    public bool IsActive { get; init; }
}

public sealed class GameSessionResponse
{
    public Guid Id { get; init; }
    public Guid GameId { get; init; }
    public string GameTitle { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public Guid RelationshipId { get; init; }
    public GameSessionStatus Status { get; init; }
    public int? Score { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}

public sealed class CompleteGameSessionRequest
{
    public int? Score { get; set; }
}

/// <summary>
/// Distraction Centre — browse games, start sessions, record scores.
/// Game state for 2048 etc. is managed client-side; this stores sessions and scores only.
/// </summary>
[ApiController]
[Route("api/games")]
[Authorize]
public sealed class DistractionController(
    AmoriDbContext db,
    ICurrentUserService currentUser,
    IRelationshipAccessService relAccess) : ControllerBase
{
    private Guid RequireUserId() =>
        currentUser.UserId ?? throw new UnauthorizedException();

    private static GameResponse MapGame(Game g) => new()
    {
        Id = g.Id, Title = g.Title, Description = g.Description, Type = g.Type, IsActive = g.IsActive
    };

    private static GameSessionResponse MapSession(GameSession s) => new()
    {
        Id = s.Id, GameId = s.GameId, GameTitle = s.Game?.Title ?? string.Empty,
        UserId = s.UserId, RelationshipId = s.RelationshipId,
        Status = s.Status, Score = s.Score, CreatedAt = s.CreatedAt, CompletedAt = s.CompletedAt
    };

    /// <summary>List all available games.</summary>
    [HttpGet]
    public async Task<IActionResult> GetGames(CancellationToken ct)
    {
        var games = await db.Games.Where(g => g.IsActive).OrderBy(g => g.Title).ToListAsync(ct);
        return Ok(games.Select(MapGame));
    }

    /// <summary>Get a game by ID.</summary>
    [HttpGet("{gameId:guid}")]
    public async Task<IActionResult> GetGame(Guid gameId, CancellationToken ct)
    {
        var game = await db.Games.FindAsync([gameId], ct)
            ?? throw new NotFoundException("Game", gameId);
        return Ok(MapGame(game));
    }

    /// <summary>Start a game session.</summary>
    [HttpPost("{gameId:guid}/sessions")]
    public async Task<IActionResult> StartSession(Guid gameId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var game = await db.Games.FindAsync([gameId], ct)
            ?? throw new NotFoundException("Game", gameId);
        if (!game.IsActive) throw new ConflictException("This game is not currently available.");

        var session = new GameSession
        {
            GameId = game.Id, UserId = userId, RelationshipId = rel.Id,
            Status = GameSessionStatus.InProgress
        };
        db.GameSessions.Add(session);
        await db.SaveChangesAsync(ct);
        session.Game = game;
        return CreatedAtAction(nameof(GetGame), new { gameId }, MapSession(session));
    }

    /// <summary>Complete a game session and record the score.</summary>
    [HttpPost("sessions/{sessionId:guid}/complete")]
    public async Task<IActionResult> CompleteSession(Guid sessionId, [FromBody] CompleteGameSessionRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var session = await db.GameSessions.Include(s => s.Game)
            .FirstOrDefaultAsync(s => s.Id == sessionId, ct)
            ?? throw new NotFoundException("Game session", sessionId);
        if (session.UserId != userId) throw new UnauthorizedException();
        if (session.Status != GameSessionStatus.InProgress)
            throw new ConflictException("Session is not in progress.");

        session.Status = GameSessionStatus.Completed;
        session.Score = req.Score;
        session.CompletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Ok(MapSession(session));
    }

    /// <summary>Get game session history for the authenticated user's relationship.</summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(CancellationToken ct)
    {
        var userId = RequireUserId();
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");

        var sessions = await db.GameSessions.Include(s => s.Game)
            .Where(s => s.RelationshipId == rel.Id)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);

        return Ok(sessions.Select(MapSession));
    }
}
