using ClimbTrack.Web.Data;
using ClimbTrack.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace ClimbTrack.Web.Services;

/// <summary>Pure calculation (no I/O) so it is trivially unit-testable. Metrics are derived from records, never stored as counters.</summary>
public static class StatisticsCalculator
{
    public static DashboardStats Calculate(IReadOnlyCollection<ClimbFact> facts)
    {
        // A route counts as "completed" once, no matter how many times it was sent.
        var completedRoutes = facts.Where(f => f.IsCompleted)
            .GroupBy(f => f.RouteId).Select(g => g.First()).ToList();

        int? best = completedRoutes.Count == 0 ? null : completedRoutes.Max(f => f.Grade);

        var distribution = Enumerable.Range(Grades.Min, Grades.Max - Grades.Min + 1)
            .ToDictionary(g => g, g => completedRoutes.Count(f => f.Grade == g));

        var successRate = facts.Count == 0 ? 0 : (double)facts.Count(f => f.IsCompleted) / facts.Count;

        return new DashboardStats(
            TotalRecords: facts.Count,
            CompletedRoutes: completedRoutes.Count,
            BestGrade: best,
            SessionCount: facts.Select(f => f.Date).Distinct().Count(),
            TotalAttempts: facts.Sum(f => f.Attempts),
            SuccessRate: successRate,
            GradeDistribution: distribution);
    }
}

public class StatisticsService(IDbContextFactory<AppDbContext> factory)
{
    public async Task<DashboardStats> GetAsync(string userId, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var facts = await db.GetFactsAsync(userId, ct);
        return StatisticsCalculator.Calculate(facts);
    }
}
