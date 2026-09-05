using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Features.ImportantDates.Controllers;
using Amori.Api.Infrastructure.Relationships;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.ImportantDates.Services;

public sealed class ImportantDateService(
    AmoriDbContext db,
    IRelationshipAccessService relAccess) : IImportantDateService
{
    private static ImportantDateResponse Map(ImportantDate d)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        int? daysUntil = null;

        if (d.IsRecurring)
        {
            var next = new DateOnly(today.Year, d.Date.Month, d.Date.Day);
            if (next < today) next = next.AddYears(1);
            daysUntil = next.DayNumber - today.DayNumber;
        }
        else if (d.Date >= today)
        {
            daysUntil = d.Date.DayNumber - today.DayNumber;
        }

        return new ImportantDateResponse
        {
            Id = d.Id, RelationshipId = d.RelationshipId,
            CreatedByUserId = d.CreatedByUserId, CreatedByName = d.CreatedBy?.DisplayName ?? string.Empty,
            Name = d.Name, Date = d.Date, IsRecurring = d.IsRecurring,
            ReminderEnabled = d.ReminderEnabled, ReminderDaysBefore = d.ReminderDaysBefore,
            Notes = d.Notes, ImageKey = d.ImageKey, DaysUntilNext = daysUntil,
            CreatedAt = d.CreatedAt, UpdatedAt = d.UpdatedAt
        };
    }

    private async Task<(Guid relId, ImportantDate date)> LoadAsync(Guid userId, Guid dateId, CancellationToken ct)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        var date = await db.ImportantDates.Include(d => d.CreatedBy)
            .FirstOrDefaultAsync(d => d.Id == dateId, ct)
            ?? throw new NotFoundException("Important date", dateId);
        if (date.RelationshipId != rel.Id) throw new UnauthorizedException();
        return (rel.Id, date);
    }

    public async Task<IReadOnlyList<ImportantDateResponse>> GetAllAsync(Guid userId, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        return await db.ImportantDates.Include(d => d.CreatedBy)
            .Where(d => d.RelationshipId == rel.Id)
            .OrderBy(d => d.Date)
            .Select(d => Map(d)).ToListAsync(ct);
    }

    public async Task<ImportantDateResponse> GetByIdAsync(Guid userId, Guid dateId, CancellationToken ct = default)
    {
        var (_, date) = await LoadAsync(userId, dateId, ct);
        return Map(date);
    }

    public async Task<ImportantDateResponse> CreateAsync(Guid userId, CreateImportantDateRequest request, CancellationToken ct = default)
    {
        var rel = await relAccess.GetUserRelationshipAsync(userId)
            ?? throw new NotFoundException("You are not a member of any relationship.");
        if (string.IsNullOrWhiteSpace(request.Name)) throw new ValidationException("Name is required.");

        var date = new ImportantDate
        {
            RelationshipId = rel.Id, CreatedByUserId = userId,
            Name = request.Name.Trim(), Date = request.Date, IsRecurring = request.IsRecurring,
            ReminderEnabled = request.ReminderEnabled, ReminderDaysBefore = request.ReminderDaysBefore,
            Notes = request.Notes, ImageKey = request.ImageKey
        };
        db.ImportantDates.Add(date);
        await db.SaveChangesAsync(ct);
        date.CreatedBy = (await db.Users.FindAsync([userId], ct))!;
        return Map(date);
    }

    public async Task<ImportantDateResponse> UpdateAsync(Guid userId, Guid dateId, UpdateImportantDateRequest request, CancellationToken ct = default)
    {
        var (_, date) = await LoadAsync(userId, dateId, ct);
        if (date.CreatedByUserId != userId) throw new UnauthorizedException("Only the creator can update this.");
        if (request.Name != null) date.Name = request.Name.Trim();
        if (request.Date.HasValue) date.Date = request.Date.Value;
        if (request.IsRecurring.HasValue) date.IsRecurring = request.IsRecurring.Value;
        if (request.ReminderEnabled.HasValue) date.ReminderEnabled = request.ReminderEnabled.Value;
        if (request.ReminderDaysBefore.HasValue) date.ReminderDaysBefore = request.ReminderDaysBefore;
        if (request.Notes != null) date.Notes = request.Notes;
        if (request.ImageKey != null) date.ImageKey = request.ImageKey;
        await db.SaveChangesAsync(ct);
        return Map(date);
    }

    public async Task DeleteAsync(Guid userId, Guid dateId, CancellationToken ct = default)
    {
        var (_, date) = await LoadAsync(userId, dateId, ct);
        if (date.CreatedByUserId != userId) throw new UnauthorizedException("Only the creator can delete this.");
        db.ImportantDates.Remove(date);
        await db.SaveChangesAsync(ct);
    }
}
