using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Domain.Enums;
using Amori.Api.Infrastructure.Authentication;
using Amori.Api.Infrastructure.Relationships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.DatePlanner.Controllers;

public sealed class CreateDateIdeaRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateCategory Category { get; set; }
    public string? Location { get; set; }
    public decimal? EstimatedCost { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Notes { get; set; }
}

public sealed class UpdateDateIdeaRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateCategory? Category { get; set; }
    public string? Location { get; set; }
    public decimal? EstimatedCost { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Notes { get; set; }
    public bool? IsFavorite { get; set; }
}

public sealed class DateIdeaResponse
{
    public Guid Id { get; init; }
    public Guid RelationshipId { get; init; }
    public Guid CreatedByUserId { get; init; }
    public string CreatedByName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateCategory Category { get; init; }
    public string? Location { get; init; }
    public decimal? EstimatedCost { get; init; }
    public int? DurationMinutes { get; init; }
    public string? Notes { get; init; }
    public bool IsFavorite { get; init; }
    public bool IsCompleted { get; init; }
    public DateTime? CompletedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>Date ideas and planning for the relationship.</summary>
[ApiController]
[Route("api/dates")]
[Authorize]
public sealed class DatePlannerController(
    AmoriDbContext db,
    ICurrentUserService currentUser,
    IRelationshipAccessService relAccess) : ControllerBase
{
    private Guid RequireUserId() =>
        currentUser.UserId ?? throw new UnauthorizedException();

    private static DateIdeaResponse Map(DateIdea d) => new()
    {
        Id = d.Id, RelationshipId = d.RelationshipId,
        CreatedByUserId = d.CreatedByUserId, CreatedByName = d.CreatedBy?.DisplayName ?? string.Empty,
        Title = d.Title, Description = d.Description, Category = d.Category,
        Location = d.Location, EstimatedCost = d.EstimatedCost, DurationMinutes = d.DurationMinutes,
        Notes = d.Notes, IsFavorite = d.IsFavorite, IsCompleted = d.IsCompleted,
        CompletedAt = d.CompletedAt, CreatedAt = d.CreatedAt, UpdatedAt = d.UpdatedAt
    };

    private async Task<(Relationship rel, DateIdea idea)> LoadAsync(Guid ideaId, Guid userId, CancellationToken ct)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var idea = await db.DateIdeas.Include(d => d.CreatedBy)
            .FirstOrDefaultAsync(d => d.Id == ideaId, ct)
            ?? throw new NotFoundException("Date idea", ideaId);
        if (idea.RelationshipId != rel.Id) throw new UnauthorizedException();
        return (rel, idea);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DateCategory? category, CancellationToken ct)
    {
        var userId = RequireUserId();
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var query = db.DateIdeas.Include(d => d.CreatedBy).Where(d => d.RelationshipId == rel.Id);
        if (category.HasValue) query = query.Where(d => d.Category == category.Value);
        var ideas = await query.OrderByDescending(d => d.CreatedAt).ToListAsync(ct);
        return Ok(ideas.Select(Map));
    }

    [HttpGet("{dateId:guid}")]
    public async Task<IActionResult> GetById(Guid dateId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, idea) = await LoadAsync(dateId, userId, ct);
        return Ok(Map(idea));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDateIdeaRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        if (string.IsNullOrWhiteSpace(req.Title)) throw new ValidationException("Title is required.");

        var idea = new DateIdea
        {
            RelationshipId = rel.Id, CreatedByUserId = userId,
            Title = req.Title.Trim(), Description = req.Description,
            Category = req.Category, Location = req.Location,
            EstimatedCost = req.EstimatedCost, DurationMinutes = req.DurationMinutes, Notes = req.Notes
        };
        db.DateIdeas.Add(idea);
        await db.SaveChangesAsync(ct);
        idea.CreatedBy = (await db.Users.FindAsync([userId], ct))!;
        return CreatedAtAction(nameof(GetById), new { dateId = idea.Id }, Map(idea));
    }

    [HttpPatch("{dateId:guid}")]
    public async Task<IActionResult> Update(Guid dateId, [FromBody] UpdateDateIdeaRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, idea) = await LoadAsync(dateId, userId, ct);
        if (req.Title != null) idea.Title = req.Title.Trim();
        if (req.Description != null) idea.Description = req.Description;
        if (req.Category.HasValue) idea.Category = req.Category.Value;
        if (req.Location != null) idea.Location = req.Location;
        if (req.EstimatedCost.HasValue) idea.EstimatedCost = req.EstimatedCost;
        if (req.DurationMinutes.HasValue) idea.DurationMinutes = req.DurationMinutes;
        if (req.Notes != null) idea.Notes = req.Notes;
        if (req.IsFavorite.HasValue) idea.IsFavorite = req.IsFavorite.Value;
        await db.SaveChangesAsync(ct);
        return Ok(Map(idea));
    }

    [HttpDelete("{dateId:guid}")]
    public async Task<IActionResult> Delete(Guid dateId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, idea) = await LoadAsync(dateId, userId, ct);
        if (idea.CreatedByUserId != userId) throw new UnauthorizedException("Only the creator can delete this.");
        db.DateIdeas.Remove(idea);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("{dateId:guid}/complete")]
    public async Task<IActionResult> Complete(Guid dateId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (_, idea) = await LoadAsync(dateId, userId, ct);
        idea.IsCompleted = true;
        idea.CompletedAt ??= DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Ok(Map(idea));
    }

    /// <summary>Get available date categories.</summary>
    [HttpGet("categories")]
    public IActionResult GetCategories() =>
        Ok(Enum.GetValues<DateCategory>().Select(c => new { value = c, name = c.ToString() }));

    /// <summary>Return a random date idea from the relationship's list, or a default suggestion.</summary>
    [HttpPost("random")]
    public async Task<IActionResult> GetRandom(CancellationToken ct)
    {
        var userId = RequireUserId();
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");

        var ideas = await db.DateIdeas.Include(d => d.CreatedBy)
            .Where(d => d.RelationshipId == rel.Id && !d.IsCompleted)
            .ToListAsync(ct);

        if (ideas.Count == 0) return NoContent();

        var random = ideas[Random.Shared.Next(ideas.Count)];
        return Ok(Map(random));
    }
}
