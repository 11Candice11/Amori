using Amori.Api.Domain.Enums;
using Amori.Api.Features.RelationshipTickets.Controllers;

namespace Amori.Api.Features.RelationshipTickets.Services;

/// <summary>
/// Business logic for the Let's Chat relationship communication ticket system.
/// </summary>
public interface IRelationshipTicketService
{
    Task<IReadOnlyList<TicketDto>> GetAllAsync(Guid userId, TicketStatus? status, CancellationToken ct = default);
    Task<TicketDto> GetByIdAsync(Guid userId, Guid ticketId, CancellationToken ct = default);
    Task<TicketDto> CreateAsync(Guid userId, CreateTicketRequest request, CancellationToken ct = default);
    Task<TicketDto> UpdateAsync(Guid userId, Guid ticketId, UpdateTicketRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid userId, Guid ticketId, CancellationToken ct = default);
    Task<TicketDto> AcknowledgeAsync(Guid userId, Guid ticketId, CancellationToken ct = default);
    Task<TicketDto> AssignAsync(Guid userId, Guid ticketId, AssignTicketRequest request, CancellationToken ct = default);
    Task<TicketDto> RespondAsync(Guid userId, Guid ticketId, AddTicketResponseRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<TicketResponseDto>> GetResponsesAsync(Guid userId, Guid ticketId, CancellationToken ct = default);
    Task<TicketDto> SetStatusAsync(Guid userId, Guid ticketId, TicketStatus status, CancellationToken ct = default);
    Task<TicketDto> ResolveAsync(Guid userId, Guid ticketId, CancellationToken ct = default);
    Task<TicketDto> ReopenAsync(Guid userId, Guid ticketId, CancellationToken ct = default);
}
