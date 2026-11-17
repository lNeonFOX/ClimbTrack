using ClimbTrack.Web.Data;
using ClimbTrack.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace ClimbTrack.Web.Services;

/// <summary>Deterministic, rule-based recommendation logic. Isolated from EF Core so it can be unit tested.</summary>
public static class RecommendationEngine
{
    public const int MaxResults = 5;

    /// <summary>Highest successfully completed grade, or <paramref name="startGrade"/> when nothing has been completed yet.</summary>
    public static int GetLevel(IEnumerable<ClimbFact> facts, int startGrade = 0) =>
        Math.Clamp(facts.Where(f => f.IsCompleted).Select(f => f.Grade).DefaultIfEmpty(startGrade).Max(), Grades.Min, Grades.Max);

    public static IReadOnlyList<ClimbingRoute> Recommend(
        IEnumerable<ClimbFact> facts, IEnumerable<ClimbingRoute> routes, int startGrade = 0, int take = MaxResults)
    {
        var history = facts.ToList();
        var completedIds = history.Where(f => f.IsCompleted).Select(f => f.RouteId).ToHashSet();
        var level = GetLevel(history, startGrade);

        return routes
            .Where(r => !completedIds.Contains(r.Id))
            .Where(r => r.Grade >= level && r.Grade <= level + 1)
            .OrderBy(r => r.Grade).ThenBy(r => r.Name)
            .Take(Math.Clamp(take, 1, MaxResults))
            .ToList();
    }
}

public class RecommendationService(IDbContextFactory<AppDbContext> factory)
{
    public async Task<RecommendationResult> GetAsync(string userId, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);

        var facts = await db.GetFactsAsync(userId, ct);
        var preferred = await db.Users.Where(u => u.Id == userId).Select(u => u.PreferredGrade).FirstOrDefaultAsync(ct);

        var level = RecommendationEngine.GetLevel(facts, preferred);
        var completedIds = facts.Where(f => f.IsCompleted).Select(f => f.RouteId).ToList();

        // Narrow the candidate set in SQL, then apply the pure engine for the final rules/ordering.
        var candidates = await db.ClimbingRoutes.AsNoTracking()
            .Where(r => r.Grade >= level && r.Grade <= level + 1 && !completedIds.Contains(r.Id))
            .ToListAsync(ct);

        return new RecommendationResult(level, RecommendationEngine.Recommend(facts, candidates, preferred));
    }
}
