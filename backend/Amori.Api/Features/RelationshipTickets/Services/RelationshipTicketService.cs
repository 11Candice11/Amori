using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Domain.Enums;
using Amori.Api.Features.RelationshipTickets.Controllers;
using Amori.Api.Infrastructure.Relationships;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.RelationshipTickets.Services;

public sealed class RelationshipTicketService(
    AmoriDbContext db,
    IRelationshipAccessService relAccess) : IRelationshipTicketService
{
    private static TicketDto Map(RelationshipTicket t) => new()
    {
        Id = t.Id, RelationshipId = t.RelationshipId,
        CreatedByUserId = t.CreatedByUserId, CreatedByName = t.CreatedBy?.DisplayName ?? string.Empty,
        AssignedToUserId = t.AssignedToUserId, AssignedToName = t.AssignedTo?.DisplayName,
        Subject = t.Subject, Category = t.Category, Severity = t.Severity, Status = t.Status,
        Description = t.Description, Feelings = t.Feelings, WhatHappened = t.WhatHappened,
        WhatINeed = t.WhatINeed, WhatIPreferInFuture = t.WhatIPreferInFuture,
        AdditionalNotes = t.AdditionalNotes,
        CreatedAt = t.CreatedAt, UpdatedAt = t.UpdatedAt, ResolvedAt = t.ResolvedAt,
        Responses = t.Responses.Select(r => new TicketResponseDto
        {
            Id = r.Id, TicketId = r.TicketId,
            RespondedByUserId = r.RespondedByUserId,
            RespondedByName = r.RespondedBy?.DisplayName ?? string.Empty,
            Content = r.Content, CreatedAt = r.CreatedAt, UpdatedAt = r.UpdatedAt
        }).OrderBy(r => r.CreatedAt).ToList()
    };

    private async Task<(Guid relId, RelationshipTicket ticket)> LoadAsync(Guid userId, Guid ticketId, CancellationToken ct)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var ticket = await db.RelationshipTickets
            .Include(t => t.CreatedBy).Include(t => t.AssignedTo)
            .Include(t => t.Responses).ThenInclude(r => r.RespondedBy)
            .FirstOrDefaultAsync(t => t.Id == ticketId, ct)
            ?? throw new NotFoundException("Ticket", ticketId);
        if (ticket.RelationshipId != rel.Id) throw new UnauthorizedException();
        return (rel.Id, ticket);
    }

    public async Task<IReadOnlyList<TicketDto>> GetAllAsync(Guid userId, TicketStatus? status, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var query = db.RelationshipTickets
            .Include(t => t.CreatedBy).Include(t => t.AssignedTo)
            .Include(t => t.Responses).ThenInclude(r => r.RespondedBy)
            .Where(t => t.RelationshipId == rel.Id);
        if (status.HasValue) query = query.Where(t => t.Status == status.Value);
        return await query.OrderByDescending(t => t.CreatedAt).Select(t => Map(t)).ToListAsync(ct);
    }

    public async Task<TicketDto> GetByIdAsync(Guid userId, Guid ticketId, CancellationToken ct = default)
    {
        var (_, ticket) = await LoadAsync(userId, ticketId, ct);
        return Map(ticket);
    }

    public async Task<TicketDto> CreateAsync(Guid userId, CreateTicketRequest request, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        if (string.IsNullOrWhiteSpace(request.Subject)) throw new ValidationException("Subject is required.");

        var ticket = new RelationshipTicket
        {
            RelationshipId = rel.Id, CreatedByUserId = userId,
            Subject = request.Subject.Trim(), Category = request.Category,
            Severity = request.Severity, Status = TicketStatus.Open,
            Description = request.Description, Feelings = request.Feelings,
            WhatHappened = request.WhatHappened, WhatINeed = request.WhatINeed,
            WhatIPreferInFuture = request.WhatIPreferInFuture, AdditionalNotes = request.AdditionalNotes
        };
        db.RelationshipTickets.Add(ticket);
        await db.SaveChangesAsync(ct);
        ticket.CreatedBy = (await db.Users.FindAsync([userId], ct))!;
        return Map(ticket);
    }

    public async Task<TicketDto> UpdateAsync(Guid userId, Guid ticketId, UpdateTicketRequest request, CancellationToken ct = default)
    {
        var (_, ticket) = await LoadAsync(userId, ticketId, ct);
        if (ticket.CreatedByUserId != userId) throw new UnauthorizedException("Only the creator can update this ticket.");
        if (request.Subject != null) ticket.Subject = request.Subject.Trim();
        if (request.Category.HasValue) ticket.Category = request.Category.Value;
        if (request.Severity.HasValue) ticket.Severity = request.Severity.Value;
        if (request.Description != null) ticket.Description = request.Description;
        if (request.Feelings != null) ticket.Feelings = request.Feelings;
        if (request.WhatHappened != null) ticket.WhatHappened = request.WhatHappened;
        if (request.WhatINeed != null) ticket.WhatINeed = request.WhatINeed;
        if (request.WhatIPreferInFuture != null) ticket.WhatIPreferInFuture = request.WhatIPreferInFuture;
        if (request.AdditionalNotes != null) ticket.AdditionalNotes = request.AdditionalNotes;
        await db.SaveChangesAsync(ct);
        return Map(ticket);
    }

    public async Task DeleteAsync(Guid userId, Guid ticketId, CancellationToken ct = default)
    {
        var (_, ticket) = await LoadAsync(userId, ticketId, ct);
        if (ticket.CreatedByUserId != userId) throw new UnauthorizedException("Only the creator can delete this ticket.");
        db.RelationshipTickets.Remove(ticket);
        await db.SaveChangesAsync(ct);
    }

    public async Task<TicketDto> AcknowledgeAsync(Guid userId, Guid ticketId, CancellationToken ct = default)
    {
        var (_, ticket) = await LoadAsync(userId, ticketId, ct);
        ticket.Status = TicketStatus.Acknowledged;
        await db.SaveChangesAsync(ct);
        return Map(ticket);
    }

    public async Task<TicketDto> AssignAsync(Guid userId, Guid ticketId, AssignTicketRequest request, CancellationToken ct = default)
    {
        var (relId, ticket) = await LoadAsync(userId, ticketId, ct);
        var isMember = await relAccess.IsUserMemberAsync(request.AssignedToUserId, relId);
        if (!isMember) throw new UnauthorizedException("Can only assign to a member of your relationship.");
        ticket.AssignedToUserId = request.AssignedToUserId;
        ticket.Status = TicketStatus.InProgress;
        ticket.AssignedTo = await db.Users.FindAsync([request.AssignedToUserId], ct);
        await db.SaveChangesAsync(ct);
        return Map(ticket);
    }

    public async Task<TicketDto> RespondAsync(Guid userId, Guid ticketId, AddTicketResponseRequest request, CancellationToken ct = default)
    {
        var (_, ticket) = await LoadAsync(userId, ticketId, ct);
        if (string.IsNullOrWhiteSpace(request.Content)) throw new ValidationException("Content is required.");
        var response = new TicketResponse { TicketId = ticket.Id, RespondedByUserId = userId, Content = request.Content };
        db.TicketResponses.Add(response);
        if (ticket.Status == TicketStatus.Open) ticket.Status = TicketStatus.InProgress;
        await db.SaveChangesAsync(ct);
        return Map(ticket);
    }

    public async Task<IReadOnlyList<TicketResponseDto>> GetResponsesAsync(Guid userId, Guid ticketId, CancellationToken ct = default)
    {
        var (_, ticket) = await LoadAsync(userId, ticketId, ct);
        return ticket.Responses.Select(r => new TicketResponseDto
        {
            Id = r.Id, TicketId = r.TicketId, RespondedByUserId = r.RespondedByUserId,
            RespondedByName = r.RespondedBy?.DisplayName ?? string.Empty,
            Content = r.Content, CreatedAt = r.CreatedAt, UpdatedAt = r.UpdatedAt
        }).OrderBy(r => r.CreatedAt).ToList();
    }

    public async Task<TicketDto> SetStatusAsync(Guid userId, Guid ticketId, TicketStatus status, CancellationToken ct = default)
    {
        var (_, ticket) = await LoadAsync(userId, ticketId, ct);
        ticket.Status = status;
        await db.SaveChangesAsync(ct);
        return Map(ticket);
    }

    public async Task<TicketDto> ResolveAsync(Guid userId, Guid ticketId, CancellationToken ct = default)
    {
        var (_, ticket) = await LoadAsync(userId, ticketId, ct);
        ticket.Status = TicketStatus.Resolved;
        ticket.ResolvedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Map(ticket);
    }

    public async Task<TicketDto> ReopenAsync(Guid userId, Guid ticketId, CancellationToken ct = default)
    {
        var (_, ticket) = await LoadAsync(userId, ticketId, ct);
        ticket.Status = TicketStatus.Open;
        ticket.ResolvedAt = null;
        await db.SaveChangesAsync(ct);
        return Map(ticket);
    }
}
