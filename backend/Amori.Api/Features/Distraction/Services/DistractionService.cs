using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Domain.Enums;
using Amori.Api.Features.Distraction.Controllers;
using Amori.Api.Infrastructure.Relationships;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.Distraction.Services;

public sealed class DistractionService(
    AmoriDbContext db,
    IRelationshipAccessService relAccess) : IDistractionService
{
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

    public async Task<IReadOnlyList<GameResponse>> GetGamesAsync(CancellationToken ct = default) =>
        await db.Games.Where(g => g.IsActive).OrderBy(g => g.Title)
            .Select(g => MapGame(g)).ToListAsync(ct);

    public async Task<GameResponse> GetGameAsync(Guid gameId, CancellationToken ct = default)
    {
        var game = await db.Games.FindAsync([gameId], ct)
            ?? throw new NotFoundException("Game", gameId);
        return MapGame(game);
    }

    public async Task<GameSessionResponse> StartSessionAsync(Guid userId, Guid gameId, CancellationToken ct = default)
    {
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
        return MapSession(session);
    }

    public async Task<GameSessionResponse> CompleteSessionAsync(Guid userId, Guid sessionId, CompleteGameSessionRequest request, CancellationToken ct = default)
    {
        var session = await db.GameSessions.Include(s => s.Game)
            .FirstOrDefaultAsync(s => s.Id == sessionId, ct)
            ?? throw new NotFoundException("Game session", sessionId);
        if (session.UserId != userId) throw new UnauthorizedException();
        if (session.Status != GameSessionStatus.InProgress)
            throw new ConflictException("Session is not in progress.");

        session.Status = GameSessionStatus.Completed;
        session.Score = request.Score;
        session.CompletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return MapSession(session);
    }

    public async Task<IReadOnlyList<GameSessionResponse>> GetHistoryAsync(Guid userId, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        return await db.GameSessions.Include(s => s.Game)
            .Where(s => s.RelationshipId == rel.Id)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => MapSession(s)).ToListAsync(ct);
    }
}
