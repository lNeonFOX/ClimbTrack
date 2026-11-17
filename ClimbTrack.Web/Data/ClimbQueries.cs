using ClimbTrack.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace ClimbTrack.Web.Data;

public static class ClimbQueries
{
    /// <summary>Loads a lightweight projection of all of one user's records (single query, no tracking).</summary>
    public static Task<List<ClimbFact>> GetFactsAsync(this AppDbContext db, string userId, CancellationToken ct = default) =>
        db.ClimbRecords.AsNoTracking()
            .Where(r => r.UserId == userId)
            .Select(r => new ClimbFact(r.RouteId, r.Route!.Grade, r.Date, r.Attempts, r.IsCompleted))
            .ToListAsync(ct);
}
