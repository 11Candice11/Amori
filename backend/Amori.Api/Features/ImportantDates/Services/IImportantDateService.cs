using Amori.Api.Features.ImportantDates.Controllers;

namespace Amori.Api.Features.ImportantDates.Services;

/// <summary>
/// Business logic for relationship important dates (birthdays, anniversaries, milestones).
/// </summary>
public interface IImportantDateService
{
    Task<IReadOnlyList<ImportantDateResponse>> GetAllAsync(Guid userId, CancellationToken ct = default);
    Task<ImportantDateResponse> GetByIdAsync(Guid userId, Guid dateId, CancellationToken ct = default);
    Task<ImportantDateResponse> CreateAsync(Guid userId, CreateImportantDateRequest request, CancellationToken ct = default);
    Task<ImportantDateResponse> UpdateAsync(Guid userId, Guid dateId, UpdateImportantDateRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid userId, Guid dateId, CancellationToken ct = default);
}
