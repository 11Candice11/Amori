using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Domain.Enums;
using Amori.Api.Features.DatePlanner.Controllers;
using Amori.Api.Infrastructure.Relationships;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.DatePlanner.Services;

public sealed class DatePlannerService(
    AmoriDbContext db,
    IRelationshipAccessService relAccess) : IDatePlannerService
{
    private static DateIdeaResponse Map(DateIdea d) => new()
    {
        Id = d.Id, RelationshipId = d.RelationshipId,
        CreatedByUserId = d.CreatedByUserId, CreatedByName = d.CreatedBy?.DisplayName ?? string.Empty,
        Title = d.Title, Description = d.Description, Category = d.Category,
        Location = d.Location, EstimatedCost = d.EstimatedCost, DurationMinutes = d.DurationMinutes,
        Notes = d.Notes, IsFavorite = d.IsFavorite, IsCompleted = d.IsCompleted,
        CompletedAt = d.CompletedAt, CreatedAt = d.CreatedAt, UpdatedAt = d.UpdatedAt
    };

    private async Task<(Guid relId, DateIdea idea)> LoadAsync(Guid userId, Guid ideaId, CancellationToken ct)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var idea = await db.DateIdeas.Include(d => d.CreatedBy)
            .FirstOrDefaultAsync(d => d.Id == ideaId, ct)
            ?? throw new NotFoundException("Date idea", ideaId);
        if (idea.RelationshipId != rel.Id) throw new UnauthorizedException();
        return (rel.Id, idea);
    }

    public async Task<IReadOnlyList<DateIdeaResponse>> GetAllAsync(Guid userId, DateCategory? category, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var query = db.DateIdeas.Include(d => d.CreatedBy).Where(d => d.RelationshipId == rel.Id);
        if (category.HasValue) query = query.Where(d => d.Category == category.Value);
        return await query.OrderByDescending(d => d.CreatedAt).Select(d => Map(d)).ToListAsync(ct);
    }

    public async Task<DateIdeaResponse> GetByIdAsync(Guid userId, Guid dateId, CancellationToken ct = default)
    {
        var (_, idea) = await LoadAsync(userId, dateId, ct);
        return Map(idea);
    }

    public async Task<DateIdeaResponse> CreateAsync(Guid userId, CreateDateIdeaRequest request, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        if (string.IsNullOrWhiteSpace(request.Title)) throw new ValidationException("Title is required.");

        var idea = new DateIdea
        {
            RelationshipId = rel.Id, CreatedByUserId = userId,
            Title = request.Title.Trim(), Description = request.Description,
            Category = request.Category, Location = request.Location,
            EstimatedCost = request.EstimatedCost, DurationMinutes = request.DurationMinutes,
            Notes = request.Notes
        };
        db.DateIdeas.Add(idea);
        await db.SaveChangesAsync(ct);
        idea.CreatedBy = (await db.Users.FindAsync([userId], ct))!;
        return Map(idea);
    }

    public async Task<DateIdeaResponse> UpdateAsync(Guid userId, Guid dateId, UpdateDateIdeaRequest request, CancellationToken ct = default)
    {
        var (_, idea) = await LoadAsync(userId, dateId, ct);
        if (request.Title != null) idea.Title = request.Title.Trim();
        if (request.Description != null) idea.Description = request.Description;
        if (request.Category.HasValue) idea.Category = request.Category.Value;
        if (request.Location != null) idea.Location = request.Location;
        if (request.EstimatedCost.HasValue) idea.EstimatedCost = request.EstimatedCost;
        if (request.DurationMinutes.HasValue) idea.DurationMinutes = request.DurationMinutes;
        if (request.Notes != null) idea.Notes = request.Notes;
        if (request.IsFavorite.HasValue) idea.IsFavorite = request.IsFavorite.Value;
        await db.SaveChangesAsync(ct);
        return Map(idea);
    }

    public async Task DeleteAsync(Guid userId, Guid dateId, CancellationToken ct = default)
    {
        var (_, idea) = await LoadAsync(userId, dateId, ct);
        if (idea.CreatedByUserId != userId) throw new UnauthorizedException("Only the creator can delete this.");
        db.DateIdeas.Remove(idea);
        await db.SaveChangesAsync(ct);
    }

    public async Task<DateIdeaResponse> CompleteAsync(Guid userId, Guid dateId, CancellationToken ct = default)
    {
        var (_, idea) = await LoadAsync(userId, dateId, ct);
        idea.IsCompleted = true;
        idea.CompletedAt ??= DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Map(idea);
    }

    public async Task<DateIdeaResponse?> GetRandomAsync(Guid userId, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var ideas = await db.DateIdeas.Include(d => d.CreatedBy)
            .Where(d => d.RelationshipId == rel.Id && !d.IsCompleted)
            .ToListAsync(ct);
        if (ideas.Count == 0) return null;
        return Map(ideas[Random.Shared.Next(ideas.Count)]);
    }
}
