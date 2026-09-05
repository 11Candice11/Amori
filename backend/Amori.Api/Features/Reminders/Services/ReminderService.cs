using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.Reminders.Services;

public sealed class ReminderService(AmoriDbContext db) : IReminderService
{
    private static ReminderResponse Map(Reminder r) => new()
    {
        Id = r.Id, Title = r.Title, Notes = r.Notes, Type = r.Type,
        ReminderTime = r.ReminderTime, Recurrence = r.Recurrence, OneTimeDate = r.OneTimeDate,
        IsEnabled = r.IsEnabled, LastCompletedAt = r.LastCompletedAt, SnoozeUntil = r.SnoozeUntil,
        NextOccurrenceAt = r.NextOccurrenceAt, CreatedAt = r.CreatedAt, UpdatedAt = r.UpdatedAt
    };

    private async Task<Reminder> LoadAsync(Guid userId, Guid reminderId, CancellationToken ct)
    {
        var r = await db.Reminders.FindAsync([reminderId], ct)
            ?? throw new NotFoundException("Reminder", reminderId);
        if (r.UserId != userId) throw new UnauthorizedException();
        return r;
    }

    public async Task<IReadOnlyList<ReminderResponse>> GetRemindersAsync(Guid userId, CancellationToken ct = default) =>
        await db.Reminders.Where(r => r.UserId == userId).OrderBy(r => r.ReminderTime)
            .Select(r => Map(r)).ToListAsync(ct);

    public async Task<ReminderResponse> GetReminderAsync(Guid userId, Guid reminderId, CancellationToken ct = default) =>
        Map(await LoadAsync(userId, reminderId, ct));

    public async Task<ReminderResponse> CreateReminderAsync(Guid userId, CreateReminderRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title)) throw new ValidationException("Title is required.");
        var reminder = new Reminder
        {
            UserId = userId, Title = request.Title.Trim(), Notes = request.Notes,
            Type = request.Type, ReminderTime = request.ReminderTime,
            Recurrence = request.Recurrence, OneTimeDate = request.OneTimeDate,
            IsEnabled = request.IsEnabled
        };
        db.Reminders.Add(reminder);
        await db.SaveChangesAsync(ct);
        return Map(reminder);
    }

    public async Task<ReminderResponse> UpdateReminderAsync(Guid userId, Guid reminderId, UpdateReminderRequest request, CancellationToken ct = default)
    {
        var r = await LoadAsync(userId, reminderId, ct);
        if (request.Title != null) r.Title = request.Title.Trim();
        if (request.Notes != null) r.Notes = request.Notes;
        if (request.Type.HasValue) r.Type = request.Type.Value;
        if (request.ReminderTime.HasValue) r.ReminderTime = request.ReminderTime.Value;
        if (request.Recurrence.HasValue) r.Recurrence = request.Recurrence.Value;
        if (request.OneTimeDate.HasValue) r.OneTimeDate = request.OneTimeDate.Value;
        if (request.IsEnabled.HasValue) r.IsEnabled = request.IsEnabled.Value;
        await db.SaveChangesAsync(ct);
        return Map(r);
    }

    public async Task DeleteReminderAsync(Guid userId, Guid reminderId, CancellationToken ct = default)
    {
        var r = await LoadAsync(userId, reminderId, ct);
        db.Reminders.Remove(r);
        await db.SaveChangesAsync(ct);
    }

    public async Task<ReminderResponse> CompleteAsync(Guid userId, Guid reminderId, CancellationToken ct = default)
    {
        var r = await LoadAsync(userId, reminderId, ct);
        r.LastCompletedAt = DateTime.UtcNow;
        r.SnoozeUntil = null;
        await db.SaveChangesAsync(ct);
        return Map(r);
    }

    public async Task<ReminderResponse> SkipAsync(Guid userId, Guid reminderId, CancellationToken ct = default)
    {
        var r = await LoadAsync(userId, reminderId, ct);
        r.NextOccurrenceAt = DateTime.UtcNow.Date.AddDays(1).Add(r.ReminderTime.ToTimeSpan());
        await db.SaveChangesAsync(ct);
        return Map(r);
    }

    public async Task<ReminderResponse> SnoozeAsync(Guid userId, Guid reminderId, SnoozeReminderRequest request, CancellationToken ct = default)
    {
        var r = await LoadAsync(userId, reminderId, ct);
        if (request.SnoozeMinutes < 1) throw new ValidationException("Snooze must be at least 1 minute.");
        r.SnoozeUntil = DateTime.UtcNow.AddMinutes(request.SnoozeMinutes);
        await db.SaveChangesAsync(ct);
        return Map(r);
    }

    public async Task<IReadOnlyList<ReminderResponse>> GetTodayAsync(Guid userId, CancellationToken ct = default)
    {
        var todayDate = DateOnly.FromDateTime(DateTime.UtcNow);
        return await db.Reminders
            .Where(r => r.UserId == userId && r.IsEnabled &&
                (r.Recurrence != RecurrenceType.None || r.OneTimeDate == todayDate))
            .OrderBy(r => r.ReminderTime)
            .Select(r => Map(r)).ToListAsync(ct);
    }
}
