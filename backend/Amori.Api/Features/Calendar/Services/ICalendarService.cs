using Amori.Api.Features.Calendar.Controllers;

namespace Amori.Api.Features.Calendar.Services;

/// <summary>
/// Business logic for shared relationship calendar events.
/// </summary>
public interface ICalendarService
{
    Task<IReadOnlyList<CalendarEventResponse>> GetAllAsync(Guid userId, int? year, int? month, CancellationToken ct = default);
    Task<CalendarEventResponse> GetByIdAsync(Guid userId, Guid eventId, CancellationToken ct = default);
    Task<CalendarEventResponse> CreateAsync(Guid userId, CreateCalendarEventRequest request, CancellationToken ct = default);
    Task<CalendarEventResponse> UpdateAsync(Guid userId, Guid eventId, UpdateCalendarEventRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid userId, Guid eventId, CancellationToken ct = default);
    Task<CalendarEventResponse> CompleteAsync(Guid userId, Guid eventId, CancellationToken ct = default);
}
