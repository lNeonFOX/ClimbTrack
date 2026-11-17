using ClimbTrack.Web.Data;
using ClimbTrack.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace ClimbTrack.Web.Services;

public class RouteService(IDbContextFactory<AppDbContext> factory, ILogger<RouteService> logger)
{
    public const int MaxPageSize = 50;

    public async Task<PagedResult<ClimbingRoute>> SearchAsync(RouteQuery q, CancellationToken ct = default)
    {
        var page = Math.Max(1, q.Page);
        var size = Math.Clamp(q.PageSize, 1, MaxPageSize);

        await using var db = await factory.CreateDbContextAsync(ct);
        IQueryable<ClimbingRoute> query = db.ClimbingRoutes.AsNoTracking();

        if (q.Grade is { } grade) query = query.Where(r => r.Grade == grade);
        if (q.Wall is { } wall) query = query.Where(r => r.WallType == wall);
        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var pattern = $"%{q.Search.Trim()}%";
            query = query.Where(r => EF.Functions.Like(r.Name, pattern));
        }

        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(r => r.Grade).ThenBy(r => r.Name)
            .Skip((page - 1) * size).Take(size)
            .ToListAsync(ct);

        return new PagedResult<ClimbingRoute>(items, page, size, total);
    }

    public async Task<ClimbingRoute?> GetAsync(int id, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.ClimbingRoutes.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    /// <summary>All routes ordered by grade, for dropdowns.</summary>
    public async Task<IReadOnlyList<ClimbingRoute>> GetAllAsync(CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.ClimbingRoutes.AsNoTracking().OrderBy(r => r.Grade).ThenBy(r => r.Name).ToListAsync(ct);
    }

    public async Task<ServiceResult> CreateAsync(RouteInput input, CancellationToken ct = default)
    {
        if (InputValidator.Validate(input) is { } error) return ServiceResult.Fail(error);
        try
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            db.ClimbingRoutes.Add(Map(new ClimbingRoute(), input));
            await db.SaveChangesAsync(ct);
            return ServiceResult.Ok();
        }
        catch (DbUpdateException ex)
        {
            logger.LogWarning(ex, "Route could not be created");
            return ServiceResult.Fail("A route with this name already exists, or the data could not be saved.");
        }
    }

    public async Task<ServiceResult> UpdateAsync(int id, RouteInput input, CancellationToken ct = default)
    {
        if (InputValidator.Validate(input) is { } error) return ServiceResult.Fail(error);
        try
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            var route = await db.ClimbingRoutes.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (route is null) return ServiceResult.Fail("Route not found.");
            Map(route, input);
            await db.SaveChangesAsync(ct);
            return ServiceResult.Ok();
        }
        catch (DbUpdateException ex)
        {
            logger.LogWarning(ex, "Route {RouteId} could not be updated", id);
            return ServiceResult.Fail("A route with this name already exists, or the data could not be saved.");
        }
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        if (await db.ClimbRecords.AnyAsync(r => r.RouteId == id, ct))
            return ServiceResult.Fail("This route has recorded climbs and cannot be deleted.");

        var deleted = await db.ClimbingRoutes.Where(r => r.Id == id).ExecuteDeleteAsync(ct);
        return deleted == 0 ? ServiceResult.Fail("Route not found.") : ServiceResult.Ok();
    }

    private static ClimbingRoute Map(ClimbingRoute route, RouteInput input)
    {
        route.Name = input.Name.Trim();
        route.Grade = input.Grade;
        route.WallType = input.WallType;
        route.Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim();
        route.ImageUrl = string.IsNullOrWhiteSpace(input.ImageUrl) ? null : input.ImageUrl.Trim();
        return route;
    }
}
