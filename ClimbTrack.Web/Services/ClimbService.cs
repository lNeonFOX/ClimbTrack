using ClimbTrack.Web.Data;
using ClimbTrack.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace ClimbTrack.Web.Services;

/// <summary>CRUD for climb records. Every method takes the userId explicitly and filters by it, so users can only touch their own data.</summary>
public class ClimbService(IDbContextFactory<AppDbContext> factory, TimeProvider time, ILogger<ClimbService> logger)
{
    public async Task<PagedResult<ClimbListItem>> GetPageAsync(string userId, int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        await using var db = await factory.CreateDbContextAsync(ct);
        var query = db.ClimbRecords.AsNoTracking().Where(r => r.UserId == userId);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.Date).ThenByDescending(r => r.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(r => new ClimbListItem(r.Id, r.RouteId, r.Route!.Name, r.Route.Grade, r.Date, r.Attempts, r.IsCompleted, r.Notes))
            .ToListAsync(ct);

        return new PagedResult<ClimbListItem>(items, page, pageSize, total);
    }

    public async Task<IReadOnlyList<ClimbListItem>> GetForRouteAsync(string userId, int routeId, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.ClimbRecords.AsNoTracking()
            .Where(r => r.UserId == userId && r.RouteId == routeId)
            .OrderByDescending(r => r.Date).ThenByDescending(r => r.Id)
            .Select(r => new ClimbListItem(r.Id, r.RouteId, r.Route!.Name, r.Route.Grade, r.Date, r.Attempts, r.IsCompleted, r.Notes))
            .ToListAsync(ct);
    }

    public async Task<ServiceResult> AddAsync(string userId, ClimbInput input, CancellationToken ct = default)
    {
        if (await ValidateAsync(input, ct) is { } error) return ServiceResult.Fail(error);
        try
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            db.ClimbRecords.Add(new ClimbRecord
            {
                UserId = userId,
                RouteId = input.RouteId,
                Date = input.Date,
                Attempts = input.Attempts,
                IsCompleted = input.IsCompleted,
                Notes = CleanNotes(input.Notes)
            });
            await db.SaveChangesAsync(ct);
            return ServiceResult.Ok();
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Climb could not be saved for user {UserId}", userId);
            return ServiceResult.Fail("The climb could not be saved. Please try again.");
        }
    }

    public async Task<ServiceResult> UpdateAsync(string userId, int id, ClimbInput input, CancellationToken ct = default)
    {
        if (await ValidateAsync(input, ct) is { } error) return ServiceResult.Fail(error);
        try
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            var record = await db.ClimbRecords.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId, ct);
            if (record is null) return ServiceResult.Fail("Climb record not found.");

            record.RouteId = input.RouteId;
            record.Date = input.Date;
            record.Attempts = input.Attempts;
            record.IsCompleted = input.IsCompleted;
            record.Notes = CleanNotes(input.Notes);
            await db.SaveChangesAsync(ct);
            return ServiceResult.Ok();
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Climb {ClimbId} could not be updated", id);
            return ServiceResult.Fail("The climb could not be updated. Please try again.");
        }
    }

    public async Task<ServiceResult> DeleteAsync(string userId, int id, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var deleted = await db.ClimbRecords.Where(r => r.Id == id && r.UserId == userId).ExecuteDeleteAsync(ct);
        return deleted == 0 ? ServiceResult.Fail("Climb record not found.") : ServiceResult.Ok();
    }

    private async Task<string?> ValidateAsync(ClimbInput input, CancellationToken ct)
    {
        if (InputValidator.Validate(input) is { } error) return error;

        // One day of slack so users ahead of UTC (e.g. Astana, UTC+5) can log "today".
        var latest = DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime).AddDays(1);
        if (input.Date > latest) return "The date cannot be in the future.";
        if (input.Date == default) return "Please choose a date.";

        await using var db = await factory.CreateDbContextAsync(ct);
        if (!await db.ClimbingRoutes.AnyAsync(r => r.Id == input.RouteId, ct)) return "The selected route does not exist.";
        return null;
    }

    private static string? CleanNotes(string? notes) => string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
}
