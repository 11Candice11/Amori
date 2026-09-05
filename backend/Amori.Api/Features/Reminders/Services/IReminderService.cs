using Amori.Api.Features.Reminders;

namespace Amori.Api.Features.Reminders.Services;

/// <summary>
/// Business logic for user reminders (medication, water, food, custom).
/// </summary>
public interface IReminderService
{
    Task<IReadOnlyList<ReminderResponse>> GetRemindersAsync(Guid userId, CancellationToken ct = default);
    Task<ReminderResponse> GetReminderAsync(Guid userId, Guid reminderId, CancellationToken ct = default);
    Task<ReminderResponse> CreateReminderAsync(Guid userId, CreateReminderRequest request, CancellationToken ct = default);
    Task<ReminderResponse> UpdateReminderAsync(Guid userId, Guid reminderId, UpdateReminderRequest request, CancellationToken ct = default);
    Task DeleteReminderAsync(Guid userId, Guid reminderId, CancellationToken ct = default);
    Task<ReminderResponse> CompleteAsync(Guid userId, Guid reminderId, CancellationToken ct = default);
    Task<ReminderResponse> SkipAsync(Guid userId, Guid reminderId, CancellationToken ct = default);
    Task<ReminderResponse> SnoozeAsync(Guid userId, Guid reminderId, SnoozeReminderRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ReminderResponse>> GetTodayAsync(Guid userId, CancellationToken ct = default);
}
