using ClimbTrack.Web.Models;
using ClimbTrack.Web.Services;

namespace ClimbTrack.Tests;

public class RecommendationEngineTests
{
    private static ClimbingRoute R(int id, int grade, string name) =>
        new() { Id = id, Grade = grade, Name = name, WallType = WallType.Vertical };

    private static ClimbFact Sent(int routeId, int grade) =>
        new(routeId, grade, new DateOnly(2026, 11, 1), 1, true);

    private static readonly ClimbingRoute[] Routes =
        [R(1, 0, "A"), R(2, 1, "B"), R(3, 1, "A2"), R(4, 2, "C"), R(5, 3, "D")];

    [Fact]
    public void EmptyHistory_RecommendsV0AndV1_OrderedByGradeThenName()
    {
        var result = RecommendationEngine.Recommend([], Routes);
        Assert.Equal([1, 3, 2], result.Select(r => r.Id));
    }

    [Fact]
    public void CompletedRoutes_AreExcluded()
    {
        var result = RecommendationEngine.Recommend([Sent(1, 0)], Routes);
        Assert.DoesNotContain(result, r => r.Id == 1);
        Assert.Equal([3, 2], result.Select(r => r.Id));
    }

    [Fact]
    public void OnlyCurrentAndNextGrade_AreSuggested()
    {
        // Highest sent grade is V1, so only V1 and V2 are allowed (V0 and V3 are not).
        var result = RecommendationEngine.Recommend([Sent(2, 1)], Routes);
        Assert.Equal([3, 4], result.Select(r => r.Id));
    }

    [Fact]
    public void FailedAttempts_DoNotRaiseLevel()
    {
        var facts = new[] { new ClimbFact(5, 3, new DateOnly(2026, 11, 1), 5, false) };
        Assert.Equal(0, RecommendationEngine.GetLevel(facts));
    }

    [Fact]
    public void Result_IsLimitedToFive()
    {
        var many = Enumerable.Range(1, 12).Select(i => R(i, 0, $"R{i:D2}")).ToList();
        Assert.Equal(5, RecommendationEngine.Recommend([], many).Count);
    }

    [Fact]
    public void StartGrade_IsUsedOnlyWhenNothingCompleted()
    {
        Assert.Equal([4, 5], RecommendationEngine.Recommend([], Routes, startGrade: 2).Select(r => r.Id));
        Assert.Equal(1, RecommendationEngine.GetLevel([Sent(2, 1)], startGrade: 7));
    }
}
