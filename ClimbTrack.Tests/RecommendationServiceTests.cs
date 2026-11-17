using ClimbTrack.Web.Models;
using ClimbTrack.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClimbTrack.Tests;

public class RecommendationServiceTests : IDisposable
{
    private readonly TestDb _db = new();

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task Service_UsesOnlyTheCurrentUsersHistory()
    {
        var me = await _db.AddUserAsync("me");
        var other = await _db.AddUserAsync("other");
        var easy = await _db.AddRouteAsync("Easy", 0);
        var mid = await _db.AddRouteAsync("Mid", 3);
        await _db.AddRouteAsync("Next", 4);

        var climbs = new ClimbService(_db, TimeProvider.System, NullLogger<ClimbService>.Instance);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await climbs.AddAsync(me, new ClimbInput { RouteId = easy.Id, Date = today, IsCompleted = true });
        await climbs.AddAsync(other, new ClimbInput { RouteId = mid.Id, Date = today, IsCompleted = true });

        var result = await new RecommendationService(_db).GetAsync(me);

        Assert.Equal(0, result.Level);                 // the other user's V3 send must not leak in
        Assert.DoesNotContain(result.Routes, r => r.Name == "Easy");
        Assert.DoesNotContain(result.Routes, r => r.Grade > 1);
    }
}
