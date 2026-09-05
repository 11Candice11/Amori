using Amori.Api.Domain.Enums;
using Amori.Api.Features.DatePlanner.Controllers;

namespace Amori.Api.Features.DatePlanner.Services;

/// <summary>
/// Business logic for relationship date ideas and planning.
/// </summary>
public interface IDatePlannerService
{
    Task<IReadOnlyList<DateIdeaResponse>> GetAllAsync(Guid userId, DateCategory? category, CancellationToken ct = default);
    Task<DateIdeaResponse> GetByIdAsync(Guid userId, Guid dateId, CancellationToken ct = default);
    Task<DateIdeaResponse> CreateAsync(Guid userId, CreateDateIdeaRequest request, CancellationToken ct = default);
    Task<DateIdeaResponse> UpdateAsync(Guid userId, Guid dateId, UpdateDateIdeaRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid userId, Guid dateId, CancellationToken ct = default);
    Task<DateIdeaResponse> CompleteAsync(Guid userId, Guid dateId, CancellationToken ct = default);
    Task<DateIdeaResponse?> GetRandomAsync(Guid userId, CancellationToken ct = default);
}
