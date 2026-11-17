using ClimbTrack.Web.Models;
using ClimbTrack.Web.Services;

namespace ClimbTrack.Tests;

public class StatisticsCalculatorTests
{
    [Fact]
    public void EmptyHistory_ReturnsZeros()
    {
        var stats = StatisticsCalculator.Calculate([]);
        Assert.Equal(0, stats.TotalRecords);
        Assert.Null(stats.BestGrade);
        Assert.Equal(0, stats.SuccessRate);
        Assert.Equal(11, stats.GradeDistribution.Count);
    }

    [Fact]
    public void Calculate_ComputesAllMetrics()
    {
        var d1 = new DateOnly(2026, 11, 1);
        var d2 = new DateOnly(2026, 11, 3);
        var d3 = new DateOnly(2026, 11, 5);
        ClimbFact[] facts =
        [
            new(1, 2, d1, 3, true),
            new(1, 2, d1, 1, true),   // same route sent twice: counts once
            new(2, 4, d2, 5, false),  // hard route, not completed: no effect on best grade
            new(3, 3, d3, 2, true)
        ];

        var stats = StatisticsCalculator.Calculate(facts);

        Assert.Equal(4, stats.TotalRecords);
        Assert.Equal(2, stats.CompletedRoutes);
        Assert.Equal(3, stats.BestGrade);
        Assert.Equal(3, stats.SessionCount);
        Assert.Equal(11, stats.TotalAttempts);
        Assert.Equal(0.75, stats.SuccessRate, 3);
        Assert.Equal(1, stats.GradeDistribution[2]);
        Assert.Equal(1, stats.GradeDistribution[3]);
        Assert.Equal(0, stats.GradeDistribution[4]);
    }
}
