using ClimbTrack.Web.Models;
using ClimbTrack.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClimbTrack.Tests;

public class RouteServiceTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly RouteService _service;

    public RouteServiceTests() => _service = new RouteService(_db, NullLogger<RouteService>.Instance);

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task Search_FiltersByGradeAndWall()
    {
        await _db.AddRouteAsync("Slab One", 1, WallType.Slab);
        await _db.AddRouteAsync("Slab Two", 2, WallType.Slab);
        await _db.AddRouteAsync("Roof One", 1, WallType.Roof);

        var byGrade = await _service.SearchAsync(new RouteQuery(1, null, null));
        var byBoth = await _service.SearchAsync(new RouteQuery(1, WallType.Slab, null));
        var bySearch = await _service.SearchAsync(new RouteQuery(null, null, "roof"));

        Assert.Equal(2, byGrade.TotalCount);
        Assert.Equal("Slab One", Assert.Single(byBoth.Items).Name);
        Assert.Equal("Roof One", Assert.Single(bySearch.Items).Name);
    }

    [Fact]
    public async Task Search_IsPaged()
    {
        for (var i = 1; i <= 7; i++) await _db.AddRouteAsync($"Route {i:D2}", 0);

        var page2 = await _service.SearchAsync(new RouteQuery(null, null, null, Page: 2, PageSize: 3));

        Assert.Equal(7, page2.TotalCount);
        Assert.Equal(3, page2.TotalPages);
        Assert.Equal(["Route 04", "Route 05", "Route 06"], page2.Items.Select(r => r.Name));
    }

    [Fact]
    public async Task Create_RejectsInvalidGrade_AndDuplicateNames()
    {
        Assert.False((await _service.CreateAsync(new RouteInput { Name = "X", Grade = 11 })).Success);

        Assert.True((await _service.CreateAsync(new RouteInput { Name = "Unique", Grade = 3 })).Success);
        Assert.False((await _service.CreateAsync(new RouteInput { Name = "Unique", Grade = 4 })).Success);
    }

    [Fact]
    public async Task Delete_IsBlockedWhenClimbsExist()
    {
        var user = await _db.AddUserAsync("u1");
        var route = await _db.AddRouteAsync("Used", 2);
        var climbs = new ClimbService(_db, TimeProvider.System, NullLogger<ClimbService>.Instance);
        await climbs.AddAsync(user, new ClimbInput { RouteId = route.Id, Date = DateOnly.FromDateTime(DateTime.UtcNow), Attempts = 1 });

        Assert.False((await _service.DeleteAsync(route.Id)).Success);

        var unused = await _db.AddRouteAsync("Unused", 2);
        Assert.True((await _service.DeleteAsync(unused.Id)).Success);
    }
}
